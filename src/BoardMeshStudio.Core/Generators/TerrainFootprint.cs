namespace BoardMeshStudio.Core.Generators;

internal static class TerrainFootprint
{
    public static double GetSurfaceWidth(HillGenerationSettings settings)
    {
        return settings.IncludeBase ? ShrinkByWall(settings.Width, GetSafeWallThickness(settings)) : settings.Width;
    }

    public static double GetSurfaceDepth(HillGenerationSettings settings)
    {
        return settings.IncludeBase ? ShrinkByWall(settings.Depth, GetSafeWallThickness(settings)) : settings.Depth;
    }

    public static double GetSafeWallThickness(HillGenerationSettings settings)
    {
        var maxThickness = Math.Min(settings.Width, settings.Depth) * 0.35;
        return Math.Clamp(settings.OuterWallThickness, 0.0, maxThickness);
    }

    private static double ShrinkByWall(double size, double wallThickness)
    {
        return Math.Max(size * 0.2, size - wallThickness * 2.0);
    }
}
