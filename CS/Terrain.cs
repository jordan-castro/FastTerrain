using System.Collections.Generic;
using Godot;

namespace FastTerrain;

public class Terrain
{
    public Vector2I size = Vector2I.Zero;

    public GridSystem GridSystem = null;

    private Chunk currentChunk = null;

    private GridSystem noiseSytem = null;

    public Terrain(Vector2I size)
    {
        this.size = size;
        GridSystem = new GridSystem(size, Tile.Empty());
        noiseSytem = new GridSystem(size, Tile.Empty());
    }

    public void Build(
        DataLoader dataLoader,
        Chunk chunk
    )
    {
        this.currentChunk = chunk;
        GenerateNoise(dataLoader.NoiseTiles, dataLoader.Random);
        AddBorders(dataLoader.AutoTiler, dataLoader.Tiles, noiseSytem);
        if (dataLoader.ChunkObjects.Count > 0)
        {
            AddObjects(dataLoader.ChunkObjects);
        }
        AddBorders(dataLoader.AutoTiler, dataLoader.Tiles, GridSystem);
    }

    private void GenerateNoise(List<Tile> noiseTiles, Random random)
    {
        // Noise stuff
        int scale = 1;
        FastNoiseLite noise = new()
        {
            Seed = random.Seed,
            NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex,
            FractalOctaves = noiseTiles.Count
        };

        // Generate noise
        for (int x = currentChunk.PositionOnGrid.X; x < currentChunk.PositionOnGrid.X + currentChunk.Width; x++)
        {
            // Make sure noise does not go out of bounds
            if (x > size.X - 1)
            {
                break;
            }
            if (x < 0)
            {
                continue;
            }
            for (int y = currentChunk.PositionOnGrid.Y; y < currentChunk.PositionOnGrid.Y + currentChunk.Height; y++)
            {
                // Make sure the noise does not go out of bounds
                if (y > size.Y - 1)
                {
                    break;
                }
                if (y < 0)
                {
                    continue;
                }

                // Stop noise from filling already non-empty tiles
                if (!GridSystem.GetCell(x, y).IsEmpty())
                {
                    continue;
                }

                // Now noise
                float noiseValue = noise.GetNoise2D(x / scale, y / scale);
                Tile tile = noiseTiles[(int)Mathf.Floor(Mathf.Abs(noiseValue) * noiseTiles.Count)];
                GridSystem.SetCell(x, y, tile);
                noiseSytem.SetCell(x, y, tile);
            }
        }
    }

    private void AddBorders(AutoTiler autoTiler, List<Tile> terrainTiles, GridSystem system)
    {
        List<TileWithPositionOnGrid> placeHolderTiles = new();
        Tile tile = null;
        string nameOfTileToUse = null;

        for (int x = currentChunk.PositionOnGrid.X; x < currentChunk.PositionOnGrid.X + currentChunk.Width; x++)
        {
            for (int y = currentChunk.PositionOnGrid.Y; y < currentChunk.PositionOnGrid.Y + currentChunk.Height; y++)
            {
                if (x < 0 || x > size.X - 1 || y < 0 || y > size.Y - 1)
                {
                    continue;
                }

                tile = system.GetCell(x, y);
                if (tile.IsEmpty())
                {
                    continue;
                }

                nameOfTileToUse = autoTiler.DecideTile(
                    tile,
                    system.GetNeighbours(x, y),
                    system
                );

                foreach (var t in terrainTiles)
                {
                    if (t.Name == nameOfTileToUse)
                    {
                        tile = t;
                        break;
                    }
                }

                placeHolderTiles.Add(
                    TileWithPositionOnGrid.FromTile(
                        tile,
                        new Vector2I(x, y)
                    )
                );
            }
        }

        foreach (TileWithPositionOnGrid t in placeHolderTiles)
        {
            GridSystem.SetCell(
                t.PositionOnGrid.X,
                t.PositionOnGrid.Y,
                t
            );
        }
    }

    private void AddObjects(List<FTObjectBuilder> objects) {
        GridSystem system;
        foreach (var obj in objects) {
           system = obj.Build(GridSystem, currentChunk);
           if (system != null) {
               GridSystem = system;
           }
        }
    }
}