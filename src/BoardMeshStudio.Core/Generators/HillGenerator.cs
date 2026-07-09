using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public sealed class HillGenerator : ITerrainGenerator
{
    public Mesh Generate(HillGenerationSettings settings)
    {
        settings = TriangleBudgetResolver.Apply(settings);
        Validate(settings);

        var random = new Random(settings.Seed);
        var top = CreateTopSurface(settings, random);
        var mesh = new Mesh();

        var activeCells = AddTopSurface(mesh, top, settings, trimFlatCells: false, GetTrimHeight(settings));
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

        if (settings.NoiseStrength < 0)
        {
            throw new ArgumentException("NoiseStrength cannot be negative.", nameof(settings));
        }

        if (settings.BaseThickness < 0 || settings.OuterWallThickness < 0)
        {
            throw new ArgumentException("BaseThickness and OuterWallThickness cannot be negative.", nameof(settings));
        }
    }

    private static Vertex[,] CreateTopSurface(HillGenerationSettings settings, Random random)
    {
        var points = new Vertex[settings.Resolution + 1, settings.Resolution + 1];
        var profile = TerrainStyleProfile.From(settings.Style);
        var seedProfile = SeedProfile.FromSeed(settings.Seed);
        var asymmetryX = seedProfile.Asymmetry * 0.16;
        var asymmetryY = -seedProfile.Asymmetry * 0.09;
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
                var shapedX = normalizedX + asymmetryX;
                var shapedY = normalizedY + asymmetryY;
                var ovalBias = 1.0 + seedProfile.Asymmetry * 0.18;
                var radialDistanceSquared = shapedX * shapedX * Math.Max(0.72, ovalBias) + shapedY * shapedY / Math.Max(0.72, ovalBias);
                var baseFalloff = Math.Pow(Math.Max(0.0, 1.0 - radialDistanceSquared), profile.ShapeExponent);
                var shoulder = 0.18 * Math.Exp(-Math.Pow((Math.Sqrt(radialDistanceSquared) - 0.55) * 3.2, 2.0));
                var smoothNoise = SmoothTerrainNoise(normalizedX, normalizedY, settings.Seed, profile.FeatureScale * seedProfile.FeatureScale);
                var ridge = Math.Abs(Math.Sin((normalizedX * 5.1 - normalizedY * 3.4 + seedProfile.Phase) * seedProfile.FeatureScale));
                var falloff = profile.ShapeHeight(baseFalloff + shoulder * profile.Smoothing);
                var noise = (smoothNoise * seedProfile.Roughness + ridge * seedProfile.Rockiness * 0.55)
                    * settings.NoiseStrength
                    * profile.NoiseMultiplier
                    * falloff;
                var z = Math.Max(0.0, settings.Height * profile.HeightMultiplier * falloff + noise);
                z = ApplyEdgeFade(z, normalizedX, normalizedY);

                var vertex = new Vertex(worldX, worldY, z);
                points[x, y] = vertex;
            }
        }

        return points;
    }

    private static Vertex[,] CreateBottomSurface(HillGenerationSettings settings)
    {
        var points = new Vertex[settings.Resolution + 1, settings.Resolution + 1];
        var bottomZ = -settings.EffectiveBaseThickness;

        for (var y = 0; y <= settings.Resolution; y++)
        {
            for (var x = 0; x <= settings.Resolution; x++)
            {
                var worldX = -settings.Width / 2.0 + settings.Width * x / settings.Resolution;
                var worldY = -settings.Depth / 2.0 + settings.Depth * y / settings.Resolution;

                points[x, y] = new Vertex(worldX, worldY, bottomZ);
            }
        }

        return points;
    }

    private static double SmoothTerrainNoise(double x, double y, int seed, double featureScale)
    {
        var seedOffset = seed * 0.013;
        var coarse = Math.Sin((x * 2.1 + seedOffset) * featureScale) * Math.Cos((y * 1.7 - seedOffset) * featureScale);
        var fine = Math.Sin((x * 6.3 - seedOffset) * featureScale + Math.Cos(y * 2.0)) * 0.35;
        return (coarse + fine) / 1.35;
    }

    private static double GetTrimHeight(HillGenerationSettings settings)
    {
        return Math.Max(0.12, settings.Height * 0.025);
    }

    private static double ApplyEdgeFade(double z, double normalizedX, double normalizedY)
    {
        var radius = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
        var fade = Math.Clamp((1.0 - radius) / 0.28, 0.0, 1.0);
        var smoothFade = fade * fade * (3.0 - 2.0 * fade);
        return z * smoothFade;
    }

    private static bool[,] AddTopSurface(Mesh mesh, Vertex[,] top, HillGenerationSettings settings, bool trimFlatCells, double trimHeight)
    {
        return TerrainGridMesher.AddTopSurface(
            mesh,
            top,
            settings.Resolution,
            (p00, p10, p01, p11) => trimFlatCells && IsFlatCell(p00, p10, p01, p11, trimHeight));
    }

    private static bool IsFlatCell(Vertex p00, Vertex p10, Vertex p01, Vertex p11, double trimHeight)
    {
        return p00.Z <= trimHeight
            && p10.Z <= trimHeight
            && p01.Z <= trimHeight
            && p11.Z <= trimHeight;
    }

    private static void AddBottomSurface(Mesh mesh, Vertex[,] bottom, int resolution)
    {
        for (var y = 0; y < resolution; y++)
        {
            for (var x = 0; x < resolution; x++)
            {
                var p00 = bottom[x, y];
                var p10 = bottom[x + 1, y];
                var p01 = bottom[x, y + 1];
                var p11 = bottom[x + 1, y + 1];

                mesh.AddTriangle(p00, p11, p10);
                mesh.AddTriangle(p00, p01, p11);
            }
        }
    }

    private static void AddSides(Mesh mesh, Vertex[,] top, Vertex[,] bottom, int resolution)
    {
        for (var x = 0; x < resolution; x++)
        {
            AddQuad(mesh, top[x, 0], bottom[x, 0], bottom[x + 1, 0], top[x + 1, 0]);
            AddQuad(mesh, top[x + 1, resolution], bottom[x + 1, resolution], bottom[x, resolution], top[x, resolution]);
        }

        for (var y = 0; y < resolution; y++)
        {
            AddQuad(mesh, top[0, y + 1], bottom[0, y + 1], bottom[0, y], top[0, y]);
            AddQuad(mesh, top[resolution, y], bottom[resolution, y], bottom[resolution, y + 1], top[resolution, y + 1]);
        }
    }

    private static void AddQuad(Mesh mesh, Vertex topA, Vertex bottomA, Vertex bottomB, Vertex topB)
    {
        mesh.AddTriangle(topA, bottomA, bottomB);
        mesh.AddTriangle(topA, bottomB, topB);
    }
}
