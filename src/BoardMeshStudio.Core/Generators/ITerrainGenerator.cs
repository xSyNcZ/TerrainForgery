using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public interface ITerrainGenerator
{
    Mesh Generate(HillGenerationSettings settings);
}
