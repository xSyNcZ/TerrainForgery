using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public sealed class BlockBuildingGenerator : ITerrainGenerator
{
    public Mesh Generate(HillGenerationSettings settings)
    {
        Validate(settings);

        var profile = TerrainStyleProfile.From(settings.Style);
        var mesh = new Mesh();
        var floorZ = -settings.EffectiveBaseThickness;
        var wallHeight = settings.Height * profile.HeightMultiplier;
        var roofHeight = Math.Max(4.0, wallHeight * GetRoofRatio(settings.Style));

        BoxMeshBuilder.AddBox(mesh, 0.0, 0.0, floorZ, settings.Width, settings.Depth, wallHeight - floorZ);
        BoxMeshBuilder.AddGableRoof(mesh, settings.Width, settings.Depth, wallHeight, roofHeight, Math.Max(1.0, settings.Width * 0.04));
        AddFacadeDetails(mesh, settings, wallHeight);
        AddChimney(mesh, settings, wallHeight, roofHeight);

        return mesh;
    }

    private static double GetRoofRatio(TerrainStyle style)
    {
        return style switch
        {
            TerrainStyle.AnimeInspired => 0.45,
            TerrainStyle.Stylized => 0.38,
            TerrainStyle.LowPoly => 0.28,
            TerrainStyle.RuggedNatural => 0.34,
            TerrainStyle.MiniatureFriendly => 0.32,
            _ => 0.30
        };
    }

    private static void AddFacadeDetails(Mesh mesh, HillGenerationSettings settings, double wallHeight)
    {
        var frontY = -settings.Depth / 2.0 - 0.35;
        var backY = settings.Depth / 2.0 + 0.35;
        var doorWidth = Math.Max(5.0, settings.Width * 0.16);
        var doorHeight = Math.Max(9.0, wallHeight * 0.42);
        var windowWidth = Math.Max(4.0, settings.Width * 0.13);
        var windowHeight = Math.Max(4.0, wallHeight * 0.18);
        var windowZ = Math.Max(doorHeight * 0.75, wallHeight * 0.48);

        BoxMeshBuilder.AddBox(mesh, 0.0, frontY, 0.0, doorWidth, 0.7, doorHeight);
        BoxMeshBuilder.AddBox(mesh, -settings.Width * 0.28, frontY, windowZ, windowWidth, 0.7, windowHeight);
        BoxMeshBuilder.AddBox(mesh, settings.Width * 0.28, frontY, windowZ, windowWidth, 0.7, windowHeight);
        BoxMeshBuilder.AddBox(mesh, -settings.Width * 0.25, backY, windowZ, windowWidth, 0.7, windowHeight);
        BoxMeshBuilder.AddBox(mesh, settings.Width * 0.25, backY, windowZ, windowWidth, 0.7, windowHeight);

        var sideWindowX = settings.Width / 2.0 + 0.35;
        AddSideWindow(mesh, sideWindowX, -settings.Depth * 0.22, windowZ, windowWidth, windowHeight);
        AddSideWindow(mesh, sideWindowX, settings.Depth * 0.22, windowZ, windowWidth, windowHeight);
        AddSideWindow(mesh, -sideWindowX, -settings.Depth * 0.22, windowZ, windowWidth, windowHeight);
        AddSideWindow(mesh, -sideWindowX, settings.Depth * 0.22, windowZ, windowWidth, windowHeight);
    }

    private static void AddSideWindow(Mesh mesh, double x, double y, double z, double width, double height)
    {
        BoxMeshBuilder.AddBox(mesh, x, y, z, 0.7, width, height);
    }

    private static void AddChimney(Mesh mesh, HillGenerationSettings settings, double wallHeight, double roofHeight)
    {
        var chimneyWidth = Math.Max(3.0, settings.Width * 0.08);
        var chimneyDepth = Math.Max(3.0, settings.Depth * 0.08);
        var chimneyHeight = Math.Max(8.0, roofHeight * 0.75);
        BoxMeshBuilder.AddBox(
            mesh,
            settings.Width * 0.28,
            settings.Depth * 0.15,
            wallHeight + roofHeight * 0.25,
            chimneyWidth,
            chimneyDepth,
            chimneyHeight);
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
