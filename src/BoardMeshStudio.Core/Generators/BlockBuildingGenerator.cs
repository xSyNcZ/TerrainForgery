using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public sealed class BlockBuildingGenerator : ITerrainGenerator
{
    public Mesh Generate(HillGenerationSettings settings)
    {
        Validate(settings);

        return BoxMeshBuilder.CreateBox(settings.Width, settings.Depth, settings.Height, -settings.BaseThickness);
    }

    private static void Validate(HillGenerationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Width <= 0 || settings.Depth <= 0 || settings.Height <= 0)
        {
            throw new ArgumentException("Width, Depth, and Height must be greater than zero.", nameof(settings));
        }

        if (settings.BaseThickness < 0)
        {
            throw new ArgumentException("BaseThickness cannot be negative.", nameof(settings));
        }
    }
}
