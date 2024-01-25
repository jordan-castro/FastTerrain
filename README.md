# FastTerrain

FastTerrain is the algorithm behind Pixel Ai Dash's terrain generation. It is a fast, simple, and easy to use algorithm that generates terrain in a 2D array. It is written in C# and is compatible with Godot. The implementation is pretty simple, and can be implemented in any language and framework.

## How it works
So how does FastTerrain work it's magic? Let's take a closer look at the algorithm.

### Step 1: Procedural Generation (FastNoiseLite)
The first step in creating PAD's terrain is setting the seed and generating noise. To do this, we use [FastNoiseLite](url), a built in Godot module.

### Step 2: Auto Tile
Then after the noise is planted, we have a bunch of tiles that are basically 1-dimensional in being that it doesnt follow any rules. There are no boundries, and there is nothing to expect, rather just choosing a tile at random. So, in order to fix that we use Godots built in Auto Tile features. The original FastTerrain algorithm had a custom AutoTiler built in, but it was slow and inefficient.

### Step 3: Add Objects
After auto tiling, we have a blank world with noise that has edges and follows some rules which means you can expect what the world will look like. Which is important for the player to have a feeling that it is like a real world. But, nonetheless it is still pretty blank. So we add in Objects to the world. Objects are things like trees, grass, mushrooms, etc. They are placed in the world at random, but they are placed in a way that makes sense. For example, trees are placed on grass, coins are placed on empty space, hearts are placed above grass, etc. This is done using a custom algorithm built into FastTerrain, called FTObject.

### Step 4: Add Nodes
After objects are placed, we add any nodes that are important to the chunk. To add nodes, we use another custom class built into FastTerrain, called FTNode. Nodes are things like Enemies, SignPosts, etc. They have the same logic as a FTObject because FTNode inherits from FTObject. The only difference is that FTNode uses a different function to place the nodes in the world. While FTObject uses `build` FTNode uses `spawn` with different parameters. Looking closer at any class that inherits FTNode or FTObject you can see how they work. And `spawn` returns a List<Vector2I> which is a list of all the positions that the node was placed at. This is needed for our FTPainter object to add them to the scene using CallDeffered.

### Step 5: Chunking
All of these steps are done within the FTPainter _Process function. There is a chunking system which loads and unloads chunks based on the `player`s position.

## How to use
First you need to instantiate a TileMap node in your editor. Then Attach either the FTPainter.cs or ft_painter.gd script. Add a player object and vioala. The rest will be done automatically.

## How to add custom objects
To add custom objects, you need to create a JSON file like so: 
```json
{
	"objects": {
		"chunk": [
			{
				"name": "FTTree",
				"rarity": 0.1,
				"spawnOn": [
					"GrassTallMiddle",
					"GrassTallLeft",
					"GrassTallRight",
					"GrassShortSingle",
					"GrassTallSingle"
				],
				"options": {
					"base": "TreeBase",
					"body": "TreeBodyNoBranches",
					"top": "TreeTop",
					"leaf": "TreeLeafMiddle",
					"branches": [
						"TreeBranchEmpty",
						"TreeBranchLeafs"
					]
				},
				"terrains": {
					"body": 1,
					"leaf": 2
				}
			},
			{
				"name": "MushroomTree",
				"rarity": 0.1,
				"spawnOn": [
					"GrassTallMiddle",
					"GrassTallLeft",
					"GrassTallRight",
					"GrassShortSingle",
					"GrassTallSingle"
				],
				"options": {
					"top": "MushroomHead",
					"head": "MushroomHeadMiddle",
					"body": "MushroomBody3Spots",
					"base": "MushroomBase"
				},
				"terrains": {
					"body": 3,
					"head": 4
				}
			},
			{
				"name": "Default",
				"rarity": 0.05,
				"spawnOn": [
					"Empty",
					"TreeLeafTopMiddle",
					"MushroomHeadMiddle",
					"MushroomHead"
				],
				"options": {
					"tile": "Coin"
				}
			},
			{
				"name": "Grass",
				"rarity": 0.3,
				"spawnOn": [
					"GrassTallMiddle",
					"GrassTallLeft",
					"GrassTallRight",
					"GrassShortSingle",
					"GrassTallSingle",
					"GrassShortLeft",
					"GrassShortMiddle",
					"GrassShortRight",
					"DirtMiddleEnd",
					"DirtLeftEnd",
					"DirtRightEnd"
				],
				"options": {
					"tiles": [
						"GrassShort",
						"GrassTall"
					],
					"altTiles": [
						"GrassShortAlt",
						"GrassTallAlt"
					],
					"useAltFor": [
						"DirtMiddleEnd",
						"DirtLeftEnd",
						"DirtRightEnd"
					]
				}
			},
			{
				"name": "Default",
				"rarity": 0.1,
				"spawnOn": [
					"GrassTallMiddle",
					"GrassTallLeft",
					"GrassTallRight",
					"GrassShortSingle",
					"GrassTallSingle"
				],
				"options": {
					"tiles": [
						"RedMushroomSmall",
						"GreyMushroomSmall"
					]
				}
			},
			{
				"name": "EnemySpawner",
				"rarity": 0.1,
				"spawnOn": [
					"GrassTallMiddle"
				],
				"options": {
					"max": 2,
					"node": "res://scenes/characters/enemies/spike_enemy.tscn"
				}
			}
		],
		"world": [
			{
				"name": "End",
				"spawnOn": [
					"Empty"
				],
				"options": {
					"top": "FlagTop",
					"bottom": "FlagBottom"
				}
			}
		]
	},
	"behaviors": [
		{
			"tile": "MushroomHeadMiddle",
			"behavior": "Bounce",
			"collision": [
				6
			],
			"direction": 1
		},
		{
			"tile": "FlagTop",
			"behavior": "Finish",
			"collision": [
				6
			],
			"direction": 0
		},
		{
			"tile": "Coin",
			"behavior": "CollectCoin",
			"collision": [
				6
			],
			"direction": 0
		},
		{
			"tile": "Spikes",
			"behavior": "TakeDamage",
			"collision": [
				6
			],
			"direction": 0
		}
	],
	"tiles": [
		{
			"name": "Empty",
			"atlas": {
				"x": -1,
				"y": -1
			}
		},
		{
			"name": "GrassShortSingle",
			"atlas": {
				"x": 0,
				"y": 0
			}
		},
		...
	]
}
```
Under objects['chunks'] you add a new JSON object, containing the name of the class, and paramaters. Then in FTObject.cs `or .gd equivalent` in your FromJson function make sure to add that name and the contstrutor as a response in the switch statement. Liek this:

