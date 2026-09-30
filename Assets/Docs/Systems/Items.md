# Items

## ItemData (asset)

Each item is one `.asset` in `Assets/Data/Items/` (Create > SwipeStory > Item), e.g. `OldSword.asset`. Fields: `Id`, `DisplayName`, `Description`, `Icon`, `Type`, `Stars` (1 to 3), `Price`, `StatBonus`, `AffinityBonus`.

- `Type` is the category (Sword); the asset is a specific item in it (Old Sword).
- `StatBonus` means: Weapon adds Attack, Hat adds Defense, Potion restores HP.
- `AffinityBonus` is for Gifts.
- Items are read-only at runtime. Ten Old Swords are ten references to the same asset.

## Types, slots and class locks

`ItemTypeRules` decides where an item goes and who can use it:

| Type | Slot | Who can equip |
| --- | --- | --- |
| Hat | Hat | Anyone |
| Sword / Dagger / Staff / Cross | Weapon | Warrior / Rogue / Mage / Healer |
| Potion, Gift | None | Used, not equipped |

`item.Slot` and `item.CanBeEquippedBy(heroClass)` read these rules.

## Inventory

A list of item asset references (duplicates allowed).

`Inventory.UseOn(item, hero)` does one of:

| Item | Effect | Fails (returns false) when |
| --- | --- | --- |
| Weapon / Hat | Equips it; the previously equipped item goes back to the inventory | The hero's class can't use it |
| Potion | Heals `StatBonus` HP, up to `MaxHp` | |
| Gift | Adds `AffinityBonus` affinity with the player (`RelationshipSystem`) | Given to the player hero |

On success the item is removed from the inventory, and `OnInventoryChanged` plus `OnHeroUpdated` fire.

## Where items come from (prototype)

Hand-made placeholder assets only: `BalanceConfig.ShopItems` (Shop) and `AdventureData.ItemRewards` (rewards). Random loot comes after the prototype.
