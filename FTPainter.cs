using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using Godot;
using Newtonsoft.Json.Linq;

namespace FastTerrain;

public partial class FTPainter : TileMap
{
	/// <summary>
	/// The player character.
	/// </summary>
	private CharacterBody2D player = null;

	/// <summary>
	/// The tilemap that will have all the tiles painted too before being sent to the main scene.
	/// This is used to prevent lagging on the main thread.
	/// </summary>
	private TileMap threadMap = new();

	/// <summary>
	/// Chunks that have already been loaded.
	/// <summary>
	private readonly List<Vector2I> loadedChunks = new();

	/// <summary>
	/// Keeping track of the current frame
	/// </summary>
	private int frame = 0;

	/// <summary>
	/// Our chunk loader thread.
	/// </summary>
	private Thread loadChunksThread = null;

	/// <summary>
	/// A id to Thread map.
	/// </summary>
	private int[] noiseValues = new int[] { 0, 1, 1, 1 };

	/// <summary>
	/// A random number generator.
	/// </summary>
	private RandomNumberGenerator random = new();

	/// <summary>
	/// Our Objects.
	/// </summary>
	private List<FTObject> objects = new();
	
	/// <summary>
	/// Our behaviors.
	/// </summary>
	private List<FTBehavior> behaviors = new();

	/// <summary>
	/// The max size of the threadMap. This is for performance reasons.
	/// </summary>
	private Vector2I threadMapMaxSize = new(100, 100);

	/// <summary>
	/// This converts into terrainsToUse. Please do a comma delimited list of terrain ids.\
	/// All terrains are within the same terrainset.
	/// The first terrain is used to paint the base (grass, dirt, snow, sand, etc.).
	/// </summary>
	[Export]
	public Godot.Collections.Array<int> terrainsToUse = new();

	/// <summary>
	/// The chunk size
	/// </summary>
	[Export]
	public int ChunkSize = 16;

	/// <summary>
	/// The world seed.
	/// </summary>
	[Export]
	public int WorldSeed = 0;

	/// <summary>
	/// The world size.
	/// </summary>
	[Export]
	public Vector2I WorldSize = new(100, 100);

	/// <summary>
	/// The FastNoiseLite noise object.
	/// </summary>
	[Export]
	public FastNoiseLite Noise = new();

	/// <summary>
	/// The JSON file.
	/// </summary>
	[Export]
	public string DataFilePath = "";

	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// </summary>
	public override void _Ready()
	{
		// Get the player
		player = GetChild<CharacterBody2D>(0);

		// Initialize the thread map tileset
		threadMap.TileSet = TileSet;

		// Set the noise seed
		Noise.Seed = WorldSeed;
		random.Seed = (ulong)WorldSeed;

		// Set the behaviors and objects from JSON.
        FileAccess fileAccess = FileAccess.Open(DataFilePath, FileAccess.ModeFlags.Read);
		Godot.Collections.Dictionary data = (Godot.Collections.Dictionary)Json.ParseString(fileAccess.GetAsText());
		fileAccess.Close();

		// Objects first
		foreach (Godot.Collections.Dictionary ftObject in data["objects"].As<Godot.Collections.Dictionary>()["chunk"].As<Godot.Collections.Array>()) {
			objects.Add(
				FTObject.FromJson(
					ftObject
				)
			);
		}

		// Behaviors
		foreach (Godot.Collections.Dictionary behavior in data["behaviors"].As<Godot.Collections.Array>()) {
			behaviors.Add(
				FTBehavior.FromJson(
					behavior
				)
			);
		}

		// TODO: spawn player in random location
		var playerPos = new Vector2(
			random.RandfRange(0, WorldSize.X),
			random.RandfRange(0, WorldSize.Y)
		);
		player.GlobalPosition = playerPos;
	}

