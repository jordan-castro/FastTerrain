using Godot;

namespace FastTerrain;

public partial class Tile : GodotObject
{
    private static readonly Tile _empty = new("Empty", new Vector2I(-1, -1));

    // The name of the tile. This is used to identify the tile.
    public string Name { get; set; } = "";

    // The position of the tile in the atlas.
    public Vector2I Atlas { get; set; } = new Vector2I();

    // Alternative tile id
    public int Alt { get; set; } = 0;

    // Constructor
    public Tile(string name = "", Vector2I atlas = default(Vector2I), int alt = 0)
    {
        Name = name;
        Atlas = atlas;
        Alt = alt;
    }

    // Equality Check
    public override bool Equals(object obj)
    {
        if (obj is Tile other)
        {
            return Name == other.Name;
        }
        return false;
    }

    // Hash Function
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    // Check if Tile is Empty
    public bool IsEmpty()
    {
        return Name == "Empty";
    }

    // Creates a new empty tile.
    public static Tile Empty()
    {
        return _empty;
    }
}
