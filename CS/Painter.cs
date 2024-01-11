using Godot;

using FastTerrain;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    private GodotThread ChunkLoaderThread = new();
    public DataLoader DataLoader = null;
    private Vector2I playerPositionForThread = Vector2I.Zero;

    public override void _Ready()
    {
        base._Ready();
        GD.Print("Painter Ready");
        // Load data
        DataLoader = new DataLoader(dataFilePath, worldSeed);

        // Set tileset
        TileSet tileSet = ResourceLoader.Load<TileSet>(DataLoader.TextureImage);
        TileSet = tileSet;

        // Initialze chunks
        foreach (var chunk in DataLoader.Chunks)
        {
            chunk.Initialize(tileSet, map_to_local(chunk.PositionOnGrid));
            AddChild(chunk);
        }
        GD.Print("Loaded chunks");

        // OnReady stuff
        BehaviorNodes = GetParent().GetNode("Behaviors");
        EnemyNodes = GetParent().GetNode("Enemies");

        GD.Print("Painter Done Loading Data");

        // Load player chunk
        Chunk playerChunk = (Chunk)DataLoader.Random.Choose(DataLoader.Chunks);
        DataLoader.Chunks.Remove(playerChunk);
        playerChunk.Load(this);
        // // GET POSITION to spawn on
        // List<Vector2I> spawnPoints = playerChunk.terrain.GridSystem.GetCellsByType(
        //     new string[] { "GrassTallLeft" },
        //     playerChunk.terrain.GridSystem.BoxSafe(
        //         new Vector2I(0, 0),
        //         playerChunk.Width,
        //         playerChunk.Height
        //     ).ToArray()
        // );
        // Vector2I spawnPoint = (Vector2I)DataLoader.Random.Choose(spawnPoints);
        // while (spawnPoint.Y == playerChunk.Height)
        // {
        //     spawnPoint = (Vector2I)DataLoader.Random.Choose(spawnPoints);
        // }
        // spawnPoint.Y -= 1;
        // Thread.Sleep(1000);
        // player.GlobalPosition = map_to_local(spawnPoint);

        ChunkLoaderThread = new GodotThread();
        ChunkLoaderThread.Start(Callable.From(LoadChunks));
        // Task.Run()
    }
    public void _on_behavior_area_body_entered(Node body, BehaviorArea behaviorArea)
    {
        if (body.HasMethod("call_behavior"))
        {
            // GD.Print("Body has behavior");
            body.Call("call_behavior", behaviorArea, this);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (player is null)
        {
            return;
        }

        if (player.IsQueuedForDeletion())
        {
            player = null;
            return;
        }

        var newPosition = local_to_map(player.GlobalPosition);
        if (newPosition != playerPositionForThread)
        {
            playerPositionForThread = newPosition;
        }
    }

    private void LoadChunks()
    {
        while (true)
        {
            Rect2 ppp = new(
                playerPositionForThread.X - DataLoader.Chunks[0].Width / 2,
                playerPositionForThread.Y - DataLoader.Chunks[0].Height / 2,
                DataLoader.Chunks[0].Width,
                DataLoader.Chunks[0].Height
            );

            foreach (var chunk in DataLoader.Chunks)
            {
                if (chunk.IsLoaded)
                {
                    continue;
                }

                if (ppp.Intersects(chunk.Rect))
                {
                    double startTime = Time.GetUnixTimeFromSystem();

                    chunk.Load(this);
                    double endTime = Time.GetUnixTimeFromSystem();

                    GD.Print("Loaded chunk in " + (endTime - startTime) + " seconds");
                }
            }
        }
    }

    private void LoadChunk(Chunk chunk)
    {
        // chunk.Load(true, this);

        // DataLoader.Terrain = chunk.Load(this, true, DataLoader.Terrain);
        // CallDeferred(nameof(LoadTileSet), new GodotChunk(chunk));

        // GD.Print("Loaded chunk in " + (endTime - startTime) + " seconds");
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
    public void _set_cell(Vector2I position, string tileName)
    {
        Tile tile = null;
        foreach (var t in DataLoader.Tiles)
        {
            if (t.Name == tileName)
            {
                tile = t;
                break;
            }
        }

        // DataLoader.Terrain.GridSystem.SetCell(position, tile);
        // tileMap.SetCell(
        //     0,
        //     position,
        //     0,
        //     tile.Atlas,
        //     tile.Alt
        // );
        // UpdateTileMap();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ChunkLoaderThread.WaitToFinish();
    }
}