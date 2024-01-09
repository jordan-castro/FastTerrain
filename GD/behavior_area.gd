class_name BehaviorArea extends Area2D


var behavior_name : String = "default"

# TODO: Allow passing of collision values.

func _init(name_: String)->void:
	behavior_name = name_


signal behavior_body_entered(body: Node, behavior_area : BehaviorArea)


func _ready():
	connect("body_entered", _on_body_entered)


func _on_body_entered(body: Node):
	emit_signal("behavior_body_entered", body, self)