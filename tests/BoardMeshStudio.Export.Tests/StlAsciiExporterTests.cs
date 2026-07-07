using BoardMeshStudio.Core.Geometry;
using BoardMeshStudio.Export;

namespace BoardMeshStudio.Export.Tests;

public sealed class StlAsciiExporterTests
{
    [Fact]
    public void Export_CreatesStlFile()
    {
        var mesh = CreateSingleTriangleMesh();
        var filePath = Path.Combine(Path.GetTempPath(), $"boardmeshstudio_{Guid.NewGuid():N}.stl");
        var exporter = new StlAsciiExporter();

        try
        {
            exporter.Export(mesh, filePath);

            Assert.True(File.Exists(filePath));
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    [Fact]
    public void Export_WritesExpectedAsciiStlPhrases()
    {
        var mesh = CreateSingleTriangleMesh();
        var filePath = Path.Combine(Path.GetTempPath(), $"boardmeshstudio_{Guid.NewGuid():N}.stl");
        var exporter = new StlAsciiExporter();

        try
        {
            exporter.Export(mesh, filePath);
            var content = File.ReadAllText(filePath);

            Assert.Contains("solid BoardMesh Studio", content);
            Assert.Contains("facet normal", content);
            Assert.Contains("vertex", content);
            Assert.Contains("endsolid BoardMesh Studio", content);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    private static Mesh CreateSingleTriangleMesh()
    {
        var mesh = new Mesh();
        mesh.AddTriangle(
            new Vertex(0.0, 0.0, 0.0),
            new Vertex(1.0, 0.0, 0.0),
            new Vertex(0.0, 1.0, 0.0));

        return mesh;
    }
}
