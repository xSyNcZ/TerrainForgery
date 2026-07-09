using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

internal static class TerrainGridMesher
{
    public static Vertex CreateFootprintVertex(double squareX, double squareY, double width, double depth, double z)
    {
        var (diskX, diskY) = MapSquareToDisk(squareX, squareY);
        return new Vertex(diskX * width / 2.0, diskY * depth / 2.0, z);
    }

    public static (double X, double Y) MapSquareToDisk(double squareX, double squareY)
    {
        if (Math.Abs(squareX) < 0.000001 && Math.Abs(squareY) < 0.000001)
        {
            return (0.0, 0.0);
        }

        var absX = Math.Abs(squareX);
        var absY = Math.Abs(squareY);
        double radius;
        double angle;

        if (absX > absY)
        {
            radius = absX;
            angle = Math.PI / 4.0 * (squareY / squareX);
            if (squareX < 0.0)
            {
                angle += Math.PI;
            }
        }
        else
        {
            radius = absY;
            angle = Math.PI / 2.0 - Math.PI / 4.0 * (squareX / squareY);
            if (squareY < 0.0)
            {
                angle += Math.PI;
            }
        }

        return (radius * Math.Cos(angle), radius * Math.Sin(angle));
    }

    public static void AddRectangularBase(Mesh mesh, HillGenerationSettings settings)
    {
        var top = CreateFlatSurface(settings.Width, settings.Depth, settings.Resolution, 0.0);
        var bottom = CreateFlatSurface(settings.Width, settings.Depth, settings.Resolution, -settings.EffectiveBaseThickness);

        AddTopSurface(mesh, top, settings.Resolution, (_, _, _, _) => false);
        AddBottomSurface(mesh, bottom, settings.Resolution);
        AddOuterSides(mesh, top, bottom, settings.Resolution);
    }

    public static bool[,] AddTopSurface(
        Mesh mesh,
        Vertex[,] top,
        int resolution,
        Func<Vertex, Vertex, Vertex, Vertex, bool> shouldSkipCell)
    {
        var activeCells = new bool[resolution, resolution];

        for (var y = 0; y < resolution; y++)
        {
            for (var x = 0; x < resolution; x++)
            {
                var p00 = top[x, y];
                var p10 = top[x + 1, y];
                var p01 = top[x, y + 1];
                var p11 = top[x + 1, y + 1];

                if (shouldSkipCell(p00, p10, p01, p11))
                {
                    continue;
                }

                activeCells[x, y] = true;
                mesh.AddTriangle(p00, p10, p11);
                mesh.AddTriangle(p00, p11, p01);
            }
        }

        return activeCells;
    }

    public static void AddZeroBottomAndBoundarySides(Mesh mesh, Vertex[,] top, bool[,] activeCells, int resolution)
    {
        for (var y = 0; y < resolution; y++)
        {
            for (var x = 0; x < resolution; x++)
            {
                if (!activeCells[x, y])
                {
                    continue;
                }

                var p00 = top[x, y];
                var p10 = top[x + 1, y];
                var p01 = top[x, y + 1];
                var p11 = top[x + 1, y + 1];
                var b00 = ToZero(p00);
                var b10 = ToZero(p10);
                var b01 = ToZero(p01);
                var b11 = ToZero(p11);

                mesh.AddTriangle(b00, b11, b10);
                mesh.AddTriangle(b00, b01, b11);

                if (y == 0 || !activeCells[x, y - 1])
                {
                    AddBoundaryQuad(mesh, p00, b00, b10, p10);
                }

                if (y == resolution - 1 || !activeCells[x, y + 1])
                {
                    AddBoundaryQuad(mesh, p11, b11, b01, p01);
                }

                if (x == 0 || !activeCells[x - 1, y])
                {
                    AddBoundaryQuad(mesh, p01, b01, b00, p00);
                }

                if (x == resolution - 1 || !activeCells[x + 1, y])
                {
                    AddBoundaryQuad(mesh, p10, b10, b11, p11);
                }
            }
        }
    }

    public static Vertex ProjectToNoBaseFootprint(Vertex vertex, double normalizedX, double normalizedY)
    {
        var radius = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
        if (radius <= 1.0)
        {
            return vertex;
        }

        return new Vertex(vertex.X / radius, vertex.Y / radius, 0.0);
    }

    private static Vertex ToZero(Vertex vertex)
    {
        return new Vertex(vertex.X, vertex.Y, 0.0);
    }

    private static Vertex[,] CreateFlatSurface(double width, double depth, int resolution, double z)
    {
        var points = new Vertex[resolution + 1, resolution + 1];

        for (var y = 0; y <= resolution; y++)
        {
            for (var x = 0; x <= resolution; x++)
            {
                var worldX = -width / 2.0 + width * x / resolution;
                var worldY = -depth / 2.0 + depth * y / resolution;
                points[x, y] = new Vertex(worldX, worldY, z);
            }
        }

        return points;
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

    private static void AddOuterSides(Mesh mesh, Vertex[,] top, Vertex[,] bottom, int resolution)
    {
        for (var x = 0; x < resolution; x++)
        {
            AddBoundaryQuad(mesh, top[x, 0], bottom[x, 0], bottom[x + 1, 0], top[x + 1, 0]);
            AddBoundaryQuad(mesh, top[x + 1, resolution], bottom[x + 1, resolution], bottom[x, resolution], top[x, resolution]);
        }

        for (var y = 0; y < resolution; y++)
        {
            AddBoundaryQuad(mesh, top[0, y + 1], bottom[0, y + 1], bottom[0, y], top[0, y]);
            AddBoundaryQuad(mesh, top[resolution, y], bottom[resolution, y], bottom[resolution, y + 1], top[resolution, y + 1]);
        }
    }

    private static void AddBoundaryQuad(Mesh mesh, Vertex topA, Vertex bottomA, Vertex bottomB, Vertex topB)
    {
        if (Math.Abs(topA.Z) <= 0.0001
            && Math.Abs(topB.Z) <= 0.0001
            && Math.Abs(bottomA.Z) <= 0.0001
            && Math.Abs(bottomB.Z) <= 0.0001)
        {
            return;
        }

        mesh.AddTriangle(topA, bottomA, bottomB);
        mesh.AddTriangle(topA, bottomB, topB);
    }
}
