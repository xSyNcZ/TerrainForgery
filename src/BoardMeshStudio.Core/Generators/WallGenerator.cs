using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public sealed class WallGenerator : ITerrainGenerator
{
    public Mesh Generate(HillGenerationSettings settings)
    {
        Validate(settings);

        var profile = TerrainStyleProfile.From(settings.Style);
        var wallDepth = Math.Max(2.0, settings.Depth * 0.18);
        var mesh = BoxMeshBuilder.CreateBox(settings.Width, wallDepth, settings.Height * profile.HeightMultiplier, -settings.EffectiveBaseThickness);
        return MeshBoundsFitter.FitToSettings(mesh, settings);
    }

    private static void Validate(HillGenerationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Width <= 0 || settings.Depth <= 0 || settings.Height <= 0)
        {
            throw new ArgumentException("Width, Depth, and Height must be greater than zero.", nameof(settings));
        }

        if (settings.BaseThickness < 0 || settings.OuterWallThickness < 0)
        {
            throw new ArgumentException("BaseThickness and OuterWallThickness cannot be negative.", nameof(settings));
        }
    }
}
