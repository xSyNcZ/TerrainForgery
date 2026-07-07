namespace TerrainForgery.Core.Geometry;

public sealed class Triangle
{
    public Triangle(Vertex a, Vertex b, Vertex c)
    {
        A = a;
        B = b;
        C = c;
    }

    public Vertex A { get; }

    public Vertex B { get; }

    public Vertex C { get; }
}
