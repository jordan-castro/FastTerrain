using Godot;

namespace FastTerrain;

public partial class ChunkGodot : GodotObject {
    public Chunk chunk = null;

    public ChunkGodot(Chunk chunk) {
        this.chunk = chunk;
    }
}