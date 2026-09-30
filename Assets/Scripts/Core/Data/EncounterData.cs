using System;
using System.Collections.Generic;
using UnityEngine;

// One fight inside an adventure, authored inside AdventureData
[Serializable]
public class EncounterData
{
    [SerializeField] private List<EnemyData> _enemies = new List<EnemyData>();

    // Also used as the element label in the Inspector list
    [field: SerializeField] public string Name { get; private set; }

    public IReadOnlyList<EnemyData> Enemies => _enemies;
}
