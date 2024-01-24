using Godot;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace FastTerrain;

public class FTBehavior {
	/// <summary>
	/// The tile that needs a behavior.
	/// </summary>
	private Vector2I tileNeedingBehavior = new();
	
	/// <summary>
	/// The name of the behavior.
	/// </summary>
	private readonly string behaviorName = "";

	/// <summary>
	/// The layers that this behavior will collide with.
	/// </summary>
	private readonly List<int> collisionMasks = new();

	/// <summary>
	/// The layer that this behavior will be on.
	/// </summary>
	private readonly int layer = 0;

	/// <summary>
	/// Create a new behavior.
	/// </summary>
	/// <param name="tileNeedingBehavior"></param>
	/// <param name="behaviorName"></param>
	/// <param name="collisionMasks"></param>
	public FTBehavior(Vector2I tileNeedingBehavior, string behaviorName, List<int> collisionMasks) {
		this.tileNeedingBehavior = tileNeedingBehavior;
		this.behaviorName = behaviorName;
		this.collisionMasks = collisionMasks;
	}

	public static FTBehavior FromJson(Godot.Collections.Dictionary json) {
		List<int> collisions = new();
		foreach (var collision in json["collision"].As<Godot.Collections.Array>()) {
			collisions.Add((int)collision);
		}
		return new FTBehavior(
			Utils.StringToVector2I((string)json["tile"]),
			(string)json["behavior"],
			collisions
		);
	}

	/// <summary>
	/// Apply behaviors to tiles.
	/// </summary>
	/// <param name="threadMap"></param>
	/// <param name="currentTiles"></param>
	/// <param name="painter"></param>
	/// <param name="chunkPosition"></param>
	public void Apply(
		TileMap threadMap, 
		Godot.Collections.Array<Vector2I> currentTiles
	) {
		List<Area2D> behaviorsToAdd = new();
		foreach (var tile in currentTiles) {
			if (threadMap.GetCellAtlasCoords(layer, tile) == tileNeedingBehavior) {
				// Add behavior
				Area2D behavior = new();
				behavior.BodyEntered += OnBodyEntered;
			}
		}
	}

	/// <summary>
	/// Our on body entered event handler.
	/// </summary>
	public void OnBodyEntered(Node body) {
		if (body.HasMethod("call_behavior")) {
			body.Call("call_behavior", new Variant[] { behaviorName });
		}
	}
}