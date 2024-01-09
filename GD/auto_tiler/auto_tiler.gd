## auto_tiler.gd
## This class is responsible for handling the "autotileRules" in the JSON file.
## Methods:
## - [decideTile] - Decides which tile to use based on the surrounding tiles.
class_name AutoTiler


var rules: Array[AutoTilerRule] = []
var random : RandomNumberGenerator = null


func _init(rules_: Array[AutoTilerRule], random_ : RandomNumberGenerator) -> void:
	rules = rules_
	random = random_


## Decide which tile to use based on the surrounding tiles.
## Returns the name of the tile to use.
##
## Args:
## - [tile]: The tile to decide for.
## - [neighbours]: The neighbours of the tile.
## - [gridSystem]: The grid system.
func decide_tile(tile: Tile, neighbours: DirectionMap, gridSystem: GridSystem) -> String:
	# Get the rule for the tile
	var rule: AutoTilerRule = _get_rule_for_tile(tile)
	if rule == null:
		return tile.name

	var is_met : bool = true

	# Loop through conditions of the rule
	for condition in rule.conditions:
		is_met = true
		# Check if the condition is met
		for direction in neighbours.keys():
			var value = neighbours.get(direction)
			if value == null:
				continue

			if not default_condition(
				rule, 
				condition.get_direction(direction), 
				value, 
				gridSystem, 
			):
				is_met = false
				break

		if is_met:
			if condition.result.contains(","):
				var results = condition.result.split(",")
				return results[(random.randi() + 1) % results.size()]
			return condition.result

	return tile.name


## Get the rule for the tile.
func _get_rule_for_tile(tile: Tile) -> AutoTilerRule:
	for rule in rules:
		if rule.tile_name == tile.name:
			return rule
	return null


func any_condition() -> bool:
	return true


func not_condition(
	rule: AutoTilerRule,
	condition_value : String,
	neighbor_value: Vector2i,
	gridSystem: GridSystem,
) -> bool:
	var tile_name = condition_value.split("!")[1]

	if tile_name == "Self":
		return not self_condition(rule, neighbor_value, gridSystem)
	
	return tile_name != gridSystem.get_cell(neighbor_value.x, neighbor_value.y).name


func self_condition(
	rule: AutoTilerRule,
	neighbor_value : Vector2i,
	gridSystem: GridSystem,
) -> bool:
	return rule.tile_name == gridSystem.get_cell(neighbor_value.x, neighbor_value.y).name


func and_condition(
	rule: AutoTilerRule,
	condition_value : String,
	neighbor_value: Vector2i,
	gridSystem: GridSystem,
) -> bool:
	var conditions = condition_value.split("&")
	for condition in conditions:
		if not default_condition(rule, condition, neighbor_value, gridSystem):
			return false
	return true


func or_condition(
	rule: AutoTilerRule,
	condition_value : String,
	neighbor_value: Vector2i,
	gridSystem: GridSystem,
) -> bool:
	var conditions = condition_value.split("|")
	for condition in conditions:
		if default_condition(rule, condition, neighbor_value, gridSystem):
			return true
	return false


func has_condition(
	_rule: AutoTilerRule,
	condition_value : String,
	neighbor_value: Vector2i,
	gridSystem: GridSystem,
) -> bool:
	var tile_name = condition_value.split("Has(")[1].split(")")[0]
	return tile_name in gridSystem.get_cell(neighbor_value.x, neighbor_value.y).name


func default_condition(
	rule: AutoTilerRule,
	condition_value: String,
	neighbor_value: Vector2i,
	gridSystem: GridSystem,
):
	if gridSystem.get_cell_safe(neighbor_value.x, neighbor_value.y) == null:
		return false

	if "&" in condition_value:
		return and_condition(rule, condition_value, neighbor_value, gridSystem)
	
	if "|" in condition_value:
		return or_condition(rule, condition_value, neighbor_value, gridSystem)
	
	if "!" in condition_value:
		return not_condition(rule, condition_value, neighbor_value, gridSystem)
	
	if condition_value == "Any":
		return any_condition()
	
	if condition_value == "Self":
		return self_condition(rule, neighbor_value, gridSystem)

	if "Has(" in condition_value:
		return has_condition(rule, condition_value, neighbor_value, gridSystem)

	# If the conditions direction is met then the condition is valid.
	return condition_value == gridSystem.get_cell(neighbor_value.x, neighbor_value.y).name