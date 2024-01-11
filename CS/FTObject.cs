using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json.Linq;

namespace FastTerrain;

public class FTObject
{
    protected GridSystem GridSystem = null;
    protected Dictionary<string, object> Args = null;
    protected Random Random = null;
    protected Chunk Chunk = null;
    protected int Direction = -1;
    protected List<Tile> Tiles = new();

    public GridSystem Build(
        GridSystem gridSystem,
        Dictionary<string, object> args,
        Random random,
        Chunk chunk,
        List<Tile> tiles
    )
    {
        GridSystem = gridSystem;
        Args = args;
        Random = random;
        Chunk = chunk;
        Tiles = tiles;

        _Build();

        return GridSystem;
    }

    /// <summary>
    /// This method runs after initialization.
    /// </summary>
    protected virtual void _Build()
    {
    }

    protected Tile GetTile(string name) {
        foreach (var tile in Tiles) {
            if (tile.Name == name) {
                return tile;
            }
        }
        return null;
    }

    protected List<TileWithPositionOnGrid> GetTilesCanSpawnOn()
    {
        List<TileWithPositionOnGrid> tiles = Chunk.GetTiles(GridSystem);
        List<TileWithPositionOnGrid> tilesCanSpawnOn = new();

        foreach (var tile in tiles)
        {
            foreach (var tileName in (JArray)Args["spawnOn"]) {
                if (tile.Name == (string)tileName) {
                    tilesCanSpawnOn.Add(tile);
                }
            }
        }

        return tilesCanSpawnOn;
    }

    protected bool CanBuildHere(Vector2I position, int width, int height)
    {
        // Check we are within the chunk
        if (position.X < 0 ||
        position.Y < 0 ||
        position.X + width > Chunk.Width ||
        position.Y + height > Chunk.Height)
        {
            return false;
        }

        // All tiles within space must be empty, unless otherwise specified.
        List<TileWithPositionOnGrid> box = GridSystem.BoxSafe(position, width, height);
        foreach (var tile in box)
        {
            if (tile == null || !tile.IsEmpty())
            {
                return false;
            }
        }

        return true;
    }

    protected bool IsValidTile(Vector2I position)
    {
        if (position.Y <= 0 ||
        position.Y >= Chunk.Height)
        {
            return false;
        }

        Tile tileToPlace = GridSystem.GetCellSafe(position);
        if (tileToPlace == null) {
            return false;
        }
        if (!tileToPlace.IsEmpty()) {
            return false;
        }

        return true;
    }
}