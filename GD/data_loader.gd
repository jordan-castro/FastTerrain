## data_loader.gd
class_name DataLoader


## A list of all tiles defined in the JSON
var tiles: Array[Tile] = []

## A random number generator
var random : RandomNumberGenerator = null

## Size of full map
var size_of_map : Vector2i = Vector2i.ZERO

## The path to the tileset image
var texture_image : String = ""

## The autotiler
var auto_tiler : AutoTiler = null 

## A list of all tiles to be used in the noise pattern
var noise_tiles : Array[Tile] = []

# ## A list of spawners that will be added to the terrain
# var spawners : Array[FTSpawner] = []

## A list of behaviors
var behaviors : Array[Behavior] = []

## A list of our chunk Objects
var chunk_objects : Array[FTObjectBuilder] = []

## A list of our world Objects
var world_objects : Array[FTObjectBuilder] = []

## A list of all chunks, this is very MATHY
var chunks : Array[Chunk] = []

# ## A player spawner
# var player_spawner : FTSpawner = null

## Our data that can be accessed anywhere as [string: key]
var data : Dictionary = {}


## Load the JSON file
func load_json(path_to_json: String) -> Dictionary:
	var file = FileAccess.open(path_to_json, FileAccess.READ)
	var json_string = file.get_as_text()
	file.close()

	# Parse the JSON file
	return JSON.parse_string(json_string)


## Initiate the class
func init(path_to_json: String, world_seed:int)->void:
	data = load_json(path_to_json)
	# Setup random
	random = RandomNumberGenerator.new()
	random.seed = world_seed
	# Setup source
	texture_image = data["tileSet"]
	# Setup tiles
	for tile in data['tiles']:
		tiles.append(Tile.from_json(tile))
	# Load noise tiles
	for tile in data['noiseTiles']:
		for t in tiles:
			if t.name == tile:
				noise_tiles.append(t)
				break
	# Setup terrain
	size_of_map = Vector2i(
			random.randi_range(
				data['terrainSize']['width']['min'],
				data['terrainSize']['width']['max']
			),
			random.randi_range(
				data['terrainSize']['height']['min'],
				data['terrainSize']['height']['max']
			)
		)
	# Setup chunks
	for x in range(0, size_of_map.x, data['chunk']['width']):
		for y in range(0, size_of_map.y, data['chunk']['height']):
			chunks.append(
				Chunk.new(
					Vector2i(x, y),
					data['chunk']['width'],
					data['chunk']['height']
				)
			)

	# Setup autotiler
	var rules: Array[AutoTilerRule] = []
	for rule in data['autotileRules']:
		rules.append(AutoTilerRule.from_json(rule))
	# Autotiler setup
	auto_tiler = AutoTiler.new(
		rules,
		random
	)
	
	# Load objects
	for obj in data['objects']['world']:
		world_objects.append(
			FTObjectBuilder.new(obj, tiles, random)
		)
	for obj in data['objects']['chunk']:
		chunk_objects.append(
			FTObjectBuilder.new(obj, tiles, random)
		)

	# Load behaviors
	for behavior in data['behaviors']:
		behaviors.append(
			Behavior.from_json(behavior)
		)


## Get a tile by name
func get_tile_by_name(name: String) -> Tile:
	for tile in tiles:
		if tile.name == name:
			return tile
	return null