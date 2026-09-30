using System;
using UnityEngine;

// Inclusive min/max pair for tuning values that are a range, e.g. "MaxHp 30 to 36"
[Serializable]
public struct IntRange
{
    [field: SerializeField] public int Min { get; private set; }
    [field: SerializeField] public int Max { get; private set; }

    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public bool IsValid => Min <= Max;

    // Both ends included (System.Random.Next excludes its upper bound, hence the + 1)
    public int Roll(System.Random rng)
    {
        return rng.Next(Min, Max + 1);
    }

    public int Clamp(int value)
    {
        return Mathf.Clamp(value, Min, Max);
    }

    public override string ToString()
    {
        return Min + " to " + Max;
    }
}
