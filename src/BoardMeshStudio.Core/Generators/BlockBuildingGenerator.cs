using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public sealed class BlockBuildingGenerator : ITerrainGenerator
{
    public Mesh Generate(HillGenerationSettings settings)
    {
        settings = TerrainScaleCalculator.ApplyToDimensions(settings);
        Validate(settings);

        var mesh = new Mesh();
        var floorZ = -settings.EffectiveBaseThickness;
        var margin = Math.Clamp(settings.OuterWallThickness, 1.0, Math.Min(settings.Width, settings.Depth) * 0.2);
        var bodyWidth = Math.Max(settings.Width * 0.45, settings.Width - margin * 2.0);
        var bodyDepth = Math.Max(settings.Depth * 0.45, settings.Depth - margin * 2.0);
        var roofOverhang = Math.Min(margin * 0.45, Math.Min((settings.Width - bodyWidth) / 2.0, (settings.Depth - bodyDepth) / 2.0));
        var roofHeight = Math.Max(2.5, settings.Height * GetRoofRatio(settings.Style));
        var wallHeight = Math.Max(settings.Height * 0.45, settings.Height - roofHeight - 0.4);
        var bodyHeight = wallHeight - floorZ;

        BoxMeshBuilder.AddBox(mesh, 0.0, 0.0, floorZ, bodyWidth, bodyDepth, bodyHeight);
        BoxMeshBuilder.AddGableRoof(mesh, bodyWidth, bodyDepth, wallHeight, roofHeight, roofOverhang);
        AddFacadeDetails(mesh, settings, bodyWidth, bodyDepth, wallHeight);
        AddChimney(mesh, settings, bodyWidth, bodyDepth, wallHeight, roofHeight);

        return MeshBoundsFitter.FitToSettings(mesh, settings);
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

    private static void AddFacadeDetails(Mesh mesh, HillGenerationSettings settings, double bodyWidth, double bodyDepth, double wallHeight)
    {
        var detailDepth = Math.Min(0.7, Math.Max(0.25, settings.Depth * 0.025));
        var frontY = -bodyDepth / 2.0 - detailDepth / 2.0;
        var backY = bodyDepth / 2.0 + detailDepth / 2.0;
        var miniatureHeight = Math.Max(4.0, settings.ReferenceMiniatureHeight);
        var doorWidth = Math.Clamp(miniatureHeight * 0.28, bodyWidth * 0.12, bodyWidth * 0.32);
        var doorHeight = Math.Clamp(miniatureHeight * 0.68, wallHeight * 0.35, wallHeight * 0.78);
        var windowWidth = Math.Clamp(miniatureHeight * 0.22, bodyWidth * 0.09, bodyWidth * 0.24);
        var windowHeight = Math.Clamp(miniatureHeight * 0.22, wallHeight * 0.12, wallHeight * 0.28);
        var windowZ = Math.Max(doorHeight * 0.75, wallHeight * 0.48);

        BoxMeshBuilder.AddBox(mesh, 0.0, frontY, 0.0, doorWidth, detailDepth, doorHeight);
        BoxMeshBuilder.AddBox(mesh, -bodyWidth * 0.27, frontY, windowZ, windowWidth, detailDepth, windowHeight);
        BoxMeshBuilder.AddBox(mesh, bodyWidth * 0.27, frontY, windowZ, windowWidth, detailDepth, windowHeight);
        BoxMeshBuilder.AddBox(mesh, -bodyWidth * 0.25, backY, windowZ, windowWidth, detailDepth, windowHeight);
        BoxMeshBuilder.AddBox(mesh, bodyWidth * 0.25, backY, windowZ, windowWidth, detailDepth, windowHeight);

        var sideWindowX = bodyWidth / 2.0 + detailDepth / 2.0;
        AddSideWindow(mesh, sideWindowX, -bodyDepth * 0.22, windowZ, windowWidth, windowHeight, detailDepth);
        AddSideWindow(mesh, sideWindowX, bodyDepth * 0.22, windowZ, windowWidth, windowHeight, detailDepth);
        AddSideWindow(mesh, -sideWindowX, -bodyDepth * 0.22, windowZ, windowWidth, windowHeight, detailDepth);
        AddSideWindow(mesh, -sideWindowX, bodyDepth * 0.22, windowZ, windowWidth, windowHeight, detailDepth);
    }

    private static void AddSideWindow(Mesh mesh, double x, double y, double z, double width, double height, double detailDepth)
    {
        BoxMeshBuilder.AddBox(mesh, x, y, z, detailDepth, width, height);
    }

    private static void AddChimney(Mesh mesh, HillGenerationSettings settings, double bodyWidth, double bodyDepth, double wallHeight, double roofHeight)
    {
        var chimneyWidth = Math.Max(2.0, bodyWidth * 0.08);
        var chimneyDepth = Math.Max(2.0, bodyDepth * 0.08);
        var chimneyHeight = Math.Min(Math.Max(4.0, roofHeight * 0.7), settings.Height - wallHeight - roofHeight * 0.15);
        BoxMeshBuilder.AddBox(
            mesh,
            bodyWidth * 0.26,
            bodyDepth * 0.12,
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

        if (settings.BaseThickness < 0 || settings.OuterWallThickness < 0)
        {
            throw new ArgumentException("BaseThickness and OuterWallThickness cannot be negative.", nameof(settings));
        }
    }
}
