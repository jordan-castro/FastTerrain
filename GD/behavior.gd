## behavior.gd
## This handles our behavior tree
class_name Behavior

var tile : String = ""
var behavior : String = ""


func _init(tile_: String, behavior_: String)->void:
	tile = tile_
	behavior = behavior_


static func from_json(json: Dictionary)->Behavior:
	var tile_ = json["tile"]
	var behavior_ = json["behavior"]
	return Behavior.new(tile_, behavior_)
