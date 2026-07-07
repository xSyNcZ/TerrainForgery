using TerrainForgery.Core.Geometry;
using TerrainForgery.Export;

namespace TerrainForgery.Export.Tests;

public sealed class StlAsciiExporterTests
{
    [Fact]
    public void Export_CreatesStlFile()
    {
        var mesh = CreateSingleTriangleMesh();
        var filePath = Path.Combine(Path.GetTempPath(), $"terrainforgery_{Guid.NewGuid():N}.stl");
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
        var filePath = Path.Combine(Path.GetTempPath(), $"terrainforgery_{Guid.NewGuid():N}.stl");
        var exporter = new StlAsciiExporter();

        try
        {
            exporter.Export(mesh, filePath);
            var content = File.ReadAllText(filePath);

            Assert.Contains("solid TerrainForgery", content);
            Assert.Contains("facet normal", content);
            Assert.Contains("vertex", content);
            Assert.Contains("endsolid TerrainForgery", content);
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
