## The conditions which must be met for a tile to be used.
class_name AutoTilerRuleCondition extends DirectionMap


var result = null


func _init(result_, N_:String, E_:String, S_:String, W_:String):
	super._init(N_, E_, S_, W_)
	result = result_


static func from_json(json:Dictionary)->AutoTilerRuleCondition:
	return AutoTilerRuleCondition.new(json["result"], json["N"], json["E"], json["S"], json["W"])
