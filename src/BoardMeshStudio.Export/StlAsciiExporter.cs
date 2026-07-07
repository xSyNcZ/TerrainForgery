using System.Globalization;
using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Export;

public sealed class StlAsciiExporter : IModelExporter
{
    public void Export(Mesh mesh, string filePath)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(filePath);
        writer.WriteLine("solid BoardMesh Studio");

        foreach (var triangle in mesh.Triangles)
        {
            var normal = CalculateNormal(triangle);

            writer.WriteLine(FormattableString.Invariant($"  facet normal {normal.X} {normal.Y} {normal.Z}"));
            writer.WriteLine("    outer loop");
            WriteVertex(writer, triangle.A);
            WriteVertex(writer, triangle.B);
            WriteVertex(writer, triangle.C);
            writer.WriteLine("    endloop");
            writer.WriteLine("  endfacet");
        }

        writer.WriteLine("endsolid BoardMesh Studio");
    }

    private static Vertex CalculateNormal(Triangle triangle)
    {
        var ux = triangle.B.X - triangle.A.X;
        var uy = triangle.B.Y - triangle.A.Y;
        var uz = triangle.B.Z - triangle.A.Z;
        var vx = triangle.C.X - triangle.A.X;
        var vy = triangle.C.Y - triangle.A.Y;
        var vz = triangle.C.Z - triangle.A.Z;

        var nx = uy * vz - uz * vy;
        var ny = uz * vx - ux * vz;
        var nz = ux * vy - uy * vx;
        var length = Math.Sqrt(nx * nx + ny * ny + nz * nz);

        if (length == 0.0)
        {
            return new Vertex(0.0, 0.0, 0.0);
        }

        return new Vertex(nx / length, ny / length, nz / length);
    }

    private static void WriteVertex(TextWriter writer, Vertex vertex)
    {
        writer.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "      vertex {0} {1} {2}",
            vertex.X,
            vertex.Y,
            vertex.Z));
    }
}
