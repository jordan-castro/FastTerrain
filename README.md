# FastTerrain: An Advanced Terrain Generation Algorithm for Pixel Ai Dash

FastTerrain represents the cutting-edge algorithm powering the terrain generation in Pixel Ai Dash. Developed using C# and fully compatible with the Godot engine, this algorithm is renowned for its speed, simplicity, and ease of use. The design of FastTerrain allows for seamless integration across various programming languages and frameworks.

## Operational Mechanics of FastTerrain
An in-depth exploration of FastTerrain's functionality reveals the intricacies of the algorithm:

### Step 1: Procedural Generation (FastNoiseLite)
The terrain creation process in Pixel Ai Dash begins with setting a seed and generating noise using [FastNoiseLite](url), an integral module within Godot.

### Step 2: Auto Tile
Post-noise generation, the terrain comprises tiles that lack definitive structure. To introduce coherence, Godot's Auto Tile features are employed, significantly enhancing the original FastTerrain algorithm that used a slower, less efficient custom AutoTiler.

### Step 3: Object Integration
With auto tiling complete, the terrain gains structure and predictability, essential for immersive player experience. However, the terrain remains relatively barren, necessitating the addition of Objects (e.g., trees, grass, mushrooms). Object placement follows logical patterns (trees on grass, coins in empty spaces) using FastTerrain's custom algorithm, FTObject.

### Step 4: Node Addition
The integration of essential nodes follows object placement. Nodes like Enemies and SignPosts are added using FTNode, a custom class in FastTerrain, which inherits from FTObject. The distinction lies in the placement function (`spawn` with varied parameters), and `spawn` outputs a List<Vector2I>, indicating node positions for scene incorporation via CallDeffered.

### Step 5: Chunk Management
All aforementioned steps occur within the FTPainter _Process function, utilizing a chunking system that dynamically loads and unloads chunks based on the player's location.

## Implementation Guidelines
To utilize FastTerrain, instantiate a TileMap node in your editor and attach the FTPainter.cs or ft_painter.gd script. Adding a player object will trigger automatic terrain generation.

### Custom Object Integration
Incorporating custom objects involves creating a JSON file and adding a new object under `objects['chunks']`. The FTObject.cs (or .gd equivalent) requires updating in the FromJson function:

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

The process is similar for FTNode, with differences in the class structure:

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

## Adding custom Behaviors
To add new behaviors, create a JSON object in the behaviors array and implement the logic in your player class:

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

## The Future of FastTerrain
FastTerrain is a robust, standalone algorithm created for Pixel Ai Dash. Users are encouraged to adopt it in their projects. For further inquiries or discussions, join our Discord Server and visit the #fastterrain channel.

We appreciate your interest in FastTerrain and invite you to explore Pixel Ai Dash on our supported [platforms](link to platforms).

