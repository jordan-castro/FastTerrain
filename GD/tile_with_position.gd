class_name TileWithPosition

var tile : Tile = null
var grid_position : Vector2i = Vector2i.ZERO


func _init(_tile: Tile, _grid_position: Vector2i):
	tile = _tile
	grid_position = _grid_position


## Create a new TileWithPosition from a Tile and a Vector2i.
## If either argument is null, replace it with the current value.
func new_from(_tile, _grid_position) -> TileWithPosition:
	var new_tile : Tile = tile
	var new_grid_position : Vector2i = grid_position

	if _tile != null:
		new_tile = _tile

	if _grid_position != null:
		new_grid_position = _grid_position

	return TileWithPosition.new(new_tile, new_grid_position)