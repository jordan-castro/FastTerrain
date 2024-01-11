## grid.gd
class_name GridSystem


var grid: Array = []


func _init(size: Vector2i, fill: Tile):
	for x in range(size.x):
		grid.append([])
		for y in range(size.y):
			grid[x].append(fill)


## Get a cell, not safe.
func get_cell(x:int,y:int) -> Tile:
	return grid[x][y]


## Set a cell, not safe.
func set_cell(x:int,y:int,tile:Tile)->void:
	grid[x][y] = tile


## Get a cell, safe.
func get_cell_safe(x:int,y:int) -> Tile:
	if x < 0 or x >= grid.size() or y < 0 or y >= grid[0].size():
		return null

	return grid[x][y]


## Set a cell, safe.
func set_cell_safe(x:int,y:int,tile:Tile)->void:
	if x < 0 or x >= grid.size() or y < 0 or y >= grid[0].size():
		return

	grid[x][y] = tile


func get_neighbors(x:int, y:int)->DirectionMap:
	var neighbors:DirectionMap = DirectionMap.EMPTY()

	# Top
	if y > 0:
		neighbors.N = Vector2i(x, y-1)
	# Right
	if x < grid.size()-1:
		neighbors.E = Vector2i(x+1, y)
	# Bottom
	if y < grid[0].size()-1:
		neighbors.S = Vector2i(x, y+1)
	# Left
	if x > 0:
		neighbors.W = Vector2i(x-1, y)
	
	return neighbors


## Returns all cell positions of said type.
## [args]:
## type - [Array[String]]
func get_cells_by_type(type:Array, grid_box:Array[TileWithPosition])->Array[Vector2i]:
	var cells: Array[Vector2i] = []

	for tile in grid_box:
		if tile.tile.name in type:
			cells.append(tile.grid_position)

	return cells


func set_cell_if_empty(x:int, y:int, tile:Tile)->void:
	var cell = get_cell_safe(x, y)
	if not cell or not cell.is_empty():
		return
		
	set_cell(x, y, tile)


## Get a box of tiles. 
## If the box is out of bounds, it will crash.
## Returns an Array[TileWithPosition].
func box(position: Vector2i, width: int, height: int) -> Array[TileWithPosition] :
	var tiles: Array[TileWithPosition] = []

	for x in range(position.x, position.x + width):
		for y in range(position.y, position.y + height):
			tiles.append(
				TileWithPosition.new(
					get_cell(x, y),
					Vector2i(x, y)
				)
			)

	return tiles


## Get a box of tiles safely.
func box_safe(position: Vector2i, width: int, height: int)->Array[TileWithPosition]:
	var tiles: Array[TileWithPosition] = []

	var tile: Tile = null
	for x in range(position.x, position.x + width):
		for y in range(position.y, position.y + height):
			tile = get_cell_safe(x, y)
			if tile != null:
				tiles.append(
					TileWithPosition.new(
						tile,
						Vector2i(x, y)
					)
				)

	return tiles


## This returns a Array[Tile]
static func filter_tiles(tiles:Array[Tile], type:Array)->Array[Tile]:
	var filtered_tiles:Array[Tile] = []

	for tile in tiles:
		if tile.name in type:
			filtered_tiles.append(tile)

	return filtered_tiles


## This returns a Array[TileWithPosition]
static func filter_tiles_as_array(tiles:Array[TileWithPosition], type:Array)->Array[TileWithPosition]:
	var filtered_tiles:Array[TileWithPosition] = []

	for tile in tiles:
		if tile.tile.name in type:
			filtered_tiles.append(tile)

	return filtered_tiles