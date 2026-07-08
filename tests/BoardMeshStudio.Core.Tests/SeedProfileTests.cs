using BoardMeshStudio.Core.Generators;

namespace BoardMeshStudio.Core.Tests;

public sealed class SeedProfileTests
{
    [Fact]
    public void FromSeed_MapsDigitsToTerrainControls()
    {
        var profile = SeedProfile.FromSeed(4321);

        Assert.Equal(1, profile.RoughnessDigit);
        Assert.Equal(2, profile.RockinessDigit);
        Assert.Equal(3, profile.AsymmetryDigit);
        Assert.Equal(4, profile.FeatureScaleDigit);
    }

    [Fact]
    public void FromSeed_UsesAbsoluteValueForNegativeSeeds()
    {
        var profile = SeedProfile.FromSeed(-9876);

        Assert.Equal(6, profile.RoughnessDigit);
        Assert.Equal(7, profile.RockinessDigit);
        Assert.Equal(8, profile.AsymmetryDigit);
        Assert.Equal(9, profile.FeatureScaleDigit);
    }
}
