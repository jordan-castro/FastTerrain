## The `Chunk` class represents a chunk of the terrain in the game.
## Each chunk has a position on the grid, a width, a height, and a loaded state.
class_name Chunk

## A boolean indicating whether the chunk is loaded or not.
var is_loaded: bool = false
## Is the chunk cached?
var is_cached: bool = false
## The position of the chunk on the grid.
var position_on_grid : Vector2i = Vector2i.ZERO
## The width of the chunk.
var width : int = 100
## The height of the chunk.
var height : int = 100
## Our spawners
var spawners : Array[FTSpawner] = []
# Our painter for FastTerrain
var painter : Painter = null
# Cached dictionary
var cached : Dictionary = {
	"tiles_with_positions": [],
	"spawned_nodes": [],
}


## The constructor for the `Chunk` class.
## It initializes the position on the grid, width, and height of the chunk.
func _init(position_on_grid_: Vector2i, width_: int, height_: int)->void:
	position_on_grid = position_on_grid_
	width = width_
	height = height_

## Set's necessary variables for the chunk to be loaded.
func load(painter_: Painter, full:bool=true) -> void:
	painter = painter_
	# If not full, we just want access to the painter.
	if not full:
		return
	
	if is_cached:
		# Load cached data
		__load_from_cache()
		is_loaded = true
		return
	
	load_terrain()
	# for spawner in painter.data_loader.spawners:
	# 	load_spawner(spawner)
	for b in painter.data_loader.behaviors:
		load_behavior(b)
	is_loaded = true


func rect()->Rect2:
	return Rect2(position_on_grid, Vector2(width, height))


## Returns an array of tiles in the chunk.
## Each tile is represented as an array with two elements: the position of the tile on the grid and the tile itself.
func get_tiles() -> Array[TileWithPosition]:
	return painter.data_loader.terrain.grid_system.box_safe(position_on_grid, width, height)


func load_terrain()->void:
	painter.data_loader.terrain.build(painter.data_loader, self)


## Spawn a [Node] into the [Painter]
func spawn_into_painter(node: String, position: Vector2i, node_path : String)->void:
	spawners.append(
		FTSpawner.new(node, position)
	)

	if not is_cached:
		cached['spawned_nodes'].append(
			[node_path, position]
		)


func load_behavior(behavior: Behavior)->void:
	# Get all tiles of behavior.
	var tiles_needing_this_behavior: Array[Vector2i] = painter.data_loader.terrain.grid_system.get_cells_by_type(
		[behavior.tile],
		painter.data_loader.terrain.grid_system.box_safe(
			position_on_grid,
			width,
			height,
		)
	)

	if tiles_needing_this_behavior.size() == 0:
		return

	var collision_shape : CollisionShape2D = null
	var area : BehaviorArea = null

	for tile in tiles_needing_this_behavior:
		collision_shape = CollisionShape2D.new()
		collision_shape.shape = RectangleShape2D.new()
		collision_shape.shape.size = Vector2(20, 25)

		# Add area
		area = BehaviorArea.new(behavior.behavior)
		area.set_collision_layer_value(4, true)
		area.set_collision_layer_value(1, false)
		area.set_collision_mask_value(3, true)

		area.position = painter.map_to_local(tile)
		area.connect("behavior_body_entered", painter._on_behavior_area_body_entered)
		area.call_deferred("add_child", collision_shape)

		painter.behaviors_node.call_deferred("add_child", area)


## Unload a chunk to free up memory.
func unload() -> void:
	pass
	# if not is_loaded:
	# 	return
	# # Remove them from the painter
	# for spawner in spawners:
	# 	if spawner.node.is_inside_tree():
	# 		spawner.node.queue_free()
	# spawners = []
	
	# is_cached = true
	# is_loaded = false

## Load a chunk from cache.
func __load_from_cache():
	pass
	# for spawner_data in cached['spawned_nodes']:
	# 	spawners.append(
	# 		FTSpawner.new(
	# 			load(spawner_data[0]).instantiate(),
	# 			spawner_data[1]
	# 		)
	# 	)
		# spawner.node = spawner.node.duplicate()
		# spawners.append(spawner)