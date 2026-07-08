using BoardMeshStudio.Core.Geometry;

namespace BoardMeshStudio.Core.Generators;

internal static class BoxMeshBuilder
{
    public static Mesh CreateBox(double width, double depth, double height, double bottomZ)
    {
        var mesh = new Mesh();
        AddBox(mesh, 0.0, 0.0, bottomZ, width, depth, height - bottomZ);
        return mesh;
    }

    public static void AddBox(Mesh mesh, double centerX, double centerY, double bottomZ, double width, double depth, double height)
    {
        var halfWidth = width / 2.0;
        var halfDepth = depth / 2.0;
        var topZ = bottomZ + height;

        var nwb = new Vertex(centerX - halfWidth, centerY - halfDepth, bottomZ);
        var neb = new Vertex(centerX + halfWidth, centerY - halfDepth, bottomZ);
        var seb = new Vertex(centerX + halfWidth, centerY + halfDepth, bottomZ);
        var swb = new Vertex(centerX - halfWidth, centerY + halfDepth, bottomZ);
        var nwt = new Vertex(centerX - halfWidth, centerY - halfDepth, topZ);
        var net = new Vertex(centerX + halfWidth, centerY - halfDepth, topZ);
        var set = new Vertex(centerX + halfWidth, centerY + halfDepth, topZ);
        var swt = new Vertex(centerX - halfWidth, centerY + halfDepth, topZ);

        AddQuad(mesh, nwt, net, set, swt);
        AddQuad(mesh, swb, seb, neb, nwb);
        AddQuad(mesh, nwb, neb, net, nwt);
        AddQuad(mesh, neb, seb, set, net);
        AddQuad(mesh, seb, swb, swt, set);
        AddQuad(mesh, swb, nwb, nwt, swt);
    }

    public static void AddGableRoof(Mesh mesh, double width, double depth, double wallTopZ, double roofHeight, double overhang)
    {
        var halfWidth = width / 2.0 + overhang;
        var halfDepth = depth / 2.0 + overhang;
        var ridgeZ = wallTopZ + roofHeight;

        var nw = new Vertex(-halfWidth, -halfDepth, wallTopZ);
        var ne = new Vertex(halfWidth, -halfDepth, wallTopZ);
        var sw = new Vertex(-halfWidth, halfDepth, wallTopZ);
        var se = new Vertex(halfWidth, halfDepth, wallTopZ);
        var rn = new Vertex(0.0, -halfDepth, ridgeZ);
        var rs = new Vertex(0.0, halfDepth, ridgeZ);

        AddQuad(mesh, nw, rn, rs, sw);
        AddQuad(mesh, rn, ne, se, rs);
        mesh.AddTriangle(nw, ne, rn);
        mesh.AddTriangle(sw, rs, se);
    }

    public static void AddQuad(Mesh mesh, Vertex a, Vertex b, Vertex c, Vertex d)
    {
        mesh.AddTriangle(a, b, c);
        mesh.AddTriangle(a, c, d);
    }
}
