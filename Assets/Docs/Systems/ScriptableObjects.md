# ScriptableObjects

## What they are

A ScriptableObject (SO) is a script whose instances are **asset files** in the Project window, instead of components on GameObjects. We use them for anything designed in the Editor rather than created during play. `OldSword.asset` is one `ItemData`; every hero and inventory that "has" an Old Sword points at that same asset.

Unity's intro: https://docs.unity3d.com/Manual/class-ScriptableObject.html

## Our SOs

| Script | One asset per | Folder | Create menu |
| --- | --- | --- | --- |
| `ItemData` | Item (Old Sword, Straw Hat) | `Assets/Data/Items` | Create > SwipeStory > Item |
| `TraitData` | Trait (Charmer) | `Assets/Data/Traits` | Create > SwipeStory > Trait |
| `AdventureData` | Adventure (its encounters and enemies are edited inside it) | `Assets/Data/Adventures` | Create > SwipeStory > Adventure |
| `BalanceConfig` | Only one, holding every tuning number | `Assets/Data/Config` | Create > SwipeStory > Balance Config |

## Who uses them

| Script | Uses |
| --- | --- |
| `GameManager` | Holds the BalanceConfig reference (`Config`); everything else reads it through `GameManager.Instance.Config` |
| `HeroFactory`, `HeroGenerator`, `RosterManager`, `RelationshipSystem` | Read BalanceConfig values |
| `HeroData` | `Weapon` and `Hat` reference ItemData; `Traits` references TraitData |
| `Inventory` | Holds ItemData references |
| `RelationshipSystem` | Reads each hero's TraitData (dating cap) |
| `BalanceConfig` | Lists AdventureData (in unlock order), ItemData (shop) and TraitData (trait pool) |
| `AdventureContext`, `BattleResult`, `AdventureResult` | Reference AdventureData and ItemData |
| `DebugSeed` | A list of ItemData you drag in |

## Rules

- **Create them in the Editor** with the Create menu above. Never copy or hand-edit `.asset` files, and commit the `.meta` file with each asset.
- **Never change them from code at runtime.** In the Editor, changes made during Play are saved into the asset file and stay after you stop. Anything that changes during a run (HP, level, affinity) lives in plain classes like `HeroData`. That's why their properties are read-only from code.
- **Reference them, don't copy them.** `hero.Weapon`, `Inventory.Items` and `Config.ShopItems` all point at assets. `==` between two items checks whether they're the same asset.
- **Hook them up by dragging** in the Inspector: `BalanceConfig.asset` into GameManager's Config field; the adventures **in order** into BalanceConfig's Adventures list (the last is the boss); items into the Shop list or an adventure's Item Rewards.
- **Saved values win over script defaults.** Changing a default in `BalanceConfig.cs` doesn't change the existing asset; edit the asset.
- **Give each item, trait and adventure an `Id`** (e.g. `old_sword`). It's the stable name for code and future save files; the Inspector warns if it's empty.
- Inspector edits made during Play mode stay after you stop. Handy for tuning BalanceConfig, easy to forget.
