class_name BufferLoader

## The auto tiler
var auto_tiler : AutoTiler = null
## All chunks, as nodes
var chunks : Node = Node.new()


func _init(auto_tiler_ : AutoTiler, chunks_ : Node):
    auto_tiler = auto_tiler_
    # Keep a copy of the chunks off the main thread
    chunks = chunks_.duplicate()