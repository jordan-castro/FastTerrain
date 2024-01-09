using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FastTerrain;

public class Chunk {
    public bool IsLoaded = false;
    public Vector2I PositionOnGrid = Vector2I.Zero;
    public int Width = 0;
    public int Height = 0;
    public List<FTSpawner> Spawners = new();
    public Painter painter = null;
    public Terrain Terrain = null;

    public Chunk(Vector2I positionOnGrid, int width, int height) {
        PositionOnGrid = positionOnGrid;
        Width = width;
        Height = height;
    }

    public Terrain Load(Painter painter, bool loadFull, Terrain terrain) {
        this.painter = painter;
        Terrain = terrain;
        if (!loadFull) {
            return null;
        }

        LoadTerrain();

        foreach (var b in this.painter.DataLoader.Behaviors) {
            LoadBehavior(b, Terrain.GridSystem);
        }
        IsLoaded = true;
        return Terrain;
    }

    public Rect2 GetRect() {
        return new Rect2(PositionOnGrid, Width, Height);
    }

    private void LoadTerrain() {
        Terrain.Build(painter.DataLoader, this);
    }

    public List<TileWithPositionOnGrid> GetTiles(GridSystem gridSystem) {
        return gridSystem.BoxSafe(PositionOnGrid, Width, Height);
    }

    public void SpawnIntoPainter(Node node, Vector2I position) {
        Spawners.Add(new FTSpawner(node, position));
    }

    public void LoadBehavior(Behavior behavior, GridSystem system) {
        List<Vector2I> tilesNeedingThisBehavior = painter.DataLoader.Terrain.GridSystem.GetCellsByType(
            new string[] { behavior.Tile },
            system.BoxSafe(PositionOnGrid, Width, Height).ToArray()
        );

        if (tilesNeedingThisBehavior.Count == 0) {
            return;
        }

        // Initiate outside to help performance
        CollisionShape2D collisionShape2D = null;
        BehaviorArea area = null;

        foreach (Vector2I tile in tilesNeedingThisBehavior) {
            collisionShape2D = new CollisionShape2D
            {
                Shape = new RectangleShape2D()
            };
            collisionShape2D.Shape.Set("extents", new Vector2(20, 25));

            // Create the area
            area = new BehaviorArea(behavior.BehaviorValue);
            area.SetCollisionLayerValue(4, true);
            area.SetCollisionLayerValue(1, false);
            area.SetCollisionMaskValue(3, true);

            area.Position = painter.MapToLocal(tile);
            area.BehaviorBodyEntered += painter._on_behavior_area_body_entered;
            area.CallDeferred("add_child", collisionShape2D);

            painter.BehaviorNodes.CallDeferred("add_child", area);
        }
    }
}