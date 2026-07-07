using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

internal static class BoxMeshBuilder
{
    public static Mesh CreateBox(double width, double depth, double height, double bottomZ)
    {
        var mesh = new Mesh();
        var halfWidth = width / 2.0;
        var halfDepth = depth / 2.0;

        var nwb = new Vertex(-halfWidth, -halfDepth, bottomZ);
        var neb = new Vertex(halfWidth, -halfDepth, bottomZ);
        var seb = new Vertex(halfWidth, halfDepth, bottomZ);
        var swb = new Vertex(-halfWidth, halfDepth, bottomZ);
        var nwt = new Vertex(-halfWidth, -halfDepth, height);
        var net = new Vertex(halfWidth, -halfDepth, height);
        var set = new Vertex(halfWidth, halfDepth, height);
        var swt = new Vertex(-halfWidth, halfDepth, height);

        AddQuad(mesh, nwt, net, set, swt);
        AddQuad(mesh, swb, seb, neb, nwb);
        AddQuad(mesh, nwb, neb, net, nwt);
        AddQuad(mesh, neb, seb, set, net);
        AddQuad(mesh, seb, swb, swt, set);
        AddQuad(mesh, swb, nwb, nwt, swt);

        return mesh;
    }

    private static void AddQuad(Mesh mesh, Vertex a, Vertex b, Vertex c, Vertex d)
    {
        mesh.AddTriangle(a, b, c);
        mesh.AddTriangle(a, c, d);
    }
}
