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

## Script reference

### ItemType.cs

Enums `ItemType` (Hat, Sword, Dagger, Staff, Cross, Potion, Gift) and `EquipSlot` (None, Weapon, Hat), plus the static helper `ItemTypeRules`:

| Member | What it does |
| --- | --- |
| `GetSlot(type)` | Which slot a type goes in: Hat to Hat, the four weapon types to Weapon, Potion and Gift to None. |
| `TryGetRequiredClass(type, out heroClass)` | Returns true and the class if the type is class locked (Sword to Warrior, Dagger to Rogue, Staff to Mage, Cross to Healer). Returns false for Hat and non-equipment. |
| `CanEquip(type, heroClass)` | True if the type is equipment and that class is allowed to use it. |

### ItemData.cs (ScriptableObject)

| Member | What it does |
| --- | --- |
| `Id`, `DisplayName`, `Description`, `Icon` | Identity and display. `Id` is a stable code name like `old_sword`; the Inspector warns if it's empty. |
| `Type`, `Stars`, `Price` | Category, 1 to 3 star rating, shop price in gold. |
| `StatBonus` | Weapon: adds Attack. Hat: adds Defense. Potion: HP restored. |
| `AffinityBonus` | Affinity a Gift adds with the player. |
| `Slot` | The slot this item goes in, worked out from its `Type`. |
| `CanBeEquippedBy(heroClass)` | Whether a hero of that class can equip it (class locks). |

### Inventory.cs (manager)

| Member | What it does |
| --- | --- |
| `Items` | Item assets the player owns. The same asset can appear several times. |
| `Add(item)` | Adds an item and fires `OnInventoryChanged`. Null is ignored. |
| `Remove(item)` | Removes one copy; returns false if it wasn't there. |
| `UseOn(item, hero)` | Equips, drinks or gives the item (see the table above). Returns false and changes nothing if it can't be used on that hero. |
| `Clear()` (internal) | Empties the inventory. Only `GameManager.NewGame` calls it. |
| Events | `OnInventoryChanged`. |
