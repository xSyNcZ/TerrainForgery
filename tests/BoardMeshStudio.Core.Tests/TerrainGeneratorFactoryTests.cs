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
    public void Generate_WithoutBase_CreatesClosedObjectWithoutSquareBase(TerrainGeneratorType type)
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
        var vertices = mesh.Triangles.SelectMany(triangle => new[] { triangle.A, triangle.B, triangle.C }).ToList();

        Assert.True(mesh.HasTriangles);
        Assert.Contains(vertices, vertex => Math.Abs(vertex.Z) < 0.0001);
        Assert.DoesNotContain(vertices, vertex =>
            Math.Abs(Math.Abs(vertex.X) - settings.Width / 2.0) < 0.0001
            && Math.Abs(Math.Abs(vertex.Y) - settings.Depth / 2.0) < 0.0001);
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

        Assert.Equal(264, mesh.Triangles.Count);
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

        Assert.NotEqual(SumVertexHeights(smooth), SumVertexHeights(rugged));
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
        var lowPolySettings = new HillGenerationSettings
        {
            Width = 40.0,
            Depth = 40.0,
            Height = 10.0,
            Resolution = 8,
            NoiseStrength = 0.0,
            Style = TerrainStyle.LowPoly
        };

        var realistic = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(realisticSettings);
        var lowPoly = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(lowPolySettings);

        Assert.NotEqual(SumVertexHeights(realistic), SumVertexHeights(lowPoly));
    }

    [Theory]
    [InlineData(TerrainGeneratorType.Hill)]
    [InlineData(TerrainGeneratorType.Crater)]
    [InlineData(TerrainGeneratorType.Rock)]
    public void Generate_WithoutBase_SmoothsOpenFootprintEdge(TerrainGeneratorType type)
    {
        var settings = new HillGenerationSettings
        {
            Width = 60.0,
            Depth = 60.0,
            Height = 18.0,
            Resolution = 18,
            NoiseStrength = 1.0,
            IncludeBase = false,
            Seed = 9876
        };

        var mesh = TerrainGeneratorFactory.Create(type).Generate(settings);
        var outerEdgeMaxZ = mesh.Triangles
            .SelectMany(triangle => new[] { triangle.A, triangle.B, triangle.C })
            .Where(vertex => GetNormalizedRadius(vertex, settings) > 0.92)
            .Max(vertex => vertex.Z);

        Assert.True(outerEdgeMaxZ < settings.Height * 0.35);
    }

    [Theory]
    [InlineData(TerrainGeneratorType.Hill)]
    [InlineData(TerrainGeneratorType.Crater)]
    [InlineData(TerrainGeneratorType.Rock)]
    [InlineData(TerrainGeneratorType.Wall)]
    [InlineData(TerrainGeneratorType.BlockBuilding)]
    public void Generate_FitsInsideRequestedBounds(TerrainGeneratorType type)
    {
        var settings = new HillGenerationSettings
        {
            Width = 50.0,
            Depth = 35.0,
            Height = 20.0,
            Resolution = 10,
            NoiseStrength = 2.5,
            Seed = 9999,
            BaseThickness = 3.0,
            IncludeBase = true,
            Style = TerrainStyle.AnimeInspired
        };

        var mesh = TerrainGeneratorFactory.Create(type).Generate(settings);
        var bounds = MeshBoundsCalculator.Calculate(mesh);

        Assert.True(bounds.MinX >= -settings.Width / 2.0 - 0.0001);
        Assert.True(bounds.MaxX <= settings.Width / 2.0 + 0.0001);
        Assert.True(bounds.MinY >= -settings.Depth / 2.0 - 0.0001);
        Assert.True(bounds.MaxY <= settings.Depth / 2.0 + 0.0001);
        Assert.True(bounds.MinZ >= -settings.BaseThickness - 0.0001);
        Assert.True(bounds.MaxZ <= settings.Height + 0.0001);
    }

    [Fact]
    public void Generate_ScalePresetChangesGeneratedDimensions()
    {
        var baselineSettings = new HillGenerationSettings
        {
            Width = 100.0,
            Depth = 80.0,
            Height = 20.0,
            Resolution = 8,
            IncludeBase = true,
            ScaleMillimeters = 28.0
        };
        var halfScaleSettings = new HillGenerationSettings
        {
            Width = 100.0,
            Depth = 80.0,
            Height = 20.0,
            Resolution = 8,
            IncludeBase = true,
            ScaleMillimeters = 14.0
        };

        var baseline = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(baselineSettings);
        var halfScale = TerrainGeneratorFactory.Create(TerrainGeneratorType.Hill).Generate(halfScaleSettings);

        var baselineBounds = MeshBoundsCalculator.Calculate(baseline);
        var halfScaleBounds = MeshBoundsCalculator.Calculate(halfScale);
        Assert.Equal(baselineBounds.Width / 2.0, halfScaleBounds.Width, 3);
        Assert.Equal(baselineBounds.Depth / 2.0, halfScaleBounds.Depth, 3);
    }

    [Theory]
    [InlineData(TerrainGeneratorType.Hill)]
    [InlineData(TerrainGeneratorType.Crater)]
    [InlineData(TerrainGeneratorType.Rock)]
    public void Generate_WithBase_KeepsRaisedTerrainAwayFromBaseEdges(TerrainGeneratorType type)
    {
        var settings = new HillGenerationSettings
        {
            Width = 60.0,
            Depth = 50.0,
            Height = 18.0,
            Resolution = 14,
            NoiseStrength = 1.2,
            IncludeBase = true,
            OuterWallThickness = 5.0,
            Seed = 9876
        };

        var mesh = TerrainGeneratorFactory.Create(type).Generate(settings);
        var raisedVertices = mesh.Triangles
            .SelectMany(triangle => new[] { triangle.A, triangle.B, triangle.C })
            .Where(vertex => vertex.Z > 0.05)
            .ToList();

        Assert.NotEmpty(raisedVertices);
        Assert.All(raisedVertices, vertex => Assert.True(Math.Abs(vertex.X) <= settings.Width / 2.0 - settings.OuterWallThickness + 0.0001));
        Assert.All(raisedVertices, vertex => Assert.True(Math.Abs(vertex.Y) <= settings.Depth / 2.0 - settings.OuterWallThickness + 0.0001));
    }

    [Theory]
    [InlineData(TerrainGeneratorType.Hill)]
    [InlineData(TerrainGeneratorType.Crater)]
    [InlineData(TerrainGeneratorType.Rock)]
    public void Generate_WithBase_PreservesFullBaseFootprint(TerrainGeneratorType type)
    {
        var settings = new HillGenerationSettings
        {
            Width = 72.0,
            Depth = 48.0,
            Height = 16.0,
            Resolution = 12,
            IncludeBase = true,
            BaseThickness = 4.0,
            OuterWallThickness = 6.0
        };

        var mesh = TerrainGeneratorFactory.Create(type).Generate(settings);
        var bounds = MeshBoundsCalculator.Calculate(mesh);

        Assert.Equal(settings.Width, bounds.Width, 3);
        Assert.Equal(settings.Depth, bounds.Depth, 3);
        Assert.Equal(-settings.BaseThickness, bounds.MinZ, 3);
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
        Assert.True(bounds.MaxZ > settings.Height * 0.85);
        Assert.True(bounds.MaxZ <= settings.Height);
        Assert.True(bounds.Depth <= settings.Depth);
    }

    private static double SumVertexHeights(BoardMeshStudio.Core.Geometry.Mesh mesh)
    {
        return mesh.Triangles.Sum(triangle => triangle.A.Z + triangle.B.Z + triangle.C.Z);
    }

    private static double GetNormalizedRadius(BoardMeshStudio.Core.Geometry.Vertex vertex, HillGenerationSettings settings)
    {
        var x = vertex.X / (settings.Width / 2.0);
        var y = vertex.Y / (settings.Depth / 2.0);
        return Math.Sqrt(x * x + y * y);
    }
}
