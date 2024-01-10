## Tile.gd
## Tile class used to represent a single tile in the FastTerrain system.
class_name Tile

## The name of the tile. This is used to identify the tile.
var name: String = ""
## The position of the tile in the atlas.
var atlas: Vector2i = Vector2i.ZERO
## Alternative tile id
var alt: int = 0

## Constructor
func _init(name_: String, atlas_: Vector2i, alt_: int=0) -> void:
	name = name_
	atlas = atlas_
	alt = alt_

## Creates a new tile from a JSON dictionary.
static func from_json(json: Dictionary) -> Tile:
	return Tile.new(
		json["name"],
		Vector2i(json["atlas"]["x"], json["atlas"]["y"]),
		json.get("alt", 0)
	)


func _eq(other: Object) -> bool:
	if other is Tile:
		return name == other.name
	return false


func _hash() -> int:
	return name.hash()


func is_empty() -> bool:
	return name == "Empty"


## Creates a new empty tile.
static func EMPTY() -> Tile:
	return Tile.new("Empty", Vector2i(-1, -1))
