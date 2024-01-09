## ft_objects/ft_object.gd
## FTObjectBuilder stands for "Fast Terrain Object". These are the objects which are defined in the JSON file.
class_name FTObjectBuilder

## The name of the object. This is used to identify the object in the JSON file.
var name: String = "FTObjectBuilder"

## The arguments for the builder
var args: Dictionary = {}


func _init(json: Dictionary, tiles: Array[Tile], random_: RandomNumberGenerator) -> void:
	name = json["name"]
	json.erase("name")

	json["random"] = random_

	# The rest are the arguments which are different to each object
	args = json
	# Check if we need to load any tile data
	if args.has("loadFor"):
		load_tile_data(tiles)


## Loads tile data from the given JSON file.
func load_tile_data(tiles: Array[Tile]) -> void:
	for key in args.keys():
		var value = args[key]
		var changed_to_array = false
		# Do not need to load this
		if not key in args["loadFor"]:
			continue

		# If the value is a string, then it is a tile name
		if value is String:
			value = [value]
			changed_to_array = true

		for i in range(value.size()):
			var tile_name = value[i]
			for tile in tiles:
				if tile.name == tile_name:
					value[i] = tile
					break

		if changed_to_array:
			value = value[0]
		args[key] = value


## Build the object at the given position
func build(grid_system: GridSystem, chunk: Chunk) -> void:
	var builder: Variant = null
	var class_list = ProjectSettings.get_global_class_list()
	for cl in class_list:
		if cl["class"] == name:
			builder = load(cl["path"]).new()

	builder.build(grid_system, args, args["random"], chunk)