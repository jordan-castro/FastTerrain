using System.Collections.Generic;
using Godot;

namespace FastTerrain;

public partial class Chunk : Node2D
{
    public bool IsLoaded = false;
    public Vector2I PositionOnGrid = Vector2I.Zero;
    public int Width = 0;
    public int Height = 0;
    public List<FTSpawner> Spawners = new();
    private Rect2 rect = new();
    public Rect2 Rect => rect;
    /// <summary>
    /// Be very careful when using this OUTSIDE of Chunk.
    /// </summary>
    public Terrain terrain = null;
    /// <summary>
    /// Our Painter widget.
    /// </summary>
    private Painter painter = null;
    /// <summary>
    /// The tilemap for this chunk.
    /// </summary>
    private TileMap tileMap = new();
    /// <summary>
    /// Enemies node
    /// </summary>
    public Node EnemyNodes = new()
    {
        Name = "Enemies"
    };
    /// <summary>
    /// Behaviors node
    /// </summary>
    public Node BehaviorNodes = new()
    {
        Name = "Behaviors"
    };
    /// <summary>
    /// Our on screen tile map.
    /// </summary>
    public TileMap OnScreenTileMap = new();
    /// <summary>
    /// A Thread to load this chunk.
    /// </summary>
    private GodotThread loadThread = new();

    /// <summary>
    /// A chunk is a section of the world.
    /// </summary>
    /// <param name="positionOnGrid"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Chunk(Vector2I positionOnGrid, int width, int height)
    {
        PositionOnGrid = positionOnGrid;
        Width = width;
        Height = height;
        // Set the rect
        rect = new Rect2(PositionOnGrid, Width, Height);
        // Set the terrain
        terrain = new Terrain(new Vector2I(Width, Height));

        // Set the script for our EnemyNodes
        EnemyNodes.SetScript(
            GD.Load<GDScript>("res://scripts/utils/Enemies.gd")
        );

        // Add EnemyNodes and BehaviorNodes
        AddChild(EnemyNodes);
        AddChild(BehaviorNodes);
        AddChild(OnScreenTileMap);
    }

    /// <summary>
    /// Initialize the chunk with a tileset. And a global_position.
    /// </summary>
    /// <param name="tileSet"></param>
    public void Initialize(TileSet tileSet, Vector2 globalPosition, Painter painter)
    {   
        // Set our painer
        this.painter = painter;
        // Set the tileset
        tileMap.TileSet = tileSet;
        OnScreenTileMap.TileSet = tileSet;
        OnScreenTileMap.Name = "ChunkTileMap";
        // Set a name for our chunk
        Name = $"Chunk {PositionOnGrid.X}x{PositionOnGrid.Y}";
        // Set the global position of the chunk
        GlobalPosition = globalPosition;
    }

    /// <summary>
    /// Load the chunk.
    /// </summary>
    /// <returns>True</returns>
    public bool ForeGroundLoad() {
        return this.Load();
    }

    /// <summary>
    /// Our load function.
    /// </summary>
    /// <returns>
    /// True if the load was successful.
    /// </returns>
    private bool Load()
    {
        // Build the terrain
        terrain.Build(painter.DataLoader, this);
        
        // Set behavior nodes
        foreach (var b in painter.DataLoader.Behaviors)
        {
            LoadBehavior(b, terrain.GridSystem);
        }
        IsLoaded = true;
        // Draw the terrain to the off screen tilemap
        DrawTerrain();
        // Finsih the load
        // This is more important during threaded loads.
        CallDeferred(nameof(FinishLoad));

        return true;
    }
    
    /// <summary>
    /// Load the chunk IN THE BACKGROUND.
    /// </summary>
    public void BackgroundLoad() {
        if (IsLoaded) {
            return;
        }

        // If thread is already running, return
        if (loadThread.IsAlive()) {
            return;
        }

        loadThread.Start(Callable.From(Load));
    }

