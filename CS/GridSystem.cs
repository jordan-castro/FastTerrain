using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FastTerrain;

public class GridSystem {
    // make a 2 dimensional array using System of Tile
    private Tile[,] Grid;

    public GridSystem(Vector2I size, Tile defaultTile) {
        Grid = new Tile[size.X, size.Y];
        for (int x = 0; x < size.X; x++) {
            for (int y = 0; y < size.Y; y++) {
                Grid[x, y] = defaultTile;
            }
        }
    }

    public Tile GetCell(int x, int y) {
        return Grid[x, y];
    }

    public Tile GetCell(Vector2I pos) {
        return Grid[pos.X, pos.Y];
    }

    public Tile GetCellSafe(int x, int y) {
        if (x < 0 || x >= Grid.GetLength(0) || y < 0 || y >= Grid.GetLength(1)) {
            return null;
        }
        return Grid[x, y];
    }

    public Tile GetCellSafe(Vector2I pos) {
        if (pos.X < 0 || pos.X >= Grid.GetLength(0) || pos.Y < 0 || pos.Y >= Grid.GetLength(1)) {
            return null;
        }
        return Grid[pos.X, pos.Y];
    }

    public void SetCell(int x, int y, Tile tile) {
        Grid[x, y] = tile;
    }

    public void SetCell(Vector2I pos, Tile tile) {
        Grid[pos.X, pos.Y] = tile;
    }

    public void SetCellSafe(int x, int y, Tile tile) {
        if (x < 0 || x >= Grid.GetLength(0) || y < 0 || y >= Grid.GetLength(1)) {
            return;
        }
        Grid[x, y] = tile;
    }

    public void SetCellSafe(Vector2I pos, Tile tile) {
        if (pos.X < 0 || pos.X >= Grid.GetLength(0) || pos.Y < 0 || pos.Y >= Grid.GetLength(1)) {
            return;
        }
        Grid[pos.X, pos.Y] = tile;
    }

    public DirectionMap<Vector2I> GetNeighbours(int x, int y) {
        return new DirectionMap<Vector2I>(
            new Vector2I(x, y - 1),
            new Vector2I(x + 1, y),
            new Vector2I(x, y + 1),
            new Vector2I(x - 1, y)
        );
    }

    // Returns all cell positions of said type.
    public List<Vector2I> GetCellsByType(string[] type, TileWithPositionOnGrid[] gridBox) {
        List<Vector2I> cells = new();

        foreach (var tile in gridBox) {
            if (type.Contains(tile.Name)) {
                cells.Add(tile.PositionOnGrid);
            }
        }

        return cells;
    }

    // Get a box of tiles from the grid. 
    // If the box is out of bounds, it will throw an error
    public TileWithPositionOnGrid[] Box(Vector2I position, int width, int height) {
        TileWithPositionOnGrid[] tiles = new TileWithPositionOnGrid[width * height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tiles[x + y * width] = TileWithPositionOnGrid.FromTile(
                    Grid[position.X + x, position.Y + y], 
                    new Vector2I(position.X + x, position.Y + y)
                );
            }
        }

        return tiles;
    }

    // Get a box of tiles from the grid.
    // If the box is out of bounds it will return null for that cell.
    public List<TileWithPositionOnGrid> BoxSafe(Vector2I position, int width, int height) {
        List<TileWithPositionOnGrid> tiles = new();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Tile tile = GetCellSafe(position.X + x, position.Y + y);
                if (tile == null) {
                    tiles.Add(null);
                } else {
                    tiles.Add(
                        TileWithPositionOnGrid.FromTile(
                            tile, 
                            new Vector2I(position.X + x, position.Y + y)
                        )
                    );
                }
            }
        }

        return tiles;
    }
}