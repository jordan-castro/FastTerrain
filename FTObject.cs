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
	protected Godot.Collections.Dictionary options = new();

	/// <summary>
	/// Our rarity.
	/// </summary>
	protected double rarity = 0.0;

	/// <summary>
	/// The terrains that this object uses.
	/// </summary>
	protected Dictionary<string, int> terrains = new();

	/// <summary>
	/// A list of tiles that have already been populated.
	/// </summary>
	protected List<Vector2I> populatedTiles = new();

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
		this.options = specificTiles;
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
		if (json.ContainsKey("options"))
		{
			foreach (var key in json["options"].As<Godot.Collections.Dictionary>().Keys)
			{
				// We used to Parse the string to a Vector2I, but now we just store the string.
				// Parsing the string is done individually in each object.
				specificTiles.Add(
					(string)key,
					json["options"].As<Godot.Collections.Dictionary>()[key]
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

        return (string)json["name"] switch
        {
            "FTTree" => new FTTree(spawnOn, specificTiles, rarity, terrains),
            "Default" => new Default(spawnOn, specificTiles, rarity, terrains),
            "Grass" => new Grass(spawnOn, specificTiles, rarity, terrains),
            "MushroomTree" => new MushroomTree(spawnOn, specificTiles, rarity, terrains),
            null => throw new NotImplementedException(),
            _ => null,
        };
    }

	/// <summary>
	/// Helper function, checks if a space is free to build on.
	/// This is more important in the case of multi-tile objects.
	/// </summary>
	protected bool HasSpaceToBuild(TileMap threadMap, int x, int y, int width, int height)
	{
		// Check if the space is free
		for (int i = x; i < x + width; i++)
		{
			for (int j = y; j < y + height; j++)
			{
				// Check if already populated by this object
				if (populatedTiles.Contains(new Vector2I(i, y-j)))
				{
					return false;
				}

				// Check if populated on TileMap.
				if (threadMap.GetCellAtlasCoords(0, new Vector2I(i, y-j)) != Utils.EmptyTile)
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
	protected static bool IsWithinChunk(Vector2I tilePos, Vector2I objectBounds, int chunkSize, Vector2I chunkPosition)
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
	/// Helper function checks if a rarity is valid
	/// </summary>
	protected bool IsRarityValid(RandomNumberGenerator random)
	{
		return random.Randf() <= rarity;
	}

	/// <summary>
	/// Helper function, checks if the tile is Valid, i.e. is within spawnOn,
	/// and the rarity checks out.
	/// </summary>
	protected bool IsValidTile(Vector2I tile, TileMap threadMap, RandomNumberGenerator random)
	{
		// Check if the tile is valid
		if (!spawnOn.Contains(threadMap.GetCellAtlasCoords(0, tile)))
		{
			return false;
		}

		// Check if the rarity is valid
		if (!IsRarityValid(random))
		{
			return false;
		}

		return true;
	}
}