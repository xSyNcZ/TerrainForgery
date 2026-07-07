using BoardMeshStudio.Core.Generators;

namespace BoardMeshStudio.Core.Tests;

public sealed class HillGeneratorTests
{
    [Fact]
    public void Generate_ReturnsMeshWithTriangles()
    {
        var settings = new HillGenerationSettings
        {
            Width = 20.0,
            Depth = 20.0,
            Height = 6.0,
            Resolution = 8,
            NoiseStrength = 0.5,
            Seed = 42,
            BaseThickness = 2.0
        };

        var generator = new HillGenerator();

        var mesh = generator.Generate(settings);

        Assert.True(mesh.HasTriangles);
        Assert.NotEmpty(mesh.Triangles);
    }

    [Fact]
    public void Generate_ThrowsException_WhenResolutionIsLessThanTwo()
    {
        var settings = new HillGenerationSettings
        {
            Resolution = 1
        };

        var generator = new HillGenerator();

        Assert.Throws<ArgumentException>(() => generator.Generate(settings));
    }
}
