using Godot;

namespace FastTerrain;

public partial class GodotChunk : GodotObject {
    public Chunk chunk = null;

    public GodotChunk(Chunk chunk) {
        this.chunk = chunk;
    }
}