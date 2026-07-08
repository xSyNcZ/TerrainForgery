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
        var mesh = new Mesh();
        var top = new Vertex[settings.Resolution + 1, settings.Resolution + 1];
        var bottom = new Vertex[settings.Resolution + 1, settings.Resolution + 1];

        for (var y = 0; y <= settings.Resolution; y++)
        {
            for (var x = 0; x <= settings.Resolution; x++)
            {
                var worldX = -settings.Width / 2.0 + settings.Width * x / settings.Resolution;
                var worldY = -settings.Depth / 2.0 + settings.Depth * y / settings.Resolution;
                var normalizedX = worldX / (settings.Width / 2.0);
                var normalizedY = worldY / (settings.Depth / 2.0);
                var radiusSquared = normalizedX * normalizedX + normalizedY * normalizedY;
                var cap = Math.Pow(Math.Sqrt(Math.Max(0.0, 1.0 - radiusSquared)), profile.ShapeExponent);
                var noise = (random.NextDouble() * 2.0 - 1.0) * settings.NoiseStrength * profile.NoiseMultiplier * cap;
                var z = Math.Max(0.0, settings.Height * profile.HeightMultiplier * cap + noise);

                top[x, y] = new Vertex(worldX, worldY, z);
                bottom[x, y] = new Vertex(worldX, worldY, -settings.EffectiveBaseThickness);
            }
        }

        for (var y = 0; y < settings.Resolution; y++)
        {
            for (var x = 0; x < settings.Resolution; x++)
            {
                mesh.AddTriangle(top[x, y], top[x + 1, y], top[x + 1, y + 1]);
                mesh.AddTriangle(top[x, y], top[x + 1, y + 1], top[x, y + 1]);
                mesh.AddTriangle(bottom[x, y], bottom[x + 1, y + 1], bottom[x + 1, y]);
                mesh.AddTriangle(bottom[x, y], bottom[x, y + 1], bottom[x + 1, y + 1]);
            }
        }

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
}
