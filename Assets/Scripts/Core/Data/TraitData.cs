using UnityEngine;

// Trait definition drawn on level-up. Read-only at runtime.
// Effects are data only for now; RelationshipSystem applies them later
[CreateAssetMenu(menuName = "SwipeStory/Trait", fileName = "NewTrait")]
public class TraitData : ScriptableObject
{
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }

    // 1 = normal. Charmer: 2
    [field: SerializeField, Min(0f)] public float AffinityGainMultiplier { get; private set; } = 1f;

    // 0 = use BalanceConfig.DefaultDatingCap. Multi-dating trait: 3
    [field: SerializeField, Min(0)] public int DatingCapOverride { get; private set; }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Debug.LogWarning("[TraitData] " + name + " has no Id.", this);
        }
    }
}
