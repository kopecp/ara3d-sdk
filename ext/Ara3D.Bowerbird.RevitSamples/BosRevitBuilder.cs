using System.Collections.Generic;
using Ara3D.BimOpenSchema;
using Autodesk.Revit.DB;

namespace Ara3D.Bowerbird.RevitSamples;

public class BosRevitBuilder
{
    public BosRevitBuilder(Options options, BimOpenSchemaExportSettings settings)
    {
        BimDataBuilder = new BimDataBuilder();
        Options = options;
        Settings = settings;
        
        // TEMP
        BimDataBuilder.Manifest = new();
        
        BimDataBuilder.Manifest.GeneratorApplication = "Revit 2025 BIM Open Schema Parquet Exporter";
        BimDataBuilder.Manifest.GeneratorVersion = "";
        BimDataBuilder.Manifest.ExportOptions = Settings;
        CreateCommonDescriptors();
    }

    public void CreateCommonDescriptors()
    {
        foreach (var p in CommonRevitParameters.GetParameters())
        {
            var desc = BimDataBuilder.AddDescriptor(p.Name, "", "RevitAPI", p.Type);
            DescriptorLookup.Add(p.Name, desc);
        }
    }

    public BimDataBuilder BimDataBuilder { get; }
    public BimOpenSchemaExportSettings Settings { get; }
    public Options Options { get; }
    public Dictionary<string, DescriptorIndex> DescriptorLookup = new();
}