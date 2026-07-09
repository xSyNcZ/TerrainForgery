namespace BoardMeshStudio.Core.Generators;

public static class TriangleBudgetResolver
{
    private const int MinResolution = 2;
    private const int MaxResolution = 256;

    public static HillGenerationSettings Apply(HillGenerationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.TargetTriangleCount is not { } targetTriangleCount || targetTriangleCount <= 0)
        {
            return settings;
        }

        var resolution = EstimateGridResolution(targetTriangleCount, settings.IncludeBase ? 6.0 : 4.0, 8.0);

        return new HillGenerationSettings
        {
            Width = settings.Width,
            Depth = settings.Depth,
            Height = settings.Height,
            Resolution = resolution,
            NoiseStrength = settings.NoiseStrength,
            Seed = settings.Seed,
            BaseThickness = settings.BaseThickness,
            OuterWallThickness = settings.OuterWallThickness,
            IncludeBase = settings.IncludeBase,
            TargetTriangleCount = settings.TargetTriangleCount,
            Style = settings.Style
        };
    }

    public static int EstimateGridResolution(int targetTriangleCount)
    {
        if (targetTriangleCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetTriangleCount), "Triangle target must be greater than zero.");
        }

        // No-base grid terrain creates roughly 4r^2 + 8r triangles: top, underside, and side strips.
        var estimatedResolution = EstimateGridResolution(targetTriangleCount, 4.0, 8.0);
        return estimatedResolution;
    }

    private static int EstimateGridResolution(int targetTriangleCount, double surfaceFactor, double sideFactor)
    {
        var estimatedResolution = (int)Math.Round((-sideFactor + Math.Sqrt(sideFactor * sideFactor + 4.0 * surfaceFactor * targetTriangleCount)) / (2.0 * surfaceFactor));
        return Math.Clamp(estimatedResolution, MinResolution, MaxResolution);
    }
}
