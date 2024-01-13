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
    public TileMap tileMap = null;
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

    public Chunk(Vector2I positionOnGrid, int width, int height)
    {
        PositionOnGrid = positionOnGrid;
        Width = width;
        Height = height;
        rect = new Rect2(PositionOnGrid, Width, Height);

        terrain = new Terrain(new Vector2I(Width, Height));
        tileMap = new TileMap();

        // Add EnemyNodes and BehaviorNodes
        AddChild(EnemyNodes);
        AddChild(BehaviorNodes);
    }

    /// <summary>
    /// Initialize the chunk with a tileset. And a global_position.
    /// </summary>
    /// <param name="tileSet"></param>
    public void Initialize(TileSet tileSet, Vector2 globalPosition, GDScript enemyScript)
    {   
        // Apply the script
        EnemyNodes.SetScript(enemyScript);

        tileMap.TileSet = tileSet;
        tileMap.Name = "ChunkTileMap";
        Name = $"Chunk {PositionOnGrid.X}x{PositionOnGrid.Y}";
        GlobalPosition = globalPosition;

        AddChild(tileMap);
    }

    public void Load(Painter painter)
    {
        this.painter = painter;
        LoadTerrain();
        
        foreach (var b in painter.DataLoader.Behaviors)
        {
            LoadBehavior(b, terrain.GridSystem);
        }
        IsLoaded = true;
        CallDeferred(nameof(DrawTerrain));
        CallDeferred(nameof(SpawnNodes));
    }

    private void LoadTerrain()
    {
        terrain.Build(painter.DataLoader, this);
    }

    public List<TileWithPositionOnGrid> GetTiles(GridSystem gridSystem)
    {
        return gridSystem.BoxSafe(new Vector2I(0, 0), Width, Height);
    }

    public void SpawnIntoPainter(string node, Vector2I position)
    {
        Spawners.Add(new FTSpawner(node, position));
    }

    public void LoadBehavior(Behavior behavior, GridSystem system)
    {
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

        // CallDeferred("add_child", tileMap);
        // CallDeferred(nameof(SpawnNode));
    }

    private void SpawnNodes()
    {
        foreach (var spawner in Spawners)
        {
            Node2D node = (Node2D)ResourceLoader.Load<PackedScene>(spawner.Node).Instantiate();
            EnemyNodes.AddChild(node);
            node.GlobalPosition = painter.map_to_local(PositionOnGrid) + tileMap.MapToLocal(spawner.GridPosition);
        }
    }
}