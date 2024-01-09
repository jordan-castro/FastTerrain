using Godot;

namespace FastTerrain;

public partial class TileWithPositionOnGrid : Tile {
    // The position of the tile on the grid.
    public Vector2I PositionOnGrid { get; set; } = new Vector2I();

    // Constructor
    public TileWithPositionOnGrid(
        string name = "", 
        Vector2I atlas = default(Vector2I), 
        int alt = 0, 
        Vector2I positionOnGrid = default(Vector2I)
    ) : base(name, atlas, alt) {
        PositionOnGrid = positionOnGrid;
    }

    public static TileWithPositionOnGrid FromTile(Tile tile, Vector2I positionOnGrid) {
        return new TileWithPositionOnGrid(tile.Name, tile.Atlas, tile.Alt, positionOnGrid);
    }
}