    /// <summary>
    /// Finish the load of the chunk.
    /// </summary>
    private void FinishLoad()
    {
        // We only check when calling from a sub thread
        if (loadThread.IsAlive())
        {
            // Await the thread
            var res = loadThread.WaitToFinish();
        }
        // Update the tilemap INSTANTLY
        OnScreenTileMap.Set("layer_0/tile_data", tileMap.Get("layer_0/tile_data"));
        // Spawn nodes
        SpawnNodes();
    }

    /// <summary>
    /// Get the tiles of this chunk.
    /// </summary>
    /// <param name="gridSystem"></param>
    public List<TileWithPositionOnGrid> GetTiles(GridSystem gridSystem)
    {
        return gridSystem.BoxSafe(new Vector2I(0, 0), Width, Height);
    }

    /// <summary>
    /// Helper function which adds a node to be spawned into the chunk (Ak)
    /// </summary>
    /// <param name="node"></param>
    /// <param name="position"></param>
    public void SpawnIntoPainter(string node, Vector2I position)
    {
        Spawners.Add(new FTSpawner(node, position));
    }

    /// <summary>
    /// Load a behavior into the chunk.
    /// </summary>
    /// <param name="behavior"></param>
    /// <param name="system"></param>
    public void LoadBehavior(Behavior behavior, GridSystem system)
    {
        // Check which tiles need this behavior
        List<Vector2I> tilesNeedingThisBehavior = terrain.GridSystem.GetCellsByType(
            new string[] { behavior.Tile },
            system.BoxSafe(new Vector2I(0, 0), Width, Height).ToArray()
        );

        if (tilesNeedingThisBehavior.Count == 0)
        {
            return;
        }

        // Initiate outside to help performance
        CollisionShape2D collisionShape2D = null;
        BehaviorArea area = null;

        foreach (Vector2I tile in tilesNeedingThisBehavior)
        {
            collisionShape2D = new CollisionShape2D
            {
                Shape = new RectangleShape2D()
            };
            collisionShape2D.Shape.Set("extents", new Vector2(8, 10));

            // Create the area
            area = new BehaviorArea(behavior.BehaviorValue);
            area.SetCollisionLayerValue(4, true);
            area.SetCollisionLayerValue(1, false);
            area.SetCollisionMaskValue(3, true);

            area.Position = painter.map_to_local(PositionOnGrid) + tileMap.MapToLocal(tile);
            area.BehaviorBodyEntered += painter._on_behavior_area_body_entered;
            area.CallDeferred("add_child", collisionShape2D);

            // Add the area to the behavior nodes
            BehaviorNodes.CallDeferred("add_child", area);
        }
    }

    /// <summary>
    /// Draw the terrain to the tilemap.
    /// </summary>
    private void DrawTerrain()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                // No need to place empty tiles
                Tile tile = terrain.GridSystem.GetCellSafe(x, y);
                if (tile == null || tile.IsEmpty())
                {
                    continue;
                }
                tileMap.SetCell(
                    0,
                    new Vector2I(x, y),
                    0,
                    tile.Atlas,
                    tile.Alt
                );
            }
        }
    }

    /// <summary>
    /// Spawn nodes into the chunk.
    /// </summary>
    private void SpawnNodes()
    {
        foreach (var spawner in Spawners)
        {
            Node2D node = (Node2D)ResourceLoader.Load<PackedScene>(spawner.Node).Instantiate();
            EnemyNodes.AddChild(node);
            node.GlobalPosition = painter.map_to_local(PositionOnGrid) + tileMap.MapToLocal(spawner.GridPosition);
        }
    }

    /// <summary>
    /// Unload the chunk from memory
    /// </summary>
    public void Unload() {
        // Don't bother if we're not loaded
        if (!IsLoaded) {
            return;
        }

        // Clear all tiles
        OnScreenTileMap.Clear();
        // Clear nodes
        ClearChildrenFromNode(EnemyNodes);
        ClearChildrenFromNode(BehaviorNodes);
    }

    /// <summary>
    /// Clear all children from a node.
    /// </summary>
    /// <param name="node"></param>
    private void ClearChildrenFromNode(Node node) {
        foreach (Node child in node.GetChildren()) {
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
}