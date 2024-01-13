## painter.gd
class_name Painter extends TileMap


## Path to the data file
@export var data_file_path : String = ""

## The world seed
@export var world_seed : int = 01312002

## The player
@onready var player : Player = $Player

## Dataloader
var data_loader : DataLoader = DataLoader.new()

var count : int = 0

func _ready():
	print("Game START")
	# Load data
	data_loader.init(data_file_path, world_seed)
	var t_set = load(data_loader.texture_image)
	tile_set = t_set

	# Initialize chunks
	for chunk in data_loader.chunks:
		chunk.init(t_set, map_to_local(chunk.position_on_grid), self)
		# chunk.tile_set = t_set
		# chunk.global_position = map_to_local(chunk.position_on_grid)
		add_child(chunk)

	print("Loaded data.")
	print("Chunks ", data_loader.chunks.size())

	# Load player chunk
	var player_chunk = data_loader.chunks[(data_loader.random.randi() + 1) % data_loader.chunks.size()]
	player_chunk.load(null)
	print("Loaded terrain")

	print("Loaded player chunk.")

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

	player.position = player_chunk.tile_map.map_to_local(tile_to_spawn_on) + map_to_local(player_chunk.position_on_grid)
	

## When a body enters a behavior it calls a "call_behavior" method on the body.
## This function is triggered when a body enters a behavior area.
## It checks if the body has a "call_behavior" method and if it does, it calls that method with the behavior name.
func _on_behavior_area_body_entered(body: Node, behavior_area: BehaviorArea)->void:
	if body.has_method("call_behavior"):
		body.call_behavior(behavior_area, self)


func _set_cell(position_: Vector2i, tilename: String)->void:
	var tile : Tile = data_loader.get_tile_by_name(tilename)
	if tile == null:
		return

	# Find the chunk that the tile is in
	var chunk : Chunk = null
	
	# Find the chunk that the tile is in
	for c in data_loader.chunks:
		# Check that position_ is in the chunk
		if c.rect.has_point(position_):
			chunk = c
			break
	
	# Get the position of the tile on the chunk
	var tile_position_on_chunk : Vector2i = Vector2i(
		position_.x - chunk.position_on_grid.x - 1, # It is always -1
		position_.y - chunk.position_on_grid.y - 1 # It is always -1
	)
	
	# Set chunk grid_system
	chunk.terrain.grid_system.set_cell(
		tile_position_on_chunk.x,
		tile_position_on_chunk.y,
		tile
	)
	# Set chunk tile_map
	chunk.tile_map.set_cell(
		0,
		tile_position_on_chunk,
		0,
		tile.atlas,
		tile.alt,
	)

