namespace BoardMeshStudio.Core.Generators;

public sealed class HillGenerationSettings
{
    public double Width { get; set; } = 100.0;

    public double Depth { get; set; } = 100.0;

    public double Height { get; set; } = 25.0;

    public int Resolution { get; set; } = 32;

    public double NoiseStrength { get; set; } = 1.5;

    public int Seed { get; set; } = 12345;

    public double BaseThickness { get; set; } = 4.0;

    public bool IncludeBase { get; set; } = true;

    public int? TargetTriangleCount { get; set; }

    public TerrainStyle Style { get; set; } = TerrainStyle.Realistic;

    public double EffectiveBaseThickness => IncludeBase ? BaseThickness : 0.0;
}
