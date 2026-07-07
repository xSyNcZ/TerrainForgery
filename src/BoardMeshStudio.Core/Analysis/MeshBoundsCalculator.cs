using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Analysis;

public static class MeshBoundsCalculator
{
    public static MeshBounds Calculate(Mesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        if (!mesh.HasTriangles)
        {
            throw new ArgumentException("Mesh must contain at least one triangle.", nameof(mesh));
        }

        var vertices = mesh.Triangles.SelectMany(triangle => new[] { triangle.A, triangle.B, triangle.C });

        var minX = double.PositiveInfinity;
        var minY = double.PositiveInfinity;
        var minZ = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
        var maxY = double.NegativeInfinity;
        var maxZ = double.NegativeInfinity;

        foreach (var vertex in vertices)
        {
            minX = Math.Min(minX, vertex.X);
            minY = Math.Min(minY, vertex.Y);
            minZ = Math.Min(minZ, vertex.Z);
            maxX = Math.Max(maxX, vertex.X);
            maxY = Math.Max(maxY, vertex.Y);
            maxZ = Math.Max(maxZ, vertex.Z);
        }

        return new MeshBounds(minX, minY, minZ, maxX, maxY, maxZ);
    }
}
