## The `Chunk` class represents a chunk of the terrain in the game.
## Each chunk has a position on the grid, a width, a height, and a loaded state.
class_name Chunk extends Node2D

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
## Our painter for FastTerrain
var painter : Painter = null
## Our terrain, each chunk has a terrain.
var terrain : Terrain = null
## Our tilemap which we draw our terrain on.
var tile_map : TileMap = TileMap.new()

var rect = null


## The constructor for the `Chunk` class.
## It initializes the position on the grid, width, and height of the chunk.
func _init(position_on_grid_: Vector2i, width_: int, height_: int)->void:
	position_on_grid = position_on_grid_
	width = width_
	height = height_
	# Set rect and terrain.
	rect = Rect2(position_on_grid, Vector2(width, height))
	terrain = Terrain.new(Vector2i(width, height))


func init(tile_set: TileSet, gb_position: Vector2):
	# Set tilemap
	tile_map.tile_set = tile_set
	tile_map.global_position = gb_position


## Set's necessary variables for the chunk to be loaded.
func load(painter_: Painter) -> void:
	painter = painter_
	
	if is_cached:
		visible = true
		is_loaded = true
		return
	
	load_terrain()
	for b in painter.data_loader.behaviors:
		load_behavior(b)
	is_loaded = true
	draw_terrain()


## Returns an array of tiles in the chunk.
## Each tile is represented as an array with two elements: the position of the tile on the grid and the tile itself.
func get_tiles() -> Array[TileWithPosition]:
	return terrain.grid_system.box_safe(Vector2i(0, 0), width, height)


func load_terrain()->void:
	terrain.build(painter.data_loader, self)


## Spawn a [Node] into the [Painter]
func spawn_into_painter(node: String, position_: Vector2i)->void:
	spawners.append(
		FTSpawner.new(node, position_)
	)


func load_behavior(behavior: Behavior)->void:
	# Get all tiles of behavior.
	var tiles_needing_this_behavior: Array[Vector2i] = terrain.grid_system.get_cells_by_type(
		[behavior.tile],
		terrain.grid_system.box_safe(
			Vector2i.ZERO,
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

		area.position = painter.map_to_local(position_on_grid) + tile_map.map_to_local(tile)
		area.connect("behavior_body_entered", painter._on_behavior_area_body_entered)
		area.call_deferred("add_child", collision_shape)

		painter.behaviors_node.call_deferred_thread_group("add_child", area)


func draw_terrain() -> void:
	# Outside for performance
	var tile : Tile = null
	for x in range(width):
		for y in range(height):
			# Check tile needs to be drawn
			tile = terrain.grid_system.get_cell_safe(x, y)
			if tile == null or tile.is_empty():
				continue
			# Set the cell
			tile_map.set_cell(
				0, # Layer
				Vector2i(x, y), # Position
				0, # source_id Not used
				tile.atlas, # Atlas on TileSet 
				tile.alt # Alt on TileSet, 0 is default
			)
	
	# Add the tilemap
	call_deferred("add_child", tile_map)
	# Load spawners
	call_deferred("load_nodes")

func load_nodes() -> void:
	for spawner in spawners:
		var node = load(spawner.node).instantiate()
		painter.enemies_node.add_child(node)
		node.position = painter.map_to_local(position_on_grid) + tile_map.map_to_local(spawner.grid_position)
