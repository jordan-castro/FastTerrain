using FTObjects;
using Godot;
using System;
using System.Collections.Generic;

namespace FastTerrain;

public class FTObject
{
	/// <summary>
	/// The tile that can have a object spawned on it.
	/// </summary>
	protected List<Vector2I> spawnOn = new();

	/// <summary>
	/// A key value pair for finding tiles based on a description.
	/// i.e. "base" => Vector2I(base atlas coords)
	/// </summary>
	protected Godot.Collections.Dictionary specificTiles = new();

	/// <summary>
	/// Our rarity.
	/// </summary>
	protected double rarity = 0.0;

	/// <summary>
	/// The terrains that this object uses.
	/// </summary>
	protected Dictionary<string, int> terrains = new();

	/// <summary>
	/// Build a FTObject. This method must be implemented from child classes.
	/// </summary>
	/// <param name="tiles"></param>
	/// <param name="threadMap"></param>
	/// <param name="chunkPosition"></param>
	/// <param name="worldSize"></param>
	/// <param name="chunkSize"></param>
	/// <exception cref="NotImplementedException"></exception>
	public virtual void Build(
		Godot.Collections.Array<Vector2I> tiles,
		TileMap threadMap,
		Vector2I chunkPosition,
		Vector2I worldSize,
		int chunkSize,
		RandomNumberGenerator random
	)
	{
		throw new NotImplementedException();
	}

	public FTObject(List<Vector2I> spawnOn, Godot.Collections.Dictionary specificTiles, double rarity, Dictionary<string, int> terrains)
	{
		this.spawnOn = spawnOn;
		this.specificTiles = specificTiles;
		this.rarity = rarity;
		this.terrains = terrains;
	}

	/// <summary>
	/// Create a FTObject from a json object.
	/// </summary>
	/// <param name="json"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public static FTObject FromJson(Godot.Collections.Dictionary json)
	{
		// Get data
		List<Vector2I> spawnOn = new();
		foreach (var tile in json["spawnOn"].As<Godot.Collections.Array>())
		{
			spawnOn.Add(Utils.StringToVector2I((string)tile));
		}
		Godot.Collections.Dictionary specificTiles = new();
		if (json.ContainsKey("specificTiles"))
		{
			foreach (var key in json["specificTiles"].As<Godot.Collections.Dictionary>().Keys)
			{
				// We used to Parse the string to a Vector2I, but now we just store the string.
				// Parsing the string is done individually in each object.
				specificTiles.Add(
					(string)key,
					json["specificTiles"].As<Godot.Collections.Dictionary>()[key]
				);
			}
		}

		Dictionary<string, int> terrains = new();
		if (json.ContainsKey("terrains"))
		{
			foreach (var key in json["terrains"].As<Godot.Collections.Dictionary>().Keys)
			{
				terrains.Add(
					(string)key,
					(int)json["terrains"].As<Godot.Collections.Dictionary>()[key]
				);
			}
		}
		double rarity = (double)json["rarity"];

		switch ((string)json["name"])
		{
			case "FTTree":
				return new FTTree(spawnOn, specificTiles, rarity, terrains);
			case "Default":
				return new Default(spawnOn, specificTiles, rarity, terrains);
			case null:
				throw new NotImplementedException();
		}

		return null;
	}

	/// <summary>
	/// Helper function to convert the tiles into a Grid like object.
	/// Using ChunSize as the width and height. For every tile in the tiles array, it will
	/// check if any tile is missing, and for missing tiles will fill them with a Vector2I(-1, -1).
	/// </summary>
	/// <param name="tiles"></param>
	/// <param name="chunkSize"></param>
	protected List<List<Vector2I>> TilesToGrid(Godot.Collections.Array<Vector2I> tiles, Vector2I chunkPos, int chunkSize)
	{
		// Create a grid
		List<List<Vector2I>> grid = new();
		// Fill the grid
		for (int x = chunkPos.X; x < chunkPos.X + chunkSize; x++)
		{
			// A row in the grid
			List<Vector2I> gridRow = new();
			for (int y = chunkPos.Y; y < chunkPos.Y + chunkSize; y++)
			{
				// If the tile already exists, add it to the grid
				if (tiles.Contains(new Vector2I(x, y)))
				{
					gridRow.Add(new Vector2I(x, y));
				}
				else
				{
					// Otherwise fill it with empty tiles.
					gridRow.Add(new Vector2I(-1, -1));
				}
			}
			grid.Add(gridRow);
		}
		return grid;
	}

	/// <summary>
	/// Helper function, checks if a space is free to build on.
	/// This is more important in the case of multi-tile objects.
	/// </summary>
	protected bool HasSpaceToBuild(TileMap threadMap, int x, int y, int width, int height)
	{
		// Refrence to our empty tile
		Vector2I emptyTile = new Vector2I(-1, -1);
		// Check if the space is free
		for (int i = x; i < x + width; i++)
		{
			for (int j = y; j < y + height; j++)
			{
				if (threadMap.GetCellAtlasCoords(0, new Vector2I(i, j)) != emptyTile)
				{
					return false;
				}
			}
		}
		return true;
	}

	/// <summary>
	/// Checks that a FTObject is within the chunk bounds.
	/// This is more important in the case of multi-tile objects.
	/// </summary>
	protected bool IsWithinChunk(Vector2I tilePos, Vector2I objectBounds, int chunkSize, Vector2I chunkPosition)
	{
		if (tilePos.X + objectBounds.X > chunkSize + chunkPosition.X || tilePos.X < chunkPosition.X)
		{
			return false;
		}

		if (tilePos.Y + objectBounds.Y > chunkSize + chunkPosition.Y || tilePos.Y < chunkPosition.Y)
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Helper function converts a Tile coords to be within the current chunk.
	/// </summary>
	protected Vector2I TileToChunk(Vector2I tile, Vector2I chunkPosition)
	{
		return new Vector2I(tile.X - chunkPosition.X, tile.Y - chunkPosition.Y);
	}

	/// <summary>
	/// Helper function checks if a rarity is valid
	/// </summary>
	protected bool IsRarityValid(RandomNumberGenerator random)
	{
		return random.Randf() <= rarity;
	}
}