using System;
using System.Collections.Generic;
using UnityEngine;

// Holds references to ItemData assets. The same asset can appear more than once (two Old Swords)
[DefaultExecutionOrder(-200)]
public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public event Action OnInventoryChanged;

    private readonly List<ItemData> _items = new List<ItemData>();

    public IReadOnlyList<ItemData> Items => _items;

    private void Awake()
    {
        Instance = this;
    }

    public void Add(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        _items.Add(item);
        OnInventoryChanged?.Invoke();
    }

    public bool Remove(ItemData item)
    {
        if (!_items.Remove(item))
        {
            return false;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    // Weapon / Hat: equips if the class allows it; the old item goes back to the inventory.
    // Potion: restores HP. Gift: adds affinity (NPC heroes only). False if nothing happened
    public bool UseOn(ItemData item, HeroData hero)
    {
        if (item == null || hero == null || !_items.Contains(item))
        {
            return false;
        }

        switch (item.Type)
        {
            case ItemType.Potion:
                hero.CurrentHp = Mathf.Min(hero.MaxHp, hero.CurrentHp + item.StatBonus);
                break;

            case ItemType.Gift:
                if (hero.IsPlayer)
                {
                    return false;
                }

                RelationshipSystem.Instance.AddAffinity(hero, item.AffinityBonus);
                break;

            default:
                if (!item.CanBeEquippedBy(hero.Class))
                {
                    return false;
                }

                ItemData previous = hero.GetEquipped(item.Slot);
                hero.SetEquipped(item.Slot, item);
                if (previous != null)
                {
                    _items.Add(previous);
                }
                break;
        }

        _items.Remove(item);
        OnInventoryChanged?.Invoke();
        RosterManager.Instance.NotifyHeroUpdated(hero);
        return true;
    }

    // Only GameManager.NewGame should call this
    internal void Clear()
    {
        _items.Clear();
        OnInventoryChanged?.Invoke();
    }
}
