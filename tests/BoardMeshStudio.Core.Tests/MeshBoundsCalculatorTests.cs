using BoardMeshStudio.Core.Analysis;
using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Tests;

public sealed class MeshBoundsCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsBoundsForMesh()
    {
        var mesh = new Mesh();
        mesh.AddTriangle(
            new Vertex(-2.0, -1.0, -3.0),
            new Vertex(4.0, 2.0, 1.0),
            new Vertex(1.0, 8.0, 5.0));

        var bounds = MeshBoundsCalculator.Calculate(mesh);

        Assert.Equal(-2.0, bounds.MinX);
        Assert.Equal(-1.0, bounds.MinY);
        Assert.Equal(-3.0, bounds.MinZ);
        Assert.Equal(4.0, bounds.MaxX);
        Assert.Equal(8.0, bounds.MaxY);
        Assert.Equal(5.0, bounds.MaxZ);
        Assert.Equal(9.0, bounds.LargestDimension);
    }

    [Fact]
    public void Calculate_ThrowsException_WhenMeshIsEmpty()
    {
        var mesh = new Mesh();

        Assert.Throws<ArgumentException>(() => MeshBoundsCalculator.Calculate(mesh));
    }
}
