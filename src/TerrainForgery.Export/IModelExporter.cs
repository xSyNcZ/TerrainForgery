using TerrainForgery.Core.Geometry;

namespace TerrainForgery.Export;

public interface IModelExporter
{
    void Export(Mesh mesh, string filePath);
}
