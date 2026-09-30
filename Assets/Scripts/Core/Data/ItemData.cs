using UnityEngine;

// Item definition. Read-only at runtime: inventories and heroes reference this asset, never copy or change it
[CreateAssetMenu(menuName = "SwipeStory/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public ItemType Type { get; private set; }
    [field: SerializeField, Range(1, 3)] public int Stars { get; private set; } = 1;
    [field: SerializeField, Min(0)] public int Price { get; private set; }

    // Weapon: Attack. Hat: Defense. Potion: HP restored
    [field: SerializeField, Min(0)] public int StatBonus { get; private set; }

    // Gift only
    [field: SerializeField, Min(0)] public int AffinityBonus { get; private set; }

    public EquipSlot Slot => ItemTypeRules.GetSlot(Type);

    public bool CanBeEquippedBy(HeroClass heroClass)
    {
        return ItemTypeRules.CanEquip(Type, heroClass);
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            Debug.LogWarning("[ItemData] " + name + " has no Id.", this);
        }
    }
}
