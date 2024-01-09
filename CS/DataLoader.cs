using Godot;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace FastTerrain;

public class DataLoader
{
    public List<Tile> Tiles = new();
    public Random Random = null;
    public Terrain Terrain = null;
    public string TextureImage = null;
    public AutoTiler AutoTiler = null;
    public List<Tile> NoiseTiles = new List<Tile>();
    public List<Behavior> Behaviors = new List<Behavior>();
    public List<FTObjectBuilder> ChunkObjects = new();
    public List<FTObjectBuilder> WorldObjects = new();
    public List<Chunk> Chunks = new();
    private JObject data = null;

    private static JObject LoadJson(string filePath)
    {
        FileAccess fileAccess = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        JObject obj = JObject.Parse(fileAccess.GetAsText());
        return obj;
    }

    public DataLoader(string filePath, int worldSeed)
    {
        JObject data = LoadJson(filePath);
        // Initiate random
        Random = new Random(worldSeed);
        // texture_iamge
        TextureImage = (string)data["tileSet"];
        // Tiles
        foreach (var tile in data["tiles"])
        {
            int alt = 0;
            if (((JObject)tile).ContainsKey("alt"))
            {
                alt = (int)tile["alt"];
            }
            Tiles.Add(
                            new Tile(
                                (string)tile["name"],
                                new Godot.Vector2I(
                                    (int)tile["atlas"]["x"],
                                    (int)tile["atlas"]["y"]
                                ),
                                // Sometimes "alt" is not defined. 
                                // when not defined it should default to 0
                                alt
                            )
                        );
        }
        // Do the same but for noise tiles
        foreach (var tile in data["noiseTiles"])
        {
            foreach (var t in Tiles)
            {
                if (t.Name == (string)tile)
                {
                    NoiseTiles.Add(t);
                }
            }
        }
        // Terrain
        Terrain = new Terrain(
            new Vector2I(
                Random.Range(
                    (int)data["terrainSize"]["width"]["min"],
                    (int)data["terrainSize"]["width"]["max"]
                ),
                Random.Range(
                    (int)data["terrainSize"]["height"]["min"],
                    (int)data["terrainSize"]["height"]["max"]
                )
            )
        );
        int chunkWidth = (int)data["chunk"]["width"];
        int chunkHeight = (int)data["chunk"]["height"];
        // Setup Chunks
        for (int x = 0; x < Terrain.size.X; x += chunkWidth)
        {
            for (int y = 0; y < Terrain.size.Y; y += chunkHeight)
            {
                Chunks.Add(
                                    new Chunk(
                                        new Vector2I(x, y),
                                        chunkWidth,
                                        chunkHeight
                                    )
                                );
            }
        }
        List<AutoTilerRule> rules = new();
        // Setup the AutoTiler
        foreach (JObject rule in data["autotileRules"].Cast<JObject>())
        {
            rules.Add(
                            AutoTilerRule.FromJson(
                                rule
                            )
                        );
        }
        AutoTiler = new AutoTiler(rules, worldSeed);

        // Load chunk objects
        foreach (JObject obj in data["objects"]["chunk"].Cast<JObject>())
        {
            ChunkObjects.Add(
                            new FTObjectBuilder(obj, Tiles, Random)
                        );
        }
        // Load behavior 
        foreach (var behavior in data["behaviors"])
        {
            Behaviors.Add(
                            new Behavior(
                                (string)behavior["tile"],
                                (string)behavior["behavior"]
                            )
                        );
        }
    }
}