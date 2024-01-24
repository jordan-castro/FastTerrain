using System.Collections.Generic;
using System.Threading;
using Godot;

namespace FastTerrain;

/// <summary>
/// This is a chunk of the terrain, it is used to draw tiles to the FTPainter.
/// </summary>
public class FTChunk
{
	/// <summary>
	/// Our thread to load the chunk.
	/// </summary>
	private Thread thread = null;

	/// <summary>
	/// Is our thread finished?
	/// </summary>
	private bool isThreadFinished = false;

	public FTChunk() { }

	/// <summary>
	/// Generate the chunk.
	/// </summary>
	public void Generate(
		Vector2I chunk,
		FastNoiseLite noise,
		TileMap threadMap,
		List<int> terrainsToUse,
		int chunkSize
	)
	{
		// The tiles to be added to the thread map.
		List<Vector2I> tiles = new();

		// 1. Apply noise to area.
		for (int x = chunk.X; x < chunk.X + chunkSize; x++)
		{
			for (int y = chunk.Y; y < chunk.Y + chunkSize; y++)
			{
				// Get noise
				double noiseValue = noise.GetNoise2D(x, y);
				if (noiseValue > 0.5)
				{
					// Add tile to list
					tiles.Add(new Vector2I(x, y));
				}
			}
		}
		// 2. Set tile positions.
		threadMap.SetCellsTerrainConnect(
			0,
			new Godot.Collections.Array<Vector2I>(tiles),
			0,
			terrainsToUse[0]
		);
		// 3. Apply objects to area.

	}
}