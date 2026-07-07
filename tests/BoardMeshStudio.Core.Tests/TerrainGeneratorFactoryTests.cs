using BoardMeshStudio.Core.Generators;

namespace BoardMeshStudio.Core.Tests;

public sealed class TerrainGeneratorFactoryTests
{
    [Theory]
    [InlineData(TerrainGeneratorType.Hill)]
    [InlineData(TerrainGeneratorType.Crater)]
    [InlineData(TerrainGeneratorType.Rock)]
    [InlineData(TerrainGeneratorType.Wall)]
    [InlineData(TerrainGeneratorType.BlockBuilding)]
    public void Create_GeneratorReturnsMeshWithTriangles(TerrainGeneratorType type)
    {
        var settings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 30.0,
            Height = 12.0,
            Resolution = 6,
            NoiseStrength = 0.4,
            Seed = 77,
            BaseThickness = 2.0
        };

        var generator = TerrainGeneratorFactory.Create(type);

        var mesh = generator.Generate(settings);

        Assert.True(mesh.HasTriangles);
    }
}
