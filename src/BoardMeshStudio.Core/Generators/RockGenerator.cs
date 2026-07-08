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
        var bottom = settings.IncludeBase ? new Vertex[settings.Resolution + 1, settings.Resolution + 1] : null;
        var trimHeight = Math.Max(0.35, settings.Height * 0.1);

        for (var y = 0; y <= settings.Resolution; y++)
        {
            for (var x = 0; x <= settings.Resolution; x++)
            {
                var worldX = -settings.Width / 2.0 + settings.Width * x / settings.Resolution;
                var worldY = -settings.Depth / 2.0 + settings.Depth * y / settings.Resolution;
                var normalizedX = worldX / (settings.Width / 2.0);
                var normalizedY = worldY / (settings.Depth / 2.0);
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

                top[x, y] = new Vertex(worldX, worldY, z);
                if (bottom is not null)
                {
                    bottom[x, y] = new Vertex(worldX, worldY, -settings.EffectiveBaseThickness);
                }
            }
        }

        for (var y = 0; y < settings.Resolution; y++)
        {
            for (var x = 0; x < settings.Resolution; x++)
            {
                var p00 = top[x, y];
                var p10 = top[x + 1, y];
                var p01 = top[x, y + 1];
                var p11 = top[x + 1, y + 1];

                if (bottom is null && (IsFlatCell(p00, p10, p01, p11, trimHeight) || IsOutsideFootprint(p00, p10, p01, p11, settings.Width, settings.Depth)))
                {
                    continue;
                }

                mesh.AddTriangle(p00, p10, p11);
                mesh.AddTriangle(p00, p11, p01);
                if (bottom is not null)
                {
                    mesh.AddTriangle(bottom[x, y], bottom[x + 1, y + 1], bottom[x + 1, y]);
                    mesh.AddTriangle(bottom[x, y], bottom[x, y + 1], bottom[x + 1, y + 1]);
                }
            }
        }

        if (bottom is not null)
        {
            for (var x = 0; x < settings.Resolution; x++)
            {
                AddQuad(mesh, top[x, 0], bottom[x, 0], bottom[x + 1, 0], top[x + 1, 0]);
                AddQuad(mesh, top[x + 1, settings.Resolution], bottom[x + 1, settings.Resolution], bottom[x, settings.Resolution], top[x, settings.Resolution]);
            }

            for (var y = 0; y < settings.Resolution; y++)
            {
                AddQuad(mesh, top[0, y + 1], bottom[0, y + 1], bottom[0, y], top[0, y]);
                AddQuad(mesh, top[settings.Resolution, y], bottom[settings.Resolution, y], bottom[settings.Resolution, y + 1], top[settings.Resolution, y + 1]);
            }
        }

        return mesh;
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
    }

    private static void AddQuad(Mesh mesh, Vertex topA, Vertex bottomA, Vertex bottomB, Vertex topB)
    {
        mesh.AddTriangle(topA, bottomA, bottomB);
        mesh.AddTriangle(topA, bottomB, topB);
    }

    private static bool IsFlatCell(Vertex p00, Vertex p10, Vertex p01, Vertex p11, double trimHeight)
    {
        return p00.Z <= trimHeight
            && p10.Z <= trimHeight
            && p01.Z <= trimHeight
            && p11.Z <= trimHeight;
    }

    private static bool IsOutsideFootprint(Vertex p00, Vertex p10, Vertex p01, Vertex p11, double width, double depth)
    {
        var centerX = (p00.X + p10.X + p01.X + p11.X) / 4.0 / (width / 2.0);
        var centerY = (p00.Y + p10.Y + p01.Y + p11.Y) / 4.0 / (depth / 2.0);
        return centerX * centerX + centerY * centerY > 1.0;
    }
}
