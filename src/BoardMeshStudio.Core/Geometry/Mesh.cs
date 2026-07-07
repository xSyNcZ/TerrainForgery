namespace BoardMeshStudio.Core.Geometry;

public sealed class Mesh
{
    public List<Triangle> Triangles { get; } = new();

    public bool HasTriangles => Triangles.Count > 0;

    public void AddTriangle(Vertex a, Vertex b, Vertex c)
    {
        Triangles.Add(new Triangle(a, b, c));
    }
}
