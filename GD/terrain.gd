## terrain.gd
class_name Terrain


## Size of the terrain
var size : Vector2i = Vector2i.ZERO

## Grid system
var grid_system : GridSystem = null
## Noise Only Grid System. This is used to stop any holes from appearing in the terrain.
## Holes are caused by the auto tiler and the add_objects function.
var noise_grid_system : GridSystem = null

var current_chunk : Chunk = null


func _init(size_: Vector2i)->void:
	size = size_

	# Fill grid
	grid_system = GridSystem.new(size, Tile.EMPTY())
	noise_grid_system = GridSystem.new(size, Tile.EMPTY())


## This function is used privately to build our terrain
func __build_terrain__(
	noise_tiles : Array[Tile],
	random : RandomNumberGenerator,
	auto_tiler : AutoTiler = null,
	tiles : Array[Tile] = [],
	objects : Array[FTObjectBuilder] = []
)->void:
	_generate_noise(noise_tiles, random)

	if auto_tiler != null:
		_add_borders(auto_tiler, tiles, noise_grid_system)

	if objects.size() > 0:
		_add_objects(objects)

	if auto_tiler != null:
		_add_borders(auto_tiler, tiles, grid_system)
	

## This is the default build function. It should be called in most cases.
func build(
	data_loader : DataLoader,
	chunk : Chunk,
)->void:
	current_chunk = chunk
	__build_terrain__(
		data_loader.noise_tiles,
		data_loader.random,
		data_loader.auto_tiler,
		data_loader.tiles,
		data_loader.chunk_objects
	)


func _generate_noise(noise_tiles:Array[Tile], random: RandomNumberGenerator)->void:
	# Noise stuff
	var scale : int = 1
	var fast_noise = FastNoiseLite.new()
	fast_noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	fast_noise.seed = random.seed
	fast_noise.fractal_octaves = noise_tiles.size()

	# Generate noise
	for x: int in range(size.x):
		# Make sure the noise does not go out of bounds
		if x > size.x - 1:
			break
		if x < 0:
			continue
		for y: int in range(size.y):
			# Make sure the noise does not go out of bounds
			if y > size.y - 1:
				break
			if y < 0:
				continue
			
			# To stop the noise from filling already filled tiles
			if not grid_system.get_cell(x, y).is_empty():
				continue

			else:
				var noise : float = fast_noise.get_noise_2d(float(x + current_chunk.position_on_grid.x) / scale, float(y + current_chunk.position_on_grid.y) / scale)
				var tile: Tile = noise_tiles[int(floor(abs(noise) * noise_tiles.size()))]
				grid_system.set_cell(x, y, tile)
				noise_grid_system.set_cell(x, y, tile)


func _add_borders(auto_tiler:AutoTiler, terrain_tiles:Array[Tile], system: GridSystem)->void:
	if auto_tiler.rules.size() == 0:
		return
	
	var place_holder_tiles : Array[TileWithPosition] = []
	var tile : Tile = null
	var name_of_tile_to_use : String = ""

	# Loop through the grid
	for x in range(current_chunk.width):
		for y in range(current_chunk.height):
			# Make sure the terrain does not go out of bounds
			if x <= 0 or x >= size.x - 1 or y <= 0 or y >= size.y - 1:
				continue
			
			tile = system.get_cell_safe(x, y)
			# Does not require border
			if tile == null or tile.is_empty():
				continue
			
			name_of_tile_to_use = auto_tiler.decide_tile(
				tile,
				system.get_neighbors(x, y),
				system
			)
			for t in terrain_tiles:
				if t.name == name_of_tile_to_use:
					tile = t
					break
			
			place_holder_tiles.append(
				TileWithPosition.new(
					tile,
					Vector2i(x, y)
				)
			)
	
	# Loop through the placeholder tiles and add them to the grid.
	for t in place_holder_tiles:
		grid_system.set_cell(
			t.grid_position.x,
			t.grid_position.y,
			t.tile
		)


func _add_objects(objects: Array[FTObjectBuilder])->void:
	for obj in objects:
		obj.build(grid_system, current_chunk)