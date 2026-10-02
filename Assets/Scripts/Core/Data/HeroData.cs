using System;
using System.Collections.Generic;
using UnityEngine;

// Runtime state for one hero. A plain class, not an asset: it changes all run.
// Base stats never include equipment or tier bonuses; battle adds those.
// Affinity and relationship status live in RelationshipSystem (one record per pair of heroes), not here.
// After changing any field, call RosterManager.NotifyHeroUpdated.
[Serializable]
public class HeroData
{
    [SerializeField] private List<TraitData> _traits = new List<TraitData>();

    [field: SerializeField] public string Id { get; private set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public HeroClass Class { get; set; }
    [field: SerializeField] public bool IsPlayer { get; set; }
    // Rolled once by HeroFactory so a hero looks the same on every screen. White until then (default Color is invisible)
    [field: SerializeField] public Color Color { get; set; } = Color.white;

    [field: SerializeField] public int Level { get; set; } = 1;
    [field: SerializeField] public int Xp { get; set; }

    [field: SerializeField] public int MaxHp { get; set; }
    [field: SerializeField] public int CurrentHp { get; set; }
    [field: SerializeField] public int Attack { get; set; }
    [field: SerializeField] public int Defense { get; set; }
    [field: SerializeField] public int Speed { get; set; }

    [field: SerializeField] public ItemData Weapon { get; set; }
    [field: SerializeField] public ItemData Hat { get; set; }

    public IReadOnlyList<TraitData> Traits => _traits;

    public HeroData()
    {
        Id = Guid.NewGuid().ToString();
    }

    public HeroData(string name, HeroClass heroClass, bool isPlayer = false) : this()
    {
        Name = name;
        Class = heroClass;
        IsPlayer = isPlayer;
    }

    // Surviving heroes return to full HP when the party gets back from an adventure (Rewards calls this).
    // Whether HP also resets between encounters inside one adventure is the battle side's call
    public void RestoreFullHp()
    {
        CurrentHp = MaxHp;
    }

    public ItemData GetEquipped(EquipSlot slot)
    {
        switch (slot)
        {
            case EquipSlot.Weapon:
                return Weapon;
            case EquipSlot.Hat:
                return Hat;
            default:
                return null;
        }
    }

    // Only checks the slot. Class locks are checked by Inventory.UseOn
    public void SetEquipped(EquipSlot slot, ItemData item)
    {
        if (item != null && item.Slot != slot)
        {
            throw new ArgumentException(item.name + " does not fit the " + slot + " slot.");
        }

        switch (slot)
        {
            case EquipSlot.Weapon:
                Weapon = item;
                break;
            case EquipSlot.Hat:
                Hat = item;
                break;
            default:
                throw new ArgumentException("Cannot equip into slot " + slot + ".");
        }
    }

    public void AddTrait(TraitData trait)
    {
        if (trait != null)
        {
            _traits.Add(trait);
        }
    }

    public bool HasTrait(TraitData trait)
    {
        return _traits.Contains(trait);
    }
}
