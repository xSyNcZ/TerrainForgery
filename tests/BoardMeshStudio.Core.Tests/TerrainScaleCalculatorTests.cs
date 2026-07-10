using BoardMeshStudio.Core.Generators;

namespace BoardMeshStudio.Core.Tests;

public sealed class TerrainScaleCalculatorTests
{
    [Fact]
    public void Settings_DefaultReferenceMiniatureMatchesBaselineScale()
    {
        var settings = new HillGenerationSettings();

        Assert.Equal(28.0, settings.ReferenceMiniatureHeight);
    }

    [Fact]
    public void ApplyToDimensions_ScalesPhysicalDimensions()
    {
        var settings = new HillGenerationSettings
        {
            Width = 100.0,
            Depth = 80.0,
            Height = 20.0,
            ScaleMillimeters = 14.0,
            ReferenceMiniatureHeight = 32.0,
            BaseThickness = 4.0,
            OuterWallThickness = 2.0
        };

        var scaled = TerrainScaleCalculator.ApplyToDimensions(settings);

        Assert.Equal(50.0, scaled.Width);
        Assert.Equal(40.0, scaled.Depth);
        Assert.Equal(10.0, scaled.Height);
        Assert.Equal(16.0, scaled.ReferenceMiniatureHeight);
        Assert.Equal(2.0, scaled.BaseThickness);
        Assert.Equal(1.0, scaled.OuterWallThickness);
    }

    [Fact]
    public void GetScaleFactor_RejectsInvalidScale()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TerrainScaleCalculator.GetScaleFactor(0.0));
    }
}
