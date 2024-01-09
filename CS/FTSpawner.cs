using Godot;
using System;

namespace FastTerrain;

public class FTSpawner
{
    // The node object
    private Node node = null;

    // The position of the node on the grid
    private Vector2I gridPosition = Vector2I.Zero;

    // Constructor
    public FTSpawner(Node node, Vector2I gridPosition)
    {
        this.node = node;
        this.gridPosition = gridPosition;
    }

    // Properties to access private fields
    public Node Node
    {
        get { return node; }
        set { node = value; }
    }

    public Vector2I GridPosition
    {
        get { return gridPosition; }
        set { gridPosition = value; }
    }
}
