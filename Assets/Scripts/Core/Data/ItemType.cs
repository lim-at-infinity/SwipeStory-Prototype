// Saved as ints in assets: never reorder or renumber, only append
public enum ItemType
{
    Hat = 0,
    Sword = 1,
    Dagger = 2,
    Staff = 3,
    Cross = 4,
    Potion = 5,
    Gift = 6
}

public enum EquipSlot
{
    None = 0,
    Weapon = 1,
    Hat = 2
}

// Which slot an item type goes in, and which class may equip it
public static class ItemTypeRules
{
    public static EquipSlot GetSlot(ItemType type)
    {
        switch (type)
        {
            case ItemType.Hat:
                return EquipSlot.Hat;
            case ItemType.Sword:
            case ItemType.Dagger:
            case ItemType.Staff:
            case ItemType.Cross:
                return EquipSlot.Weapon;
            default:
                return EquipSlot.None;
        }
    }

    // False when any class can use the type (Hat) or it isn't equipment
    public static bool TryGetRequiredClass(ItemType type, out HeroClass heroClass)
    {
        switch (type)
        {
            case ItemType.Sword:
                heroClass = HeroClass.Warrior;
                return true;
            case ItemType.Dagger:
                heroClass = HeroClass.Rogue;
                return true;
            case ItemType.Staff:
                heroClass = HeroClass.Mage;
                return true;
            case ItemType.Cross:
                heroClass = HeroClass.Healer;
                return true;
            default:
                heroClass = default;
                return false;
        }
    }

    public static bool CanEquip(ItemType type, HeroClass heroClass)
    {
        if (GetSlot(type) == EquipSlot.None)
        {
            return false;
        }

        return !TryGetRequiredClass(type, out HeroClass required) || required == heroClass;
    }
}
