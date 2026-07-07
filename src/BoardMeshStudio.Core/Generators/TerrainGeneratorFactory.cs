namespace BoardMeshStudio.Core.Generators;

public static class TerrainGeneratorFactory
{
    public static ITerrainGenerator Create(TerrainGeneratorType type)
    {
        return type switch
        {
            TerrainGeneratorType.Hill => new HillGenerator(),
            TerrainGeneratorType.Crater => new CraterGenerator(),
            TerrainGeneratorType.Rock => new RockGenerator(),
            TerrainGeneratorType.Wall => new WallGenerator(),
            TerrainGeneratorType.BlockBuilding => new BlockBuildingGenerator(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported terrain generator type.")
        };
    }
}
