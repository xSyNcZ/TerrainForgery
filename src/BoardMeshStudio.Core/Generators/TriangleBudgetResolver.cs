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

        var resolution = EstimateGridResolution(targetTriangleCount);

        return new HillGenerationSettings
        {
            Width = settings.Width,
            Depth = settings.Depth,
            Height = settings.Height,
            Resolution = resolution,
            NoiseStrength = settings.NoiseStrength,
            Seed = settings.Seed,
            BaseThickness = settings.BaseThickness,
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

        // Grid generators create roughly 4r^2 + 8r triangles: top, underside, and side strips.
        var estimatedResolution = (int)Math.Round((-8.0 + Math.Sqrt(64.0 + 16.0 * targetTriangleCount)) / 8.0);
        return Math.Clamp(estimatedResolution, MinResolution, MaxResolution);
    }
}
