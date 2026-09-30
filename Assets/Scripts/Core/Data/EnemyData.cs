using System;
using UnityEngine;

// One enemy in an adventure, authored inside AdventureData. Battle makes its own copy to track HP
[Serializable]
public class EnemyData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField, Min(1)] public int MaxHp { get; private set; } = 10;
    [field: SerializeField, Min(0)] public int Attack { get; private set; }
    [field: SerializeField, Min(0)] public int Defense { get; private set; }
    [field: SerializeField, Min(0)] public int Speed { get; private set; }

    public EnemyData()
    {
    }

    public EnemyData(string name, int maxHp, int attack, int defense, int speed)
    {
        Name = name;
        MaxHp = maxHp;
        Attack = attack;
        Defense = defense;
        Speed = speed;
    }
}
