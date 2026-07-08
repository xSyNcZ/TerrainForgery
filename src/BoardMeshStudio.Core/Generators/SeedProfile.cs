namespace BoardMeshStudio.Core.Generators;

public sealed class SeedProfile
{
    private SeedProfile(int seed, int roughnessDigit, int rockinessDigit, int asymmetryDigit, int featureScaleDigit)
    {
        Seed = seed;
        RoughnessDigit = roughnessDigit;
        RockinessDigit = rockinessDigit;
        AsymmetryDigit = asymmetryDigit;
        FeatureScaleDigit = featureScaleDigit;
    }

    public int Seed { get; }

    public int RoughnessDigit { get; }

    public int RockinessDigit { get; }

    public int AsymmetryDigit { get; }

    public int FeatureScaleDigit { get; }

    public double Roughness => 0.35 + RoughnessDigit / 9.0 * 1.55;

    public double Rockiness => RockinessDigit / 9.0;

    public double Asymmetry => (AsymmetryDigit - 4.5) / 4.5;

    public double FeatureScale => 0.7 + FeatureScaleDigit / 9.0 * 2.1;

    public double Phase => Math.Abs((long)Seed % 1_000_000L) * 0.0113;

    public static SeedProfile FromSeed(int seed)
    {
        var absoluteSeed = Math.Abs((long)seed);

        return new SeedProfile(
            seed,
            (int)(absoluteSeed % 10),
            (int)(absoluteSeed / 10 % 10),
            (int)(absoluteSeed / 100 % 10),
            (int)(absoluteSeed / 1000 % 10));
    }
}
