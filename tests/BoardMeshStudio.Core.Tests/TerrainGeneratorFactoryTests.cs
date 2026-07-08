using BoardMeshStudio.Core.Generators;
using BoardMeshStudio.Core.Analysis;

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

    [Fact]
    public void Generate_WithoutBase_DoesNotExtendBelowZero()
    {
        var settings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 6,
            BaseThickness = 4.0,
            IncludeBase = false
        };

        var mesh = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(settings);

        var bounds = MeshBoundsCalculator.Calculate(mesh);
        Assert.Equal(0.0, bounds.MinZ);
    }

    [Fact]
    public void Generate_WithBase_ExtendsBelowZero()
    {
        var settings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 6,
            BaseThickness = 4.0,
            IncludeBase = true
        };

        var mesh = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(settings);

        var bounds = MeshBoundsCalculator.Calculate(mesh);
        Assert.Equal(-4.0, bounds.MinZ);
    }

    [Fact]
    public void Generate_UsesTriangleBudgetForGridGenerators()
    {
        var settings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 48,
            TargetTriangleCount = 252,
            BaseThickness = 2.0
        };

        var mesh = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(settings);

        Assert.Equal(252, mesh.Triangles.Count);
    }

    [Fact]
    public void Generate_StyleChangesTerrainShape()
    {
        var realisticSettings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 8,
            NoiseStrength = 0.0,
            Style = TerrainStyle.Realistic
        };
        var stylizedSettings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 8,
            NoiseStrength = 0.0,
            Style = TerrainStyle.AnimeInspired
        };

        var realistic = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(realisticSettings);
        var stylized = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(stylizedSettings);

        var realisticBounds = MeshBoundsCalculator.Calculate(realistic);
        var stylizedBounds = MeshBoundsCalculator.Calculate(stylized);
        Assert.True(stylizedBounds.MaxZ > realisticBounds.MaxZ);
    }
}