	/// <summary>
	/// Called every frame. 'delta' is the elapsed time since the previous frame.
	/// </summary>
	public override void _Process(double delta)
	{
		base._Process(delta);

		frame++;
		if (frame % 15 != 0)
		{
			return;
		}

		if (loadChunksThread?.IsAlive ?? false)
		{
			return;
		}


		// 1. Check player position.
		Vector2I chunk = GetChunk(LocalToMap(player.Position));
		// 2. load in chunk and define.

		// Generate the 4 surround chunks as well.
		loadChunksThread = new(() => LoadChunks(chunk));
		loadChunksThread.Start();
	}

	/// <summary>
	/// Load in the chunks.
	/// </summary>
	private void LoadChunks(Vector2I chunk)
	{
		bool update = false;
		// N, NE, E, SE, S, SW, W, NW
		for (int x = -2; x < 2; x++)
		{
			for (int y = -2; y < 2; y++)
			{
				Vector2I newChunk = new Vector2I(chunk.X + x, chunk.Y + y);
				// Check chunk has not been loaded.
				if (loadedChunks.Contains(newChunk))
				{
					continue;
				}

				int chunkX = newChunk.X * ChunkSize;
				int chunkY = newChunk.Y * ChunkSize;

				if (chunkX < 0 || chunkX > WorldSize.X || chunkY < 0 || chunkY > WorldSize.Y)
				{
					continue;
				}

				GenerateChunk(newChunk);

				// Add the chunk to the loaded chunks
				loadedChunks.Add(newChunk);
				update = true;
			}
		}

		if (update)
		{
			// Update the main thread.
			CallDeferred("UpdateMainThread");
		}
	}

	/// <summary>
	/// Logic to generate a chunk.
	/// </summary>
	/// <param name="chunk">A Vector2I for representating the position of the chunk.</param>
	/// <param name="threadId">The id of the thread.</param>
	private void GenerateChunk(Vector2I chunk)
	{
		// The tiles to be added to the thread map.
		Godot.Collections.Array<Vector2I> tiles = new();
		Godot.Collections.Array<Vector2I> emptyTiles = new();

		// Update the chunk to be the actual position.
		chunk.X *= ChunkSize;
		chunk.Y *= ChunkSize;

		// 1. Apply noise to area.
		for (int x = chunk.X; x < chunk.X + ChunkSize; x++)
		{
			for (int y = chunk.Y; y < chunk.Y + ChunkSize; y++)
			{
				// Get noise
				double noiseValue = Noise.GetNoise2D(x, y);
				int noiseIndex = (int)(noiseValue * noiseValues.Length);
				if (noiseIndex < 0)
				{
					noiseIndex = noiseValues.Length - 1;
				}
				int tile = noiseValues[noiseIndex];
				if (tile == 0)
				{
					// Empty tile for objects.
					emptyTiles.Add(new Vector2I(x, y));
					continue;
				}

				tiles.Add(new Vector2I(x, y));
			}
		}

		// 2. Set tile positions.
		threadMap.SetCellsTerrainConnect(
			0,
			tiles,
			0,
			terrainsToUse[0]
		);

		tiles.AddRange(emptyTiles);

		// Apply objects
		foreach (var obj in objects) {
			if (obj == null) {
				continue;
			}
			obj.Build(tiles, threadMap, chunk, WorldSize, ChunkSize, random);
		}
		// // Behaviors go after objects
		// foreach (var behavior in behaviors) {
		// 	behavior.Apply(threadMap, tiles);
		// }
	}

	/// <summary>
	/// This handles setting our thread map to the main map.
	/// </summary>
	private void UpdateMainThread()
	{
		loadChunksThread = null;
		// Set our main thread man to the threadMap.
		Set("layer_0/tile_data", threadMap.Get("layer_0/tile_data"));
	}

	/// <summary>
	/// Figures out what chunk to load based on the player's position.
	/// </summary>
	/// <returns>Vecto2I</returns>
	/// <param name="playerPosition">Player position.</param>
	private Vector2I GetChunk(Vector2 playerPosition)
	{
		return new Vector2I(
			(int)(playerPosition.X / ChunkSize),
			(int)(playerPosition.Y / ChunkSize)
		);
	}
}