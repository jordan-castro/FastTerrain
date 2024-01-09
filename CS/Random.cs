using System.Collections;

namespace FastTerrain;

public class Random {
    public System.Random random = null;
    private readonly int seed = 0;

    public Random(int seed) {
        this.seed = seed;
        random = new System.Random(seed);
    }

    public object Choose(IList args) {
        return args[random.Next(0, args.Count)];
    }

    public int Range(int min, int max) {
        return random.Next(min, max);
    }

    public double NextDouble() {
        return random.NextDouble();
    }

    public int Seed {
        get {
            return seed;
        }
    }
}