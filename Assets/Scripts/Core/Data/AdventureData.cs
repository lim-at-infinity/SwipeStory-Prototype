using System.Collections.Generic;
using UnityEngine;

// Adventure definition. Read-only at runtime. Rewards here are base values; BattleResult applies multipliers
[CreateAssetMenu(menuName = "SwipeStory/Adventure", fileName = "NewAdventure")]
public class AdventureData : ScriptableObject
{
    [SerializeField] private List<EnemyData> _enemies = new List<EnemyData>();
    [SerializeField] private List<ItemData> _itemRewards = new List<ItemData>();

    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField, Range(1, 5)] public int Difficulty { get; private set; } = 1;

    [field: SerializeField, Min(0)] public int GoldReward { get; private set; }
    [field: SerializeField, Min(0)] public int XpReward { get; private set; }
    [field: SerializeField, Min(0)] public int UndoTokenReward { get; private set; }

    public IReadOnlyList<EnemyData> Enemies => _enemies;
    public IReadOnlyList<ItemData> ItemRewards => _itemRewards;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Debug.LogWarning("[AdventureData] " + name + " has no Id.", this);
        }

        if (_enemies.Count == 0)
        {
            Debug.LogWarning("[AdventureData] " + name + " has no enemies.", this);
        }
    }
}
