using Ara3D.Logging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Text;
using System.Windows.Forms;

namespace Ara3D.Bowerbird.RevitSamples;

public class CommandBimOpenSchemaVersion2 : NamedCommand
{
    public BosBackgroundExporterForm BosForm;
    public UIApplication UIApp;
    public BosRevitBuilder BosRevitBuilder;
    public override string Name => "BIM Open Schema Exporter (v2)";
    public ExportProgressForm ExportProgressForm;
    public int ProcessedCount;

    public override void Execute(object arg)
    {
        UIApp = arg as UIApplication;
        var doc = UIApp?.ActiveUIDocument?.Document;
        var sb = new StringBuilder();
        var logger = Logger.Create(sb);

        logger.Log("Getting settings and options");
        
        // TEMP: 
        //var settings = new BimOpenSchemaExportSettings();
        var settings = BimOpenSchemaExportSettings.LoadDefaultOrCreate();

        //logger.Log(settings.ToJsonString());

        var decider = new ExportDecider(settings);

        var options = new Options()
        {
            ComputeReferences = false,
            DetailLevel = (ViewDetailLevel)settings.DetailLevel,
        };

        logger.Log($"Creating Revit builder and gather links");
        BosRevitBuilder = new BosRevitBuilder(options, settings, doc, decider.ShouldExport);

        logger.Log($"Found {BosRevitBuilder.DocumentContexts.Count} documents");

        // DEBUG: 
        //foreach (var kv in decider.LookupBuiltInCategory)
        //    if (!kv.Value)
        //        logger.Log($"Skipped category {kv.Key}");
        //
        //foreach (var kv in decider.LookupDotNetType)
        //    if (!kv.Value)
        //        logger.Log($"Skipped type {kv.Key}");

        var total = 0;
        foreach (var db in BosRevitBuilder.DocumentBuilders)
        {
            var count = db.ElementToEntityIndex.Count;
            total += count;
            logger.Log($"{db.Document.Title} has {count} elements");
        }

        logger.Log($"Total {total} elements");

        if (ProcessInBackground(logger))
        {
            logger.Log($"Building Geometry");
            BosRevitBuilder.BuildGeometry();
            logger.Log($"Exporting BIM Open Schema");
            BosRevitBuilder.ExportBimOpenSchema(settings, logger);
            logger.Log($"Completed export");
        }

        TextDisplayForm.DisplayText(sb.ToString());
    }

    public bool ProcessInBackground(ILogger logger)
    {
        var processor = new BackgroundProcessor<(BosDocumentBuilder, long)>(
            pair => DoWork(pair.Item1, pair.Item2), UIApp);

        foreach (var db in BosRevitBuilder.DocumentBuilders)
        {
            db.ProcessDocument();
            foreach (var k in db.ElementToEntityIndex.Keys)
                processor.EnqueueWork((db, k));
        }
        logger.Log($"Queued all work");

        var count = processor.Queue.Count;
        ProcessedCount = 0;
        ExportProgressForm = new ExportProgressForm($"Exporting BIM Open Schema", count);
        ExportProgressForm.Show();
        
        processor.OnHeartbeat += ProcessorOnHeartbeat;
        processor.OnIdle += ProcessorOnIdle;

        try
        {
            processor.ProcessWork(true);
            logger.Log($"Processed all work");
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e);
            return false;
        }
        finally
        {
            ExportProgressForm.Close();
            ExportProgressForm = null;
            processor.Dispose();
        }
    }

    public void DoWork(BosDocumentBuilder db, long id)
    {
        if (ExportProgressForm.IsCancelRequested)
            return;
        
        db.ProcessElement(id);

        if (BosRevitBuilder.Settings.IncludeGeometry)
        {
            // Only add geometry for objects that are not types.
            if (db.NonTypeElementIds.Contains(id))
            {
                var eid = new ElementId(id);
                var e = db.Document.GetElement(eid);
                BosRevitBuilder.MeshGatherer.AddElement(db, e);
            }
        }

        ProcessedCount++;

        // Update UI every N elements
        if (ProcessedCount % 100 == 0)
        {
            ExportProgressForm.Report(ProcessedCount, $"Exporting: Element #{ProcessedCount} with id {id}");
            // Keep UI responsive
            Application.DoEvents();
        }
    }

    private void ProcessorOnHeartbeat(object sender, EventArgs e)
    {
        BosForm?.SetHeartBeat();
    }

    private void ProcessorOnIdle(object sender, EventArgs e)
    {
        BosForm?.SetIdle();
    }

}