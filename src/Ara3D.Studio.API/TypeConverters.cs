using Ara3D.Geometry;
using Ara3D.Models;

namespace Ara3D.Studio.API;

public static class TypeConverters
{
    public static void Convert(ref TriangleMesh3D from, ref IModel3D to)
    {
    }

    public static void Convert(ref LineMesh3D from, ref IModel3D to)
    {
    }

    public static void Convert(ref QuadMesh3D from, ref TriangleMesh3D to)
    {
    }

    public static List<Type> RenderTypes
        =
        [
            typeof(TriangleMesh3D),
            typeof(IModel3D),
            typeof(QuadMesh3D),
        ];
}