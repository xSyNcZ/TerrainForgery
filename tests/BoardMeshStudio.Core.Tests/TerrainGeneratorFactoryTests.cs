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
        Assert.True(bounds.MinZ >= 0.0);
    }

    [Theory]
    [InlineData(TerrainGeneratorType.Hill)]
    [InlineData(TerrainGeneratorType.Crater)]
    [InlineData(TerrainGeneratorType.Rock)]
    public void Generate_WithoutBase_TrimsFlatOuterPlate(TerrainGeneratorType type)
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

        var mesh = TerrainGeneratorFactory.Create(type).Generate(settings);

        Assert.True(mesh.HasTriangles);
        Assert.True(mesh.Triangles.Count < 72);
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
    public void Generate_SeedProfileChangesTerrainShape()
    {
        var smoothSettings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 8,
            NoiseStrength = 1.5,
            Seed = 1000
        };
        var ruggedSettings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 8,
            NoiseStrength = 1.5,
            Seed = 9999
        };

        var smooth = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(smoothSettings);
        var rugged = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(ruggedSettings);

        var smoothBounds = MeshBoundsCalculator.Calculate(smooth);
        var ruggedBounds = MeshBoundsCalculator.Calculate(rugged);
        Assert.NotEqual(smoothBounds.MaxZ, ruggedBounds.MaxZ);
    }

    [Fact]
    public void Generate_BlockBuildingIncludesArchitecturalDetails()
    {
        var settings = new HillGenerationSettings
        {
            Width = 50.0,
            Depth = 35.0,
            Height = 24.0,
            Resolution = 4,
            BaseThickness = 0.0,
            IncludeBase = false,
            Style = TerrainStyle.Realistic
        };

        var mesh = TerrainGeneratorFactory.Create(TerrainGeneratorType.BlockBuilding).Generate(settings);
        var bounds = MeshBoundsCalculator.Calculate(mesh);

        Assert.True(mesh.Triangles.Count > 12);
        Assert.True(bounds.MaxZ > settings.Height);
        Assert.True(bounds.Depth > settings.Depth);
    }
}
