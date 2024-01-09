using System;

namespace FastTerrain;

public class DirectionMap<T>
{
    public T N;
    public T E;
    public T S;
    public T W;

    public DirectionMap(T n, T e, T s, T w)
    {
        N = n;
        E = e;
        S = s;
        W = w;
    }

    // create a [] by which loading from string.
    // i.e. DirscrionMap['N'] should return N

    public T this[char c]
    {
        get
        {
            switch (c)
            {
                case 'N':
                    return N;
                case 'E':
                    return E;
                case 'S':
                    return S;
                case 'W':
                    return W;
                default:
                    throw new ArgumentException("Invalid direction");
            }
        }
    }

    public char[] Keys()
    {
        return new char[] { 'N', 'E', 'S', 'W' };
    }

    public T[] Values()
    {
        return new T[] { N, E, S, W };
    }
}