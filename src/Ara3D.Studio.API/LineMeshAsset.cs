using Ara3D.Geometry;

namespace Ara3D.Studio.API;

public class LineMeshAsset : ILineMeshAsset
{
    public LineMesh3D Lines { get; private set; }

    public LineMeshAsset(LineMesh3D lines)
        => Lines = lines;

    public void Update(LineMesh3D lines)
        => Lines = lines;
}