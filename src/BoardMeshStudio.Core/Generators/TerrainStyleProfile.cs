namespace BoardMeshStudio.Core.Generators;

internal sealed class TerrainStyleProfile
{
    private TerrainStyleProfile(double heightMultiplier, double noiseMultiplier, double shapeExponent)
    {
        HeightMultiplier = heightMultiplier;
        NoiseMultiplier = noiseMultiplier;
        ShapeExponent = shapeExponent;
    }

    public double HeightMultiplier { get; }

    public double NoiseMultiplier { get; }

    public double ShapeExponent { get; }

    public static TerrainStyleProfile From(TerrainStyle style)
    {
        return style switch
        {
            TerrainStyle.Realistic => new TerrainStyleProfile(1.0, 1.0, 1.15),
            TerrainStyle.Stylized => new TerrainStyleProfile(1.1, 0.65, 0.85),
            TerrainStyle.AnimeInspired => new TerrainStyleProfile(1.25, 0.45, 0.7),
            TerrainStyle.MiniatureFriendly => new TerrainStyleProfile(0.9, 0.55, 1.05),
            TerrainStyle.LowPoly => new TerrainStyleProfile(1.0, 0.9, 1.0),
            TerrainStyle.RuggedNatural => new TerrainStyleProfile(1.15, 1.65, 1.3),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Unsupported terrain style.")
        };
    }
}
