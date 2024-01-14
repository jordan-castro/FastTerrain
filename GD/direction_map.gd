## direction_map.gd
## A fast way to store and retrieve values for each direction.
class_name DirectionMap


var N = null
var E = null
var S = null
var W = null


static func EMPTY()->DirectionMap:
	return DirectionMap.new(null, null, null, null)


func _init(N_, E_, S_, W_) -> void:
	N = N_
	E = E_
	S = S_
	W = W_


## Returns a new DirectionMap from a JSON object
static func from_json(json) -> DirectionMap:
	return DirectionMap.new(
		json["N"] if json.has("N") else null,
		json["E"] if json.has("E") else null,
		json["S"] if json.has("S") else null,
		json["W"] if json.has("W") else null,
	)


## Returns the value for the given direction
func get_direction(direction: String):
	var value = null

	if direction == "N":
		value = self.N
	elif direction == "E":
		value = self.E
	elif direction == "S":
		value = self.S
	elif direction == "W":
		value = self.W

	return value


## Returns the keys 
func keys():
	return ["N", "E", "S", "W"]


## Returns the values
func values():
	return [self.N, self.E, self.S, self.W]


func eq(other) -> bool:
	return self.N == other.N and self.E == other.E and self.S == other.S and self.W == other.W