```csharp

	return (string)json["name"] switch
	{
		"NameOfCustomObject" => new NameOfCustomObject(spawnOn, specificTiles, rarity, terrains), // These values are already created in the FromJson function, just refrence them here.
		null => throw new NotImplementedException(),
		_ => null,
	};

```

```csharp
using Godot;
using FastTerrain;

namespace FTObjects; // <-- This is optional, but it is recommended to keep all your custom objects in a namespace.

public class NameOfCustomObject : FTObject 
{
	public NameOfCustomObject(List<string> spawnOn, List<string> specificTiles, float rarity, Dictionary<string, int> terrains) : base(spawnOn, specificTiles, rarity, terrains)
	{
		// This is the constructor for the object. It is called when the object is created.
		// You can add any custom paramaters here.
	}

    public override void Build(Array<Vector2I> tiles, TileMap threadMap, Vector2I chunkPosition, Vector2I worldSize, int chunkSize, RandomNumberGenerator random)
	{
		// This is the build function. It is called when the object is placed in the world.
		// You can add any custom logic here.
		// FTobject base class comes with many Helper functions that you can use.
	}
}
```

!Note
The same process applies to FTNode.cs `or .gd` equivalent.
The only different is that FTNode looks like this:

```csharp
using Godot;
using FastTerrain;

namespace FTObjects; // <-- This is optional, but it is recommended to keep all your custom objects in a namespace.

public class NameOfCustomNode : FTNode
{
	public NameOfCustomNode(List<string> spawnOn, List<string> specificTiles, float rarity, Dictionary<string, int> terrains) : base(spawnOn, specificTiles, rarity, terrains)
	{
		// This is the constructor for the object. It is called when the object is created.
		// You can add any custom paramaters here.
	}

	public override List<Vector2I> Spawn(Array<Vector2I> tiles, TileMap threadMap, Vector2I chunkPosition, Vector2I worldSize, int chunkSize, RandomNumberGenerator random)
	{
		// This is the spawn function. It is called when the object is placed in the world.
		// You can add any custom logic here.
		// FTobject base class comes with many Helper functions that you can use.
		return new List<Vector2I>();
	}
}
```

## How to add custom behaviors?
Adding custom behaviors is pretty simple. You just need to create a new JSON object in the behaviors array. Like this:

```json
{
	"tile": "MushroomHeadMiddle",
	"behavior": "Bounce",
	"collision": [
		6
	],
	"direction": 1
}
```

Now in order for the behavior to work, you have to implement the logic in your player class. That is beyond the scope of FastTerrain and is up to you to implement.

## The Future of FastTerrain
FastTerrain has no future plans. It is a simple algorithm that I wrote to use in the game Pixel Ai Dash. If you like it, feel free to use it in your own projects. If you have any questions, feel free to join our [Discord Server](url) and ask in the #fastterrain channel.

Thank you for reading this far. I hope you enjoy using FastTerrain as much as I enjoyed making it.
And check out Pixel Ai Dash on our supported [platforms](link to platforms)!