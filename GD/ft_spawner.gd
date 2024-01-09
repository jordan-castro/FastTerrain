## ft_spawner.gd
class_name FTSpawner


## The node object
var node : Node = null
## The position of the node on the grid
var grid_position : Vector2i = Vector2i.ZERO


func _init(node_: Node, grid_position_: Vector2i) -> void:
	node = node_
	grid_position = grid_position_