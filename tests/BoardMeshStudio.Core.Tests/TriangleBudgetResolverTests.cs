using BoardMeshStudio.Core.Generators;

namespace BoardMeshStudio.Core.Tests;

public sealed class TriangleBudgetResolverTests
{
    [Theory]
    [InlineData(40, 2)]
    [InlineData(252, 7)]
    [InlineData(9600, 48)]
    public void EstimateGridResolution_ReturnsExpectedResolution(int targetTriangleCount, int expectedResolution)
    {
        var resolution = TriangleBudgetResolver.EstimateGridResolution(targetTriangleCount);

        Assert.Equal(expectedResolution, resolution);
    }

    [Fact]
    public void Apply_PreservesSettingsAndUpdatesResolution()
    {
        var settings = new HillGenerationSettings
        {
            Width = 42.0,
            Depth = 24.0,
            Height = 12.0,
            Resolution = 48,
            NoiseStrength = 0.7,
            Seed = 99,
            BaseThickness = 3.0,
            OuterWallThickness = 4.0,
            IncludeBase = false,
            TargetTriangleCount = 252,
            Style = TerrainStyle.RuggedNatural
        };

        var resolved = TriangleBudgetResolver.Apply(settings);

        Assert.Equal(7, resolved.Resolution);
        Assert.Equal(settings.Width, resolved.Width);
        Assert.Equal(settings.Depth, resolved.Depth);
        Assert.Equal(settings.Height, resolved.Height);
        Assert.Equal(settings.NoiseStrength, resolved.NoiseStrength);
        Assert.Equal(settings.Seed, resolved.Seed);
        Assert.Equal(settings.BaseThickness, resolved.BaseThickness);
        Assert.Equal(settings.OuterWallThickness, resolved.OuterWallThickness);
        Assert.Equal(settings.IncludeBase, resolved.IncludeBase);
        Assert.Equal(settings.TargetTriangleCount, resolved.TargetTriangleCount);
        Assert.Equal(settings.Style, resolved.Style);
    }
}
