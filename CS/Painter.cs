using Godot;

using FastTerrain;
using System.Threading;
using System.Collections.Generic;

public partial class Painter : TileMap
{
    [Export]
    public string dataFilePath = "";
    [Export]
    public int worldSeed = 0;
    [Export]
    private CharacterBody2D player = null;
    public Node BehaviorNodes = null;
    public Node EnemyNodes = null;
    private Thread ChunkLoaderThread = null;
    public DataLoader DataLoader = null;
    private Vector2I playerPositionForThread = Vector2I.Zero;
    private TileMap tileMap;

    public override void _Ready()
    {
        base._Ready();
        GD.Print("Painter Ready");
        // Load data
        DataLoader = new DataLoader(dataFilePath, worldSeed);
        // Set tileset
        TileSet = ResourceLoader.Load<TileSet>(DataLoader.TextureImage);
        // OnReady stuff
        BehaviorNodes = GetParent().GetNode("Behaviors");
        EnemyNodes = GetParent().GetNode("Enemies");

        GD.Print("Painter Done Loading Data");

        Chunk playerChunk = (Chunk)DataLoader.Random.Choose(DataLoader.Chunks);
        // Remove that chunk
        DataLoader.Chunks.Remove(playerChunk);
        DataLoader.Terrain = playerChunk.Load(this, true, DataLoader.Terrain);
        LoadTileSet(new ChunkGodot(playerChunk));
        // Load player
        List<Vector2I> spawnPoints = DataLoader.Terrain.GridSystem.GetCellsByType(
            new string[] { "GrassTallMiddle", "GrassTallLeft", "GrassTallRight" },
            DataLoader.Terrain.GridSystem.BoxSafe(
                playerChunk.PositionOnGrid,
                playerChunk.Width,
                playerChunk.Height
            ).ToArray()
        );
        Vector2I spawnPoint = (Vector2I)DataLoader.Random.Choose(spawnPoints);
        while (spawnPoint.Y == playerChunk.PositionOnGrid.Y) {
            spawnPoint = (Vector2I)DataLoader.Random.Choose(spawnPoints);
        }
        spawnPoint.Y -= 1;
        player.GlobalPosition = MapToLocal(spawnPoint);
    }

    public void LoadTileSet(ChunkGodot chunkGodot)
    {
        Chunk chunk = chunkGodot.chunk;
        Tile tile = null;
        for (int x = chunk.PositionOnGrid.X; x < chunk.PositionOnGrid.X + chunk.Width; x++)
        {
            for (int y = chunk.PositionOnGrid.Y; y < chunk.PositionOnGrid.Y + chunk.Height; y++)
            {
                tile = DataLoader.Terrain.GridSystem.GetCellSafe(x, y);
                if (tile == null || tile.IsEmpty())
                {
                    continue;
                }

                SetCell(
                    0,
                    new Vector2I(x, y),
                    0,
                    tile.Atlas,
                    tile.Alt
                );
            }
        }

        foreach (var spawner in chunk.Spawners)
        {
            EnemyNodes.AddChild(spawner.Node);
            spawner.Node.Set("position", MapToLocal(spawner.GridPosition));
        }
    }

    public void _on_behavior_area_body_entered(Node body, BehaviorArea behaviorArea)
    {
        if (body.HasMethod("call_behavior"))
        {
            // GD.Print("Body has behavior");
            body.Call("call_behavior", behaviorArea, this);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (player == null)
        {
            return;
        }
        if (player.IsQueuedForDeletion()) {
            player = null;
        }

        playerPositionForThread = LocalToMap(player.GlobalPosition);

        if (ChunkLoaderThread == null)
        {
            ChunkLoaderThread = new Thread(LoadChunks);
            ChunkLoaderThread.Start();
        }
    }

    private void LoadChunks()
    {
        Rect2 ppp;
        Rect2[] chunkRects = new Rect2[DataLoader.Chunks.Count];
        Chunk chunk = null;
        for (int i = 0; i < DataLoader.Chunks.Count; i++)
        {
            chunkRects[i] = DataLoader.Chunks[i].GetRect();
        }

        while (true)
        {
            ppp = new Rect2(
                playerPositionForThread.X - DataLoader.Chunks[0].Width / 2,
                playerPositionForThread.Y - DataLoader.Chunks[0].Height / 2,
                DataLoader.Chunks[0].Width,
                DataLoader.Chunks[0].Height
            );

            for (int i = 0; i < chunkRects.Length; i++)
            {
                if (ppp.Intersects(chunkRects[i]))
                {
                    chunk = DataLoader.Chunks[i];
                    if (!chunk.IsLoaded)
                    {
                        LoadChunk(chunk);
                    }
                }
            }
        }
    }

    private void LoadChunk(Chunk chunk)
    {
        double startTime = Time.GetUnixTimeFromSystem();

        DataLoader.Terrain = chunk.Load(this, true, DataLoader.Terrain);
        CallDeferred(nameof(LoadTileSet), new ChunkGodot(chunk));

        double endTime = Time.GetUnixTimeFromSystem();
        GD.Print("Loaded chunk in " + (endTime - startTime) + " seconds");
    }

    /// <summary>
    /// For backwards compatibility
    /// </summary>
    public Vector2 map_to_local(Vector2I mapPosition)
    {
        return MapToLocal(mapPosition);
    }

    /// <summary>
    /// For backwards compatibility
    /// </summary>
    public Vector2I local_to_map(Vector2 localPosition)
    {
        return LocalToMap(localPosition);
    }

    /// <summary>
    /// For backwards compatibility
    /// </summary>
    public void _set_cell(Vector2I position, string tileName) {
        Tile tile = null;
        foreach (var t in DataLoader.Tiles)
        {
            if (t.Name == tileName)
            {
                tile = t;
                break;
            }
        }

        DataLoader.Terrain.GridSystem.SetCell(position, tile);
        SetCell(
            0,
            position,
            0,
            tile.Atlas,
            tile.Alt
        );
    }
}