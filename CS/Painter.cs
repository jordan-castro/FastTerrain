using Godot;
using System.Threading;
using System.Collections.Generic;

namespace FastTerrain;

public partial class Painter : Node
{
    /// <summary>
    /// The path to the data file. Usually JSON
    /// </summary>
    [Export]
    public string dataFilePath = "";
    /// <summary>
    /// The seed for the world. This is used to generate the terrain.
    /// </summary>
    [Export]
    public int worldSeed = 0;
    /// <summary>
    /// The player. This is used to load chunks around the player.
    /// And to initialize the player.
    /// </summary>
    [Export]
    public CharacterBody2D player = null;
    /// <summary>
    /// The node that contains all the behaviors.
    /// </summary>
    public Node BehaviorNodes = null;
    /// <summary>
    /// Our chunk loading thread. This runs our chunk loading logic and checking in
    /// a separate thread.
    /// </summary>
    private Thread ChunkLoaderThread = null;
    /// <summary>
    /// The data loader. This loads the data file and generates the terrain.
    /// </summary>
    public DataLoader DataLoader = null;
    /// <summary>
    /// The player position that is used by the thread.
    /// </summary>
    private Vector2I playerPositionForThread = Vector2I.Zero;
    /// <summary>
    /// The tilemap that  has everything done to it off the main thread.
    /// In order for this tilemap to be in the node tree, we have to call
    /// UpdateTileMap() on the main thread.
    /// </summary>
    private TileMap offScreen = new();

    private List<TileMap> chunkTileMaps = new();

    public override void _Ready()
    {
        base._Ready();
        GD.Print("Painter Ready");

        // Load data
        DataLoader = new DataLoader(dataFilePath, worldSeed);
        // Add chunk tile maps
        for (int i = 0; i < DataLoader.Chunks.Count; i++)
        {
            TileMap map = new();
            chunkTileMaps.Add(map);
            AddChild(map);
        }
        // Load the tile set
        offScreen.TileSet = ResourceLoader.Load<TileSet>(DataLoader.TextureImage);
        // Get the behavior nodes
        BehaviorNodes = GetParent().GetNode("Behaviors");
        GD.Print("Painter Done Loading Data");

        // Choose a random chunk to load the player
        int playerChunkIndex = DataLoader.Random.Range(0, DataLoader.Chunks.Count);
        // Load the chunk
        Chunk playerChunk = DataLoader.Chunks[playerChunkIndex];
        DataLoader.Terrain = playerChunk.Load(this, true, DataLoader.Terrain);
        // Update the shown tilemap to offscreen
        UpdateTileMap(playerChunkIndex);
        // Load player
        List<Vector2I> spawnPoints = DataLoader.Terrain.GridSystem.GetCellsByType(
            new string[] { "GrassTallMiddle", "GrassTallLeft", "GrassTallRight" },
            DataLoader.Terrain.GridSystem.BoxSafe(
                playerChunk.PositionOnGrid,
                playerChunk.Width,
                playerChunk.Height
            ).ToArray()
        );
        // While our spawn point is invalid, choose a new one
        Vector2I spawnPoint = (Vector2I)DataLoader.Random.Choose(spawnPoints);
        while (spawnPoint.Y == playerChunk.PositionOnGrid.Y)
        {
            spawnPoint = (Vector2I)DataLoader.Random.Choose(spawnPoints);
        }
        spawnPoint.Y -= 1;
        // Spawn in player
        player.GlobalPosition = MapToLocal(spawnPoint);
    }

    /// <summary>
    /// Loads the tile set into the offscreen tilemap.
    /// </summary>
    /// <param name="chunkGodot"></param>
    public void LoadTileSet(int chunkIndex)
    {
        // Get chunk from index
        Chunk chunk = DataLoader.Chunks[chunkIndex];
        Tile tile = null;
        for (int x = chunk.PositionOnGrid.X; x < chunk.PositionOnGrid.X + chunk.Width; x++)
        {
            for (int y = chunk.PositionOnGrid.Y; y < chunk.PositionOnGrid.Y + chunk.Height; y++)
            {
                // Get the tile and check if it is empty
                tile = DataLoader.Terrain.GridSystem.GetCellSafe(x, y);
                if (tile == null || tile.IsEmpty())
                {
                    // If it is empty, we can skip drawing it.
                    // This helps performance.
                    continue;
                }

                // Set the tile
                offScreen.SetCell(
                    0,
                    new Vector2I(x, y),
                    0,
                    tile.Atlas,
                    tile.Alt
                );
                chunkTileMaps[chunkIndex].SetCell(
                    0,
                    new Vector2I(x, y),
                    0,
                    tile.Atlas,
                    tile.Alt
                );
            }
        }

        // Any thing to spawn?
        // TODO: this should be handled in its own method
        // foreach (var spawner in chunk.Spawners)
        // {
        //     AddChild(spawner.Node);
        //     spawner.Node.Set("position", MapToLocal(spawner.GridPosition));
        // }
    }

