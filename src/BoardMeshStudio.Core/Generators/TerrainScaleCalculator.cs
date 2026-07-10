namespace BoardMeshStudio.Core.Generators;

public static class TerrainScaleCalculator
{
    public const double BaselineScaleMillimeters = 28.0;

    public static double GetScaleFactor(double scaleMillimeters)
    {
        if (scaleMillimeters <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(scaleMillimeters), "Scale must be greater than zero.");
        }

        return scaleMillimeters / BaselineScaleMillimeters;
    }

    public static HillGenerationSettings ApplyToDimensions(HillGenerationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var factor = GetScaleFactor(settings.ScaleMillimeters);
        if (Math.Abs(factor - 1.0) < 0.000001)
        {
            return settings;
        }

        return new HillGenerationSettings
        {
            Width = settings.Width * factor,
            Depth = settings.Depth * factor,
            Height = settings.Height * factor,
            ScaleMillimeters = BaselineScaleMillimeters,
            ReferenceMiniatureHeight = settings.ReferenceMiniatureHeight * factor,
            Resolution = settings.Resolution,
            NoiseStrength = settings.NoiseStrength * factor,
            Seed = settings.Seed,
            BaseThickness = settings.BaseThickness * factor,
            OuterWallThickness = settings.OuterWallThickness * factor,
            IncludeBase = settings.IncludeBase,
            TargetTriangleCount = settings.TargetTriangleCount,
            Style = settings.Style
        };
    }
}
