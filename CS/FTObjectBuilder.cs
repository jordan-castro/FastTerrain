using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FastTerrain;

public class FTObjectBuilder
{
    public string Name = "FTObjectBuilder";
    public Dictionary<string, object> Args = null;
    private Random Random = null;

    public List<Tile> Tiles = new();

    public FTObjectBuilder(JObject args, List<Tile> tiles, Random random)
    {
        Args = args.ToObject<Dictionary<string, object>>();
        Name = (string)args["name"];
        Random = random;

        Tiles = tiles;
    }

    public GridSystem Build(GridSystem gridSystem, Chunk chunk)
    {
        // Checking if the object is within the namespace FTObjects
        Type type = Type.GetType("FTObjects." + Name);
        if (type != null) {
            object instance = Activator.CreateInstance(type);  
            MethodInfo methodInfo = type.GetMethod("Build");
            return (GridSystem) methodInfo.Invoke(instance, new object[] {
                gridSystem,
                Args,
                Random,
                chunk,
                Tiles
            });
        }

        return null;
    }
}