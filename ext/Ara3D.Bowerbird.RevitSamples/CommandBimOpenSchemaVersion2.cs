using System.Text;
using Ara3D.Logging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples;

public class CommandBimOpenSchemaVersion2 : NamedCommand
{
    public override string Name => "BIM Open Schema Exporter (v2)";

    public BimOpenSchemaExportSettings GetExportSettings()
        => new() 
        {
            Folder = BimOpenSchemaExportSettings.DefaultFolder,
            IncludeLinks = true,
            IncludeGeometry = true,
            UseCurrentView = false,
        };

    public override void Execute(object arg)
    {
        var uiapp = arg as UIApplication;
        var doc = uiapp?.ActiveUIDocument?.Document;
        var sb = new StringBuilder();
        var logger = Logger.Create(sb);

        logger.Log("Creating central document with elements");
        // TODO: 
        var docWithElements = new BosDocumentContext(null, doc);

        logger.Log("Gathering linked documents");
        var docs = docWithElements.GatherLinkedDocuments();

        logger.Log($"Found {docs.Count} linked documents");
        foreach (var d in docs)
        {
            logger.Log($"Path = {d.Path}");
            logger.Log($"Title = {d.Title}");
            logger.Log($"Is Link = {d.IsLink}");
            logger.Log($"External path = {d.ExternalPath}");
            logger.Log($"Is detached = {d.IsDetached}");
            logger.Log($"Retrieving element IDs");
        }

        TextDisplayForm.DisplayText(sb.ToString());
    }
}