class_name FTObject

var grid_system : GridSystem = null
var args : Dictionary = {}
var random : RandomNumberGenerator = null
var chunk : Chunk = null
var direction : int = -1


func build(
	grid_system_: GridSystem,
	args_: Dictionary,
	random_: RandomNumberGenerator,
	chunk_: Chunk
)->void:
	grid_system = grid_system_
	args = args_
	random = random_
	chunk = chunk_

	direction = args.get("direction", -1)


## Get the tiles of the object can spawn on within it's chunk
func get_spawnable_tiles()->Array[TileWithPosition]:
	return GridSystem.filter_tiles_as_array(chunk.get_tiles(), args['spawnOn'])


## Check if the object can be spawned at the given position
func can_build_here(pos: Vector2i, width: int, height: int)-> bool:
	# Check if object would be inside the chunk
	if pos.x < chunk.position_on_grid.x or \
		pos.y < chunk.position_on_grid.y or \
		pos.x + width > chunk.position_on_grid.x + chunk.width or \
		pos.y + height > chunk.position_on_grid.y + chunk.height:
		return false

	var box : Array[TileWithPosition] = grid_system.box_safe(pos, width, height)

	for tile in box:
		if not tile.tile.is_empty():
			return false
	
	return true


## Check that the tile is valid. Checks it is not out of bounds, and that it is empty.
func check_tile(tile: TileWithPosition)->bool:
		# Check if the tile above is part of the chunk
	if tile.grid_position.y + direction < chunk.position_on_grid.y:
		return false

	if not grid_system.get_cell_safe(tile.grid_position.x, tile.grid_position.y + direction):
		return false
	if not grid_system.get_cell_safe(tile.grid_position.x, tile.grid_position.y + direction).is_empty():
		return false


	return true

## This is a default implementation of the spawn function
static func default(object: FTObject)->void:
	var where_to_build := object.get_spawnable_tiles()
	var tile_to_spawn: Tile = null

	for tile in where_to_build:
		if object.random.randf() > object.args['rarity']:
			continue

		if not object.check_tile(tile):
			continue
		
		if object.args.has("tiles"):
			tile_to_spawn = object.args['tiles'][object.random.randi() % len(object.args['tiles'])]
		else:
			tile_to_spawn = object.args['tile']

		object.grid_system.set_cell_safe(tile.grid_position.x, tile.grid_position.y + object.direction, tile_to_spawn)
