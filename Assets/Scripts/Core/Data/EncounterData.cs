using System;
using System.Collections.Generic;
using UnityEngine;

// One fight inside an adventure, authored inside AdventureData
[Serializable]
public class EncounterData
{
    // Declared first on purpose: Unity labels list elements with their first field when it's a string
    [field: SerializeField] public string Name { get; private set; }

    [SerializeField] private List<EnemyData> _enemies = new List<EnemyData>();

    public IReadOnlyList<EnemyData> Enemies => _enemies;
}
