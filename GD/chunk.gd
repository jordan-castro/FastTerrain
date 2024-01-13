## The `Chunk` class represents a chunk of the terrain in the game.
## Each chunk has a position on the grid, a width, a height, and a loaded state.
class_name Chunk extends Node2D

## A boolean indicating whether the chunk is loaded or not.
var is_loaded: bool = false
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
## Our behaviors.
var behaviors_node : Node = Node.new()
## Our Enemies
var enemies_node : Node = Node.new()
## Our loadbody area
var load_body_area2d : Area2D = Area2D.new()
## our unloadbody arae
var unload_body_area2d : Area2D = Area2D.new()

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
	# Set our behavior and enemies node
	behaviors_node.name = "Behaviors"
	enemies_node.name = "Enemies"
	# Enemies node gets a script attached.
	enemies_node.set_script(preload("res://scripts/utils/Enemies.gd"))

	# Add our nodes
	add_child(behaviors_node)
	add_child(enemies_node)
	add_child(tile_map)

	# -- Load and unload bodies --
	# Layers (is not player)
	load_body_area2d.set_collision_layer_value(1, false)
	unload_body_area2d.set_collision_layer_value(1, false)
	# Layers (Can technically be considered behaviors?)
	load_body_area2d.set_collision_layer_value(4, true)
	unload_body_area2d.set_collision_layer_value(4, true)
	# Masks (A special player mask)
	load_body_area2d.set_collision_mask_value(6, true)
	unload_body_area2d.set_collision_mask_value(6, true)
	# Shape
	var load_collision_shape = CollisionShape2D.new()
	load_collision_shape.shape = RectangleShape2D.new()
	load_collision_shape.shape.size = Vector2(width * 75, height * 75)
	load_body_area2d.add_child(load_collision_shape)
	var unload_collision_shape = CollisionShape2D.new()
	unload_collision_shape.shape = RectangleShape2D.new()
	unload_collision_shape.shape.size = Vector2(width * 100, height * 100)
	unload_body_area2d.add_child(unload_collision_shape)
	# Connects
	load_body_area2d.connect("body_entered", self.load)
	unload_body_area2d.connect("body_exited", self.unload)

	add_child(load_body_area2d)
	add_child(unload_body_area2d)


func init(tile_set: TileSet, gb_position: Vector2, painter_: Painter):
	painter = painter_

	# Finish setting body areas (position)
	load_body_area2d.global_position = gb_position
	unload_body_area2d.global_position = gb_position

	# Set tilemap
	tile_map.tile_set = tile_set
	tile_map.name = "ChunkMap"
	tile_map.global_position = gb_position
	# Set a name for our chunk!
	name = "Chunk + " + str(position_on_grid) + " " + str(width) + "x" + str(height)


## Loads the chunk
## _body is irrevalant, is only there to accept calls from AreaBody2D
func load(_body) -> void:
	if is_loaded:
		return

	var should_wait = false
	if _body == null:
		should_wait = true

	var t = Thread.new()
	t.start(
		func():
			load_terrain()
			for b in painter.data_loader.behaviors:
				load_behavior(b)
			is_loaded = true
			call_deferred("draw_terrain")
			call_deferred("load_nodes")
	)
	if should_wait:
		t.wait_to_finish()


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

		area.position = tile_map.map_to_local(tile) + painter.map_to_local(position_on_grid)
		area.connect("behavior_body_entered", painter._on_behavior_area_body_entered)
		area.call_deferred("add_child", collision_shape)

		behaviors_node.call_deferred("add_child", area)


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
	

## Spawn in any nodes that need to be spawned in.
func load_nodes() -> void:
	for spawner in spawners:
		# Get the node
		var node = load(spawner.node).instantiate()
		# The position is relative to the chunk, so we need to add the position of the chunk to it.
		node.position = tile_map.map_to_local(spawner.grid_position) + painter.map_to_local(position_on_grid)
		# Add the node to the chunk.
		enemies_node.add_child(node)


func unload(_body) -> void:
	if not is_loaded:
		return
	# Clear all tiles
	tile_map.clear()
	# Clear nodes
	call_deferred("clear_node", enemies_node)
	call_deferred("clear_node", behaviors_node)
	# Set is_loaded to false.
	is_loaded = false


## Remove and queue Free all children of node.
func clear_node(node: Node) -> void:
	for child in node.get_children():
		node.remove_child(child)
		child.queue_free()


## A function that updates the tiles on the edge of the chunk, 
## in relation to the neighbor chunks.
func update_edge_tiles():
	pass