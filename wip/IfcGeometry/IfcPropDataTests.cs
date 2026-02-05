using Ara3D.IO.StepParser;
using Ara3D.Utils;
using Ara3D.Logging;

namespace Ara3D.IfcGeometry;

public static class IfcPropDataTests
{
    public static FilePath InputFile => IfcGeometryTests.InputFile;

    public static void OutputDoc(StepDocument doc, ILogger logger)
    {
        logger.Log($"Document {doc.FilePath.GetFileName()}");
        
        var byteCount = doc.Data.Bytes.Count;
        logger.Log($"# bytes = {byteCount:N0}");
        
        var defCount = doc.GetDefinitionLookup().Count;
        logger.Log($"# definitions in lookup = {defCount:N0}");

        var entLookup = doc.GetEntityNameLookup();
        logger.Log($"# entities in lookup = {entLookup.Count:N0}");

        var defs = doc.Definitions;
        logger.Log($"# defs = {defs.Count}");
    }

    public static string ValToStr(StepValue val)
    {
        if (val.IsEntity())
        {
            return $"{val.GetEntityName()} {val.GetEntityAttributesValue()}";
        }

        return val.ToString();
    }

    public static void OutputPropData(IfcPropData pd, ILogger logger)
    {
        logger.Log($"# object to prop sets = {pd.ObjectToPropSets.Count}");
        logger.Log($"# prop sets = {pd.PropSets.Count}");
        logger.Log($"# prop values = {pd.PropValues.Count}");

        var propSets = pd.PropSets.Values.ToList();
        for (var i = 0; i < 20; i++)
        {
            if (i >= propSets.Count) break;
            var ps = propSets[i];
            Console.WriteLine($"Property Set #{i} {ps.Name}[{ps.Id}] has {ps.Ids.Count} ids");
        }

        var propVals = pd.PropValues.Values.ToList();
        for (var i = 0; i < 20; i++)
        {
            if (i >= propVals.Count) break;
            var p = propVals[i];
            Console.WriteLine($"Prop value #{i} {p.Name}[{p.Id}] has value {ValToStr(p.Value)}");
        }
    }

    [Test]
    public static void TestProData()
    {
        var logger = Logger.Console;
        
        using var doc = new StepDocument(InputFile, logger);
        OutputDoc(doc, logger);

        var pd = new IfcPropData(doc);
        logger.Log("Constructed prop data");
        OutputPropData(pd, logger);
        logger.Log($"Completed");
    }
}