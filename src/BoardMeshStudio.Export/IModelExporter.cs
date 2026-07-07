using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Export;

public interface IModelExporter
{
    void Export(Mesh mesh, string filePath);
}
