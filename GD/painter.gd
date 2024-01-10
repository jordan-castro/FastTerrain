## painter.gd
class_name Painter extends TileMap


## Path to the data file
@export var data_file_path : String = ""

## The world seed
@export var world_seed : int = 01312002

## The player
@export var player : Player = null

@onready var behaviors_node : Node = get_parent().get_node("Behaviors")
@onready var enemies_node : Node = get_parent().get_node("Enemies")

## The chunk loader thread
var chunk_loader_thread : Thread = Thread.new()

## The world chunk loader
var world_chunk_loader_thread : Thread = Thread.new()

## Dataloader
var data_loader : DataLoader = DataLoader.new()

## Player position for thread
var player_position_for_thread : Vector2i = Vector2i.ZERO


func _ready():
	print("Game START")
	# Load data
	data_loader.init(data_file_path, world_seed)
	tile_set = load(data_loader.texture_image)

	print("Loaded data.")

	# Load player chunk
	var player_chunk = data_loader.chunks[(data_loader.random.randi() + 1) % data_loader.chunks.size()]
	data_loader.chunks.remove_at(data_loader.chunks.find(player_chunk))
	player_chunk.load(self, false)
	player_chunk.load_terrain()
	print("Loaded terrain")

	for behavior in data_loader.behaviors:
		player_chunk.load_behavior(behavior)
	load_tile_set(player_chunk)
	print("Loaded player chunk.")

	# # Load the world chunk in a separate thread
	# var load_world_chunk : Callable = func ():
	# 	# Load world objects.
	# 	# Use a world chunk to load all the world objects at once!
	# 	var world_chunk = Chunk.new(
	# 		Vector2i(0, 0),
	# 		data_loader.terrain.size.x,
	# 		data_loader.terrain.size.y,
	# 	)
	# 	world_chunk.load(self, false)
	# 	# Load world objects
	# 	data_loader.terrain.current_chunk = world_chunk
	# 	data_loader.terrain._add_objects(data_loader.world_objects)
	# 	data_loader.terrain._add_borders(data_loader.auto_tiler, data_loader.tiles, data_loader.terrain.grid_system)

	# 	for behavior in data_loader.behaviors:
	# 		world_chunk.load_behavior(behavior)
	# 	load_tile_set(world_chunk)
	# 	# call_deferred("load_tile_set", false, world_chunk)
	# 	print("Loaded world chunk.")

	# world_chunk_loader_thread.start(load_world_chunk)

	# Set player position
	var possible_spawn_tiles = data_loader.terrain.grid_system.get_cells_by_type(
		["GrassTallLeft"],
		data_loader.terrain.grid_system.box_safe(
			player_chunk.position_on_grid,
			player_chunk.width,
			player_chunk.height,
		)
	)

	if possible_spawn_tiles.size() == 0:
		return

	var tile_to_spawn_on = possible_spawn_tiles[
		(data_loader.random.randi() + 1) % possible_spawn_tiles.size()
	]
	tile_to_spawn_on.y -= 1

	player.global_position = map_to_local(tile_to_spawn_on)


func load_tile_set(chunk: Chunk = null)->void:
	# Add tiles
	var x_range_start = 0 if chunk == null else chunk.position_on_grid.x
	var x_range_end = data_loader.terrain.size.x if chunk == null else chunk.position_on_grid.x + chunk.width

	var y_range_start = 0 if chunk == null else chunk.position_on_grid.y
	var y_range_end = data_loader.terrain.size.y if chunk == null else chunk.position_on_grid.y + chunk.height

	for x in range(x_range_start, x_range_end):
		for y in range(y_range_start, y_range_end):
			# Load tile
			var tile : Tile = data_loader.terrain.grid_system.get_cell_safe(x, y)
			# Skip if empty
			if tile == null or tile.is_empty():
				continue
			# Add tile
			set_cell(
				0, # This is the layer. We only have one layer. If you want more layers, you will need to add them. (See how-to-add-more-layers.md)
				Vector2i(x, y), # This is the position of the tile.
				0, # This is the tile index. We only have one tileset. If you want more tilesets, you will need to add them. (See how-to-add-more-tilesets.md)
				tile.atlas, # Our tile atlas. This is the tile we want to load.
				tile.alt
			)

	for spawner in chunk.spawners:
		enemies_node.add_child(spawner.node)
		spawner.node.set_position(map_to_local(spawner.grid_position))


## When a body enters a behavior it calls a "call_behavior" method on the body.
## This function is triggered when a body enters a behavior area.
## It checks if the body has a "call_behavior" method and if it does, it calls that method with the behavior name.
func _on_behavior_area_body_entered(body: Node, behavior_area: BehaviorArea)->void:
	if body.has_method("call_behavior"):
		body.call_behavior(behavior_area, self)


func _process(_delta):
	if player == null:
		return

	player_position_for_thread = local_to_map(player.global_position)

	if not chunk_loader_thread.is_alive():
		chunk_loader_thread = Thread.new()
		chunk_loader_thread.start(load_chunks)


func load_chunks()->void:
	var ppp = Vector2i.ZERO
	# var ppn = Vector2i.ZERO
	var chunk_rects = []
	for chunk in data_loader.chunks:
		chunk_rects.append(
			[
				chunk,
				chunk.rect()
			]
		)

	while data_loader.chunks.size() > 0:
		ppp = Rect2(
			player_position_for_thread.x - data_loader.data["chunk"]["width"] / 2,
			player_position_for_thread.y - data_loader.data["chunk"]["height"] / 2,
			data_loader.data["chunk"]["width"],
			data_loader.data["chunk"]["height"]
		)
		# ppn = Rect2(
		# 	player_position_for_thread.x - data_loader.data["chunk"]["width"] / 2,
		# 	player_position_for_thread.y - data_loader.data["chunk"]["height"] / 2,
		# 	data_loader.data["chunk"]["width"] * 2,
		# 	data_loader.data["chunk"]["height"] * 2
		# )

		for chunk_rect in chunk_rects:
			if ppp.intersects(chunk_rect[1]):
				if not chunk_rect[0].is_loaded:
					load_chunk(chunk_rect[0])
			# elif ppn.intersects(chunk_rect[1]):
			# 	if chunk_rect[0].is_loaded:
			# 		unload_chunk(chunk_rect[0])


func load_chunk(chunk: Chunk)->void:
	# For debugging
	var start_time = Time.get_unix_time_from_system()
	# Initalize chunk
	chunk.load(self)
	call_deferred("load_tile_set", chunk)
	# call_deferred("load_tile_set", chunk)
	var end_time = Time.get_unix_time_from_system()

	print("Loaded chunk in " + str(end_time - start_time) + " seconds.")


func unload_chunk(chunk: Chunk)->void:
	chunk.unload()


func _set_cell(position_: Vector2i, tilename: String)->void:
	var tile : Tile = data_loader.get_tile_by_name(tilename)
	if tile == null:
		return

	data_loader.terrain.grid_system.set_cell(position_.x, position_.y, tile)
	set_cell(
		0,
		position_,
		0,
		tile.atlas,
		tile.alt,
	)