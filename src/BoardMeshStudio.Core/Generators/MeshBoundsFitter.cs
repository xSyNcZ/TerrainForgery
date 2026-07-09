using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

internal static class MeshBoundsFitter
{
    public static Mesh FitToSettings(Mesh mesh, HillGenerationSettings settings)
    {
        var fitted = new Mesh();
        var minX = -settings.Width / 2.0;
        var maxX = settings.Width / 2.0;
        var minY = -settings.Depth / 2.0;
        var maxY = settings.Depth / 2.0;
        var minZ = -settings.EffectiveBaseThickness;
        var maxZ = settings.Height;

        foreach (var triangle in mesh.Triangles)
        {
            fitted.AddTriangle(
                FitVertex(triangle.A, minX, maxX, minY, maxY, minZ, maxZ),
                FitVertex(triangle.B, minX, maxX, minY, maxY, minZ, maxZ),
                FitVertex(triangle.C, minX, maxX, minY, maxY, minZ, maxZ));
        }

        return fitted;
    }

    private static Vertex FitVertex(Vertex vertex, double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
    {
        return new Vertex(
            Math.Clamp(vertex.X, minX, maxX),
            Math.Clamp(vertex.Y, minY, maxY),
            Math.Clamp(vertex.Z, minZ, maxZ));
    }
}
