using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using System.IO;
using System.IO.Compression;
using Ara3D.BimOpenSchema;
using Ara3D.BimOpenSchema.IO;
using Ara3D.Collections;
using Ara3D.Extras;
using Ara3D.Logging;
using Autodesk.Revit.DB;
using Parquet;
using ZstdSharp.Unsafe;
using Document = Autodesk.Revit.DB.Document;
using FilePath = Ara3D.Utils.FilePath;
using Material = Ara3D.Models.Material;

namespace Ara3D.Bowerbird.RevitSamples
{
    public static class BimOpenSchemaUtils
    {
        public static bool IsVis(Geometry g)
        {
            var demoPhase = g.Element.DemolishedPhaseId;
            if (demoPhase != ElementId.InvalidElementId)
                return false;
            var cat = g.Element.Category;
            if (cat == null)
                return false;
            if (cat.CategoryType != CategoryType.Model)
                return false;
            if (g.Element is SpatialElement)
                return false;
            return true;
        }

        public static void BuildGeometry(this BosDocumentBuilder bdb, BimGeometryBuilder builder)
        {
            var meshGatherer = new BosMeshGatherer(bdb);

            var meshOffset = builder.Meshes.Count;
            builder.Meshes.AddRange(meshGatherer.MeshList.Select(m => m.ToAra3D()));

            var geometries = meshGatherer.Geometries.Where(IsVis).ToList();
            foreach (var g in geometries)
            {
                if (g == null)
                    continue;

                var defaultMatIndex = builder.AddMaterial(g.DefaultMaterial ?? Material.Default);
                foreach (var part in g.Parts)
                {
                    var matIndex = part.Material == null 
                        ? defaultMatIndex 
                        : builder.AddMaterial(part.Material.Value);

                    var transformIndex = builder.AddTransform(part.Transform.ToAra3D());
                    builder.AddInstance((int)g.EntityIndex, matIndex, part.MeshIndex + meshOffset, transformIndex);
                }
            }
        }

        public static FilePath ExportBimOpenSchema(this Document currentDoc, BimOpenSchemaExportSettings settings, ILogger logger)
        {
            // TODO: move all of this into the Revit Builder 

            logger.Log($"Exporting BIM Open Schema Parquet Files");

            var useView = settings.UseCurrentView && settings.IncludeGeometry;
            
            var view = useView 
                ? currentDoc.ActiveView
                : null;

            if (useView)
            {
                // TODO: take it out of the UI
                throw new Exception("View based export not currently supported");
                //if (view == null) throw new Exception("No default view present, can't export");
                //logger.Log($"Exporting view {view.Name}");
            }

            var options = useView ?
                new Options()
                {
                    ComputeReferences = true,
                    View = view,
                }
                : new Options()
                {
                    ComputeReferences = true,
                    DetailLevel = (ViewDetailLevel)settings.DetailLevel,
                };

            logger.Log($"Create Revit builder and gather links");
            var bosRevitBuilder = new BosRevitBuilder(options, settings);
            var context = new BosDocumentContext(currentDoc);
            var docs = context.GatherLinkedDocuments();
            if (!settings.IncludeLinks)
                docs = [context];

            logger.Log($"Parse core documents {docs.Count}");
            var docBuilders = new List<BosDocumentBuilder>();
            for (var i = 0; i < docs.Count; i++)
            {
                var localContext = docs[i];
                localContext.RetrieveElementIds();
                logger.Log($"Parsing document {i} with {localContext.ElementIds.Count} non-type elements");
                var localDocBuilder = new BosDocumentBuilder(bosRevitBuilder, docs[i]);
                docBuilders.Add(localDocBuilder);
                localDocBuilder.ProcessDocument();
            }

            logger.Log($"Creating BIM data");
            var bimData = bosRevitBuilder.BimDataBuilder;
            var dataSet = bimData.ToDataSet();

            var inputFile = new FilePath(currentDoc.PathName);
            var fp = inputFile.ChangeDirectoryAndExt(settings.Folder, "bos");

            logger.Log($"Creating FileStream");
            var fs = new FileStream(fp, FileMode.Create, FileAccess.Write, FileShare.None);

            logger.Log($"Creating Zip Archive");
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false);

            var parquetCompressionMethod = CompressionMethod.Brotli;
            var parquetCompressionLevel = CompressionLevel.Optimal;
            var zipCompressionLevel = CompressionLevel.Fastest;

            logger.Log($"Writing non-geometry data to Zip file {fp}");
            dataSet.WriteParquetToZip(zip,
            parquetCompressionMethod,
                    parquetCompressionLevel,
                    zipCompressionLevel);

            if (settings.IncludeGeometry)
            {
                logger.Log($"Creating BIM Geometry");

                var bgb = new BimGeometryBuilder();
                foreach (var docBuilder in docBuilders)
                {
                    BuildGeometry(docBuilder, bgb);
                }
                
                logger.Log($"Writing BIM geometry");
                var bg = bgb.BuildModel();
                bg.WriteParquetToZip(zip, parquetCompressionMethod, parquetCompressionLevel, zipCompressionLevel);
            }

            logger.Log($"Finished writing to {fp}");
            return fp;
        }
    }
}
