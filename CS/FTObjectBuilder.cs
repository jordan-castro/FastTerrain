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

    private void LoadTileData(List<Tile> tiles)
    {
        if (Args["loadFor"] == null) {
            return;
        }

        List<string> valuesToChange = new();
        List<Tile> tileData = new();

        foreach (var key in (string[])Args["loadFor"]) {
            if (Args[key] == null) {
                continue;
            }

            List<string> namesOfTiles = new();
            if (Args[key].GetType() == typeof(string)) {
                namesOfTiles.Add((string)Args[key]);
            } else if (Args[key].GetType() == typeof(string[])) {
                namesOfTiles.AddRange((string[])Args[key]);
            }

            foreach (var name in namesOfTiles) {
                foreach (var tile in tiles) {
                    if (tile.Name == name) {
                        tileData.Add(tile);
                        break;
                    }
                }
            }

            if (Args[key].GetType() == typeof(string)) {
                Args[key] = tileData[0];
            } else if (Args[key].GetType() == typeof(string[])) {
                Args[key] = tileData.ToArray();
            }
        }
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