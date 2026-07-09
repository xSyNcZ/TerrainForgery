using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public sealed class RockGenerator : ITerrainGenerator
{
    public Mesh Generate(HillGenerationSettings settings)
    {
        settings = TriangleBudgetResolver.Apply(settings);
        Validate(settings);

        var random = new Random(settings.Seed);
        var profile = TerrainStyleProfile.From(settings.Style);
        var seedProfile = SeedProfile.FromSeed(settings.Seed);
        var mesh = new Mesh();
        var top = new Vertex[settings.Resolution + 1, settings.Resolution + 1];
        var trimHeight = Math.Max(0.35, settings.Height * 0.1);
        var surfaceWidth = TerrainFootprint.GetSurfaceWidth(settings);
        var surfaceDepth = TerrainFootprint.GetSurfaceDepth(settings);

        for (var y = 0; y <= settings.Resolution; y++)
        {
            for (var x = 0; x <= settings.Resolution; x++)
            {
                var squareX = -1.0 + 2.0 * x / settings.Resolution;
                var squareY = -1.0 + 2.0 * y / settings.Resolution;
                var (normalizedX, normalizedY) = TerrainGridMesher.MapSquareToDisk(squareX, squareY);
                var worldX = normalizedX * surfaceWidth / 2.0;
                var worldY = normalizedY * surfaceDepth / 2.0;
                var shapedX = normalizedX + seedProfile.Asymmetry * 0.12;
                var shapedY = normalizedY - seedProfile.Asymmetry * 0.07;
                var radiusSquared = shapedX * shapedX + shapedY * shapedY;
                var angle = Math.Atan2(normalizedY, normalizedX);
                var angularLobes = 1.0
                    + 0.16 * Math.Sin(angle * (4.0 + seedProfile.FeatureScale) + settings.Seed * 0.01)
                    + 0.09 * Math.Cos(angle * (7.0 + seedProfile.FeatureScale) - settings.Seed * 0.017);
                var cap = Math.Pow(Math.Sqrt(Math.Max(0.0, 1.0 - radiusSquared * angularLobes)), profile.ShapeExponent);
                var ridge = Math.Abs(Math.Sin((normalizedX * 3.0 - normalizedY * 2.2 + settings.Seed * 0.003) * profile.FeatureScale * seedProfile.FeatureScale));
                var noise = ((random.NextDouble() * 2.0 - 1.0) * 0.35 * seedProfile.Roughness + ridge * (0.45 + seedProfile.Rockiness * 0.75))
                    * settings.NoiseStrength
                    * profile.NoiseMultiplier
                    * cap;
                var z = Math.Max(0.0, profile.ShapeHeight(settings.Height * profile.HeightMultiplier * cap + noise));
                z = ApplyEdgeFade(z, normalizedX, normalizedY);

                var vertex = new Vertex(worldX, worldY, z);
                top[x, y] = vertex;
            }
        }

        var activeCells = TerrainGridMesher.AddTopSurface(
            mesh,
            top,
            settings.Resolution,
            (_, _, _, _) => false);

        if (settings.IncludeBase)
        {
            TerrainGridMesher.AddRectangularBase(mesh, settings);
        }
        else
        {
            TerrainGridMesher.AddZeroBottomAndBoundarySides(mesh, top, activeCells, settings.Resolution);
        }

        return MeshBoundsFitter.FitToSettings(mesh, settings);
    }

    private static void Validate(HillGenerationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Resolution < 2)
        {
            throw new ArgumentException("Resolution must be at least 2.", nameof(settings));
        }

        if (settings.Width <= 0 || settings.Depth <= 0 || settings.Height <= 0)
        {
            throw new ArgumentException("Width, Depth, and Height must be greater than zero.", nameof(settings));
        }

        if (settings.OuterWallThickness < 0)
        {
            throw new ArgumentException("OuterWallThickness cannot be negative.", nameof(settings));
        }
    }

    private static double ApplyEdgeFade(double z, double normalizedX, double normalizedY)
    {
        var radius = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
        var fade = Math.Clamp((1.0 - radius) / 0.22, 0.0, 1.0);
        var smoothFade = fade * fade * (3.0 - 2.0 * fade);
        return z * smoothFade;
    }

    private static bool IsFlatCell(Vertex p00, Vertex p10, Vertex p01, Vertex p11, double trimHeight)
    {
        return p00.Z <= trimHeight
            && p10.Z <= trimHeight
            && p01.Z <= trimHeight
            && p11.Z <= trimHeight;
    }

}
