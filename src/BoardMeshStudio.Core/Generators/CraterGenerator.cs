using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

public sealed class CraterGenerator : ITerrainGenerator
{
    public Mesh Generate(HillGenerationSettings settings)
    {
        settings = TriangleBudgetResolver.Apply(settings);
        Validate(settings);

        var random = new Random(settings.Seed);
        var top = CreateTopSurface(settings, random);
        var bottom = CreateBottomSurface(settings);
        var mesh = new Mesh();

        AddTopSurface(mesh, top, settings.Resolution);
        AddBottomSurface(mesh, bottom, settings.Resolution);
        AddSides(mesh, top, bottom, settings.Resolution);

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

        if (settings.NoiseStrength < 0 || settings.BaseThickness < 0)
        {
            throw new ArgumentException("NoiseStrength and BaseThickness cannot be negative.", nameof(settings));
        }
    }

    private static Vertex[,] CreateTopSurface(HillGenerationSettings settings, Random random)
    {
        var points = new Vertex[settings.Resolution + 1, settings.Resolution + 1];
        var profile = TerrainStyleProfile.From(settings.Style);

        for (var y = 0; y <= settings.Resolution; y++)
        {
            for (var x = 0; x <= settings.Resolution; x++)
            {
                var worldX = -settings.Width / 2.0 + settings.Width * x / settings.Resolution;
                var worldY = -settings.Depth / 2.0 + settings.Depth * y / settings.Resolution;
                var normalizedX = worldX / (settings.Width / 2.0);
                var normalizedY = worldY / (settings.Depth / 2.0);
                var radius = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                var rim = settings.Height * profile.HeightMultiplier * 0.35 * Math.Exp(-Math.Pow((radius - 0.68) * 5.0, 2.0));
                var bowlShape = Math.Pow(Math.Max(0.0, 1.0 - radius * radius), profile.ShapeExponent);
                var bowl = settings.Height * profile.HeightMultiplier * 0.28 * bowlShape;
                var edgeBlend = Math.Max(0.0, 1.0 - radius);
                var noise = (random.NextDouble() * 2.0 - 1.0) * settings.NoiseStrength * profile.NoiseMultiplier * edgeBlend;
                var minimumZ = settings.IncludeBase ? -settings.BaseThickness * 0.5 : 0.0;
                var z = Math.Max(minimumZ, rim - bowl + noise);

                points[x, y] = new Vertex(worldX, worldY, z);
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

    private static void AddTopSurface(Mesh mesh, Vertex[,] top, int resolution)
    {
        for (var y = 0; y < resolution; y++)
        {
            for (var x = 0; x < resolution; x++)
            {
                var p00 = top[x, y];
                var p10 = top[x + 1, y];
                var p01 = top[x, y + 1];
                var p11 = top[x + 1, y + 1];
                mesh.AddTriangle(p00, p10, p11);
                mesh.AddTriangle(p00, p11, p01);
            }
        }
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
