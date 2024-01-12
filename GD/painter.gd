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

var nodes_to_add = {}


func _ready():
	print("Game START")
	# Load data
	data_loader.init(data_file_path, world_seed)
	var t_set = load(data_loader.texture_image)
	tile_set = t_set

	# Initialize chunks
	for chunk in data_loader.chunks:
		chunk.init(t_set, map_to_local((chunk.position_on_grid)))
		# chunk.tile_set = t_set
		# chunk.global_position = map_to_local(chunk.position_on_grid)
		add_child(chunk)

	print("Loaded data.")
	print("Chunks ", data_loader.chunks.size())

	# Load player chunk
	var player_chunk = data_loader.chunks[(data_loader.random.randi() + 1) % data_loader.chunks.size()]
	player_chunk.load(self)
	print("Loaded terrain")

	print("Loaded player chunk.")
	await get_tree().create_timer(0.2).timeout

	# Set player position
	var possible_spawn_tiles = player_chunk.terrain.grid_system.get_cells_by_type(
		["GrassTallMiddle", "GrassTallLeft", "GrassTallRight"],
		player_chunk.terrain.grid_system.box_safe(
			Vector2i.ZERO,
			player_chunk.width,
			player_chunk.height,
		)
	)

	print("Possible spawn tiles: ", possible_spawn_tiles.size())

	var tile_to_spawn_on = possible_spawn_tiles[
		(data_loader.random.randi() + 1) % possible_spawn_tiles.size()
	]
	while tile_to_spawn_on.y == player_chunk.height:
		tile_to_spawn_on = possible_spawn_tiles[
			(data_loader.random.randi() + 1) % possible_spawn_tiles.size()
		]
	
	tile_to_spawn_on.y -= 1

	player.global_position = player_chunk.tile_map.map_to_local(tile_to_spawn_on) + map_to_local(player_chunk.position_on_grid)
	
	# Start chunk loader thread.
	chunk_loader_thread.start(load_chunks)


## When a body enters a behavior it calls a "call_behavior" method on the body.
## This function is triggered when a body enters a behavior area.
## It checks if the body has a "call_behavior" method and if it does, it calls that method with the behavior name.
func _on_behavior_area_body_entered(body: Node, behavior_area: BehaviorArea)->void:
	if body.has_method("call_behavior"):
		body.call_behavior(behavior_area, self)


func _process(_delta):
	if player == null:
		return
	
	var new_pos = local_to_map(player.global_position)
	if new_pos.x != player_position_for_thread.x or new_pos.y != player_position_for_thread.y:
		player_position_for_thread = new_pos


func load_chunks()->void:
	var ppp = Vector2i.ZERO

	while true:
		ppp = Rect2(
			player_position_for_thread.x - data_loader.data["chunk"]["width"] / 2,
			player_position_for_thread.y - data_loader.data["chunk"]["height"] / 2,
			data_loader.data["chunk"]["width"],
			data_loader.data["chunk"]["height"]
		)

		for chunk in data_loader.chunks:
			if chunk.is_loaded:
				# data_loader.chunks[data_loader.chunks.find(chunk)] = null
				continue
			if ppp.intersects(chunk.rect):
				load_chunk(chunk)


func load_chunk(chunk: Chunk)->void:
	# For debugging
	var start_time = Time.get_unix_time_from_system()
	# Initalize chunk
	chunk.load(self)
	# call_deferred("load_tile_set", chunk)
	var end_time = Time.get_unix_time_from_system()

	print("Loaded chunk in " + str(end_time - start_time) + " seconds.")


func _set_cell(position_: Vector2i, tilename: String)->void:
	var tile : Tile = data_loader.get_tile_by_name(tilename)
	if tile == null:
		return

	# Find the chunk that the tile is in
	var chunk : Chunk = null

	for c in data_loader.chunks:
		# Check that position_ is in the chunk
		if c.rect.has_point(position_):
			chunk = c
			break

	var tile_position_on_chunk : Vector2i = Vector2i(
		position_.x - chunk.position_on_grid.x - 1,
		position_.y - chunk.position_on_grid.y - 1
	)

	chunk.terrain.grid_system.set_cell(
		tile_position_on_chunk.x,
		tile_position_on_chunk.y,
		tile
	)
	print(tile_position_on_chunk)
	# print(chunk.terrain.grid_system.get_cell(tile_position_on_chunk.x, tile_position_on_chunk.y).name)
	chunk.tile_map.set_cell(
		0,
		tile_position_on_chunk,
		0,
		tile.atlas,
		tile.alt,
	)