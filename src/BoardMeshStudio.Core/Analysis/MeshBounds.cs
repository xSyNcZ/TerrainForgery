namespace BoardMeshStudio.Core.Analysis;

public sealed class MeshBounds
{
    public MeshBounds(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
    {
        MinX = minX;
        MinY = minY;
        MinZ = minZ;
        MaxX = maxX;
        MaxY = maxY;
        MaxZ = maxZ;
    }

    public double MinX { get; }

    public double MinY { get; }

    public double MinZ { get; }

    public double MaxX { get; }

    public double MaxY { get; }

    public double MaxZ { get; }

    public double Width => MaxX - MinX;

    public double Depth => MaxY - MinY;

    public double Height => MaxZ - MinZ;

    public double CenterX => (MinX + MaxX) / 2.0;

    public double CenterY => (MinY + MaxY) / 2.0;

    public double CenterZ => (MinZ + MaxZ) / 2.0;

    public double LargestDimension => Math.Max(Width, Math.Max(Depth, Height));
}
