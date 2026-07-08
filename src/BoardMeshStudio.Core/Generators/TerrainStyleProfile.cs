namespace BoardMeshStudio.Core.Generators;

internal sealed class TerrainStyleProfile
{
    private TerrainStyleProfile(
        double heightMultiplier,
        double noiseMultiplier,
        double shapeExponent,
        double featureScale,
        int terraceSteps,
        double smoothing)
    {
        HeightMultiplier = heightMultiplier;
        NoiseMultiplier = noiseMultiplier;
        ShapeExponent = shapeExponent;
        FeatureScale = featureScale;
        TerraceSteps = terraceSteps;
        Smoothing = smoothing;
    }

    public double HeightMultiplier { get; }

    public double NoiseMultiplier { get; }

    public double ShapeExponent { get; }

    public double FeatureScale { get; }

    public int TerraceSteps { get; }

    public double Smoothing { get; }

    public static TerrainStyleProfile From(TerrainStyle style)
    {
        return style switch
        {
            TerrainStyle.Realistic => new TerrainStyleProfile(1.0, 0.75, 1.45, 1.0, 0, 0.85),
            TerrainStyle.Stylized => new TerrainStyleProfile(1.15, 0.35, 0.85, 0.65, 0, 0.55),
            TerrainStyle.AnimeInspired => new TerrainStyleProfile(1.35, 0.2, 0.62, 0.45, 5, 0.35),
            TerrainStyle.MiniatureFriendly => new TerrainStyleProfile(0.95, 0.3, 1.1, 0.8, 0, 0.7),
            TerrainStyle.LowPoly => new TerrainStyleProfile(1.0, 0.8, 1.0, 1.3, 7, 0.15),
            TerrainStyle.RuggedNatural => new TerrainStyleProfile(1.18, 1.45, 1.65, 1.7, 0, 0.55),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Unsupported terrain style.")
        };
    }

    public double ShapeHeight(double value)
    {
        var clamped = Math.Max(0.0, value);

        if (TerraceSteps > 1)
        {
            clamped = Math.Round(clamped * TerraceSteps) / TerraceSteps;
        }

        return clamped;
    }
}
