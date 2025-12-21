using System.Text.Json;
using Ara3D.BimOpenSchema;
using Ara3D.Extras;
using Ara3D.Logging;
using Ara3D.Utils;

namespace Ara3D.BIMOpenSchema.Tests;

public static class BosTests
{
    public static DirectoryPath InputDir => SpecialFolders.MyDocuments.RelativeFolder("BIM Open Schema");
    public static string TestFileName = "Snowdon Towers Sample Architectural.bos";
    public static FilePath TestFile => InputDir.RelativeFile(TestFileName);

    [Test]
    public static void TestLoadBimDataAndBimGeometry()
    {
        var logger = Logger.Console;
        logger.Log("Loading BIM Geometry");
        var bg = TestFile.ReadBimGeometryFromParquetZip();
        logger.Log("Loading BIM Data");
        var bd = TestFile.ReadBimDataFromParquetZip();
        logger.Log("Loaded data");

        logger.Log("Creating BIM object model");
        var bom = new BimObjectModel(bd);
        logger.Log("Created object models");

        bd.OutputDiagnostics(logger);
        bd.OutputSummary(logger);
        bg.OutputBimGeometryCounts(logger);

        logger.Log("Strings");
        for (var i = 0; i < 20; i++)
        {
            logger.Log($"  {bd.Strings[i]}");
        }
    }

    public static void OutputDiagnostics(this IBimData bd, ILogger logger)
    {
        logger.Log("Diagnostics:");
        foreach (var d in bd.GetDiagnosticStrings())
            logger.Log($"  {d}");
    }

    public static void OutputSummary(this IBimData bd, ILogger logger)
    {
        var json = JsonSerializer.Serialize(
            bd.Manifest,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        Console.WriteLine(json); logger.Log($"# documents = {bd.Documents.Count}");
        logger.Log($"# entities = {bd.Entities.Count}");
        logger.Log($"# diagnostics = {bd.Diagnostics.Count}");
        logger.Log($"# descriptors = {bd.Descriptors.Count}");
        logger.Log($"# points = {bd.Points.Count}");
        logger.Log($"# string = {bd.Strings.Count}");
        logger.Log($"# relations = {bd.Relations.Count}");
        logger.Log($"# total parameters = {bd.GetNumParameters()}");
        logger.Log($"  # string parameters = {bd.StringParameters.Count}");
        logger.Log($"  # point parameters  = {bd.PointParameters.Count}");
        logger.Log($"  # integer parameters = {bd.IntegerParameters.Count}");
        logger.Log($"  # single parameters = {bd.SingleParameters.Count}");
        logger.Log($"  # entity parameters = {bd.EntityParameters.Count}");

        var cats = bd.GetCategoryNames();
        logger.Log($"Categories");
        foreach (var cat in cats)
            logger.Log($"  {cat}");

        var types = bd.GetTypeNames();
        logger.Log($"Types");
        foreach (var type in types)
            logger.Log($"  {type}");
    }

    public static void OutputBimGeometryCounts(this BimGeometry bimGeometry, ILogger logger)
    {
        logger.Log($"# transforms = {bimGeometry.GetNumTransforms()}");
        logger.Log($"# meshes = {bimGeometry.GetNumMeshes()}");
        logger.Log($"# elements = {bimGeometry.GetNumElements()}");
        logger.Log($"# faces = {bimGeometry.GetNumFaces()}");
        logger.Log($"# vertices = {bimGeometry.GetNumVertices()}");
        logger.Log($"# materials = {bimGeometry.GetNumMaterials()}");
    }
}