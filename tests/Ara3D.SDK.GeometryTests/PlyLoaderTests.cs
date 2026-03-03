using Ara3D.IO.PLY;
using Ara3D.Logging;
using Ara3D.Utils;

namespace Ara3D.SDK.GeometryTests
{
    public static class PlyLoaderTests
    {
        public static FilePath Lucy = new(@"C:\Users\cdigg\git\3d-format-shootout\data\copies\three.js\models\ply\binary\Lucy100k.ply");
        public static FilePath BunZipper = new(@"C:\Users\cdigg\git\draco\testdata\bun_zipper.ply");
        public static FilePath ColoredDolphins = new(@"C:\Users\cdigg\data\Ara3D\ara3d-studio-test-data\sample-data\dolphins_colored.ply");
        public static FilePath Dolphins = new(@"C:\Users\cdigg\git\3d-format-shootout\data\copies\three.js\models\ply\ascii\dolphins.ply");
        public static FilePath Wuson = new(@"C:\Users\cdigg\git\3d-format-shootout\data\copies\assimp\models\PLY\Wuson.ply");

        public static FilePath[] Files = [Lucy, BunZipper, ColoredDolphins, Dolphins, Wuson];

        [Test, TestCaseSource(nameof(Files))]
        public static void TestLoadFile(FilePath filePath)
        {
            var logger = Logger.Console;
            logger.Log($"Loading {filePath.GetFileName()}");
            var mesh = PlyImporter.LoadMesh(filePath);
            logger.Log($"Loaded # points = {mesh.Points.Count}, # triangles = {mesh.FaceIndices.Count}");
        }
    }
}
