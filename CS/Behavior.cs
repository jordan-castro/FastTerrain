
namespace FastTerrain;

public class Behavior
{
    // Fields for tile and behavior
    private string tile = "";
    private string behavior = "";

    // Constructor
    public Behavior(string tile, string behavior)
    {
        this.tile = tile;
        this.behavior = behavior;
    }

    // Properties
    public string Tile
    {
        get { return tile; }
        set { tile = value; }
    }

    public string BehaviorValue
    {
        get { return behavior; }
        set { behavior = value; }
    }
}