    /// <summary>
    /// when a body enters a behavior area, call the behavior if 
    /// the body has the method.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="behaviorArea"></param>
    public void _on_behavior_area_body_entered(Node body, BehaviorArea behaviorArea)
    {
        if (body.HasMethod("call_behavior"))
        {
            // GD.Print("Body has behavior");
            // body.Call("call_behavior", behaviorArea, this);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        // If the player is null, do nothing.
        if (player == null)
        {
            return;
        }

        // Get the position of the player on the map.
        playerPositionForThread = LocalToMap(player.GlobalPosition);

        if (ChunkLoaderThread == null)
        {
            // Instantiate a new thread if not already instantiated.
            ChunkLoaderThread = new Thread(LoadChunks);
            ChunkLoaderThread.Start();
        }
    }

    /// <summary>
    /// Loads chunks around the player.
    /// </summary>
    private void LoadChunks()
    {
        // The players positive position on the map.
        Rect2 ppp;
        // All the chunk rects.
        Rect2[] chunkRects = new Rect2[DataLoader.Chunks.Count];
        // The chunk we are currently loading.
        Chunk chunk = null;
        for (int i = 0; i < DataLoader.Chunks.Count; i++)
        {
            // Load in all chunk rects to help performance.
            chunkRects[i] = DataLoader.Chunks[i].GetRect();
        }

        while (true)
        {
            // Create a new rect of the players positon.
            ppp = new Rect2(
                playerPositionForThread.X - DataLoader.Chunks[0].Width / 2,
                playerPositionForThread.Y - DataLoader.Chunks[0].Height / 2,
                DataLoader.Chunks[0].Width,
                DataLoader.Chunks[0].Height
            );

            // Loop throught the rects
            for (int i = 0; i < chunkRects.Length; i++)
            {
                if (ppp.Intersects(chunkRects[i]))
                {
                    // When we find one and it is not already loaded. Load that shit
                    chunk = DataLoader.Chunks[i];
                    if (!chunk.IsLoaded)
                    {
                        LoadChunk(chunk, i);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Load an individual chunk.
    /// </summary>
    /// <param name="chunk"></param>
    private void LoadChunk(Chunk chunk, int chunkIndex)
    {
        // The start of the function call
        double startTime = Time.GetUnixTimeFromSystem();

        // Load the chunk and get the current terrain.
        DataLoader.Terrain = chunk.Load(this, true, DataLoader.Terrain);
        // Update offscreen tilemap
        CallDeferred("UpdateTileMap", chunkIndex);
        // UpdateTileMap();

        // The end of the function call
        double endTime = Time.GetUnixTimeFromSystem();
        GD.Print("Loaded chunk in " + (endTime - startTime) + " seconds");
    }

    /// <summary>
    /// Returns the global position. 
    /// This is a backwards compatible because we changed Painter to Node.
    /// </summary>
    /// <param name="local"></param>
    /// <returns></returns>
    public Vector2I LocalToMap(Vector2 local)
    {
        return offScreen.LocalToMap(local);
    }

    /// <summary>
    /// Returns the tile position. 
    /// This is a backwards compatible because we changed Painter to Node.
    /// </summary>
    /// <param name="map"></param>
    /// <returns></returns>
    public Vector2 MapToLocal(Vector2I map)
    {
        return offScreen.MapToLocal(map);
    }

    private void UpdateTileMap(int chunkIndex)
    {
        chunkTileMaps[chunkIndex].GlobalPosition = MapToLocal(DataLoader.Chunks[chunkIndex].PositionOnGrid);
        LoadTileSet(chunkIndex);
    }
}