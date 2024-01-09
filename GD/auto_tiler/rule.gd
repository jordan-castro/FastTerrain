## The data which is used to decide which tile to use.
class_name AutoTilerRule


var tile_name: String
var conditions: Array[AutoTilerRuleCondition] = []


func _init(tile_name_:String, conditions_:Array[AutoTilerRuleCondition]):
	tile_name = tile_name_
	conditions = conditions_


static func from_json(json:Dictionary)->AutoTilerRule:
	var conditions_:Array[AutoTilerRuleCondition] = []
	for condition in json["conditions"]:
		conditions_.append(AutoTilerRuleCondition.from_json(condition))
	return AutoTilerRule.new(json["tile"], conditions_)
