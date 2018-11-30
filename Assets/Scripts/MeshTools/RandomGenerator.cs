using System;

public static class RandomGenerator
{
    private static System.Random GlobalSeedGenerator = new System.Random();
    [ThreadStatic]
    private static System.Random LocalThreadRandomInstance;

    public static int Next()
    {
        System.Random instance = LocalThreadRandomInstance;
        if (instance == null)
        {
            int newSeed;
            lock (GlobalSeedGenerator) newSeed = GlobalSeedGenerator.Next();
            LocalThreadRandomInstance = instance = new System.Random(newSeed);
        }
        return instance.Next();
    }
    public static double NextDouble()
    {
        System.Random instance = LocalThreadRandomInstance;
        if (instance == null)
        {
            int newSeed;
            lock (GlobalSeedGenerator) newSeed = GlobalSeedGenerator.Next();
            LocalThreadRandomInstance = instance = new System.Random(newSeed);
        }
        return instance.NextDouble();
    }
    public static float NextFloat()
    {
        System.Random instance = LocalThreadRandomInstance;
        if (instance == null)
        {
            int newSeed;
            lock (GlobalSeedGenerator) newSeed = GlobalSeedGenerator.Next();
            LocalThreadRandomInstance = instance = new System.Random(newSeed);
        }
        return (float)instance.NextDouble();
    }
}


