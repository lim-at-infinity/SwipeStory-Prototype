using System.Collections.Generic;
using UnityEngine;

// Adventure definition: a series of fights, fought in order. Read-only at runtime.
// Rewards are given once for clearing the whole adventure; these are base values, BattleResult applies multipliers
[CreateAssetMenu(menuName = "SwipeStory/Adventure", fileName = "NewAdventure")]
public class AdventureData : ScriptableObject
{
    [SerializeField] private List<EncounterData> _encounters = new List<EncounterData>();
    [SerializeField] private List<ItemData> _itemRewards = new List<ItemData>();

    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField, Range(1, 5)] public int Difficulty { get; private set; } = 1;

    [field: SerializeField, Min(0)] public int GoldReward { get; private set; }
    [field: SerializeField, Min(0)] public int XpReward { get; private set; }
    [field: SerializeField, Min(0)] public int UndoTokenReward { get; private set; }

    public IReadOnlyList<EncounterData> Encounters => _encounters;
    public IReadOnlyList<ItemData> ItemRewards => _itemRewards;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Debug.LogWarning("[AdventureData] " + name + " has no Id.", this);
        }

        if (_encounters.Count == 0)
        {
            Debug.LogWarning("[AdventureData] " + name + " has no encounters.", this);
        }

        for (int i = 0; i < _encounters.Count; i++)
        {
            if (_encounters[i] == null || _encounters[i].Enemies.Count == 0)
            {
                Debug.LogWarning("[AdventureData] " + name + ": encounter " + (i + 1) + " has no enemies.", this);
            }
        }
    }
}
