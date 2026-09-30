# Systems Overview

How the Core-Data pieces fit together. For exact signatures see `DataCheatSheet.md`; for how we work see `DevPractices.md`.

| Doc | Covers |
| --- | --- |
| `Heroes.md` | HeroData, HeroFactory, HeroGenerator, RosterManager (living and fallen) |
| `Items.md` | ItemData, item types and class locks, Inventory |
| `Relationships.md` | Affinity, tiers, dating, breakups, widows |
| `Adventures.md` | Adventures, encounters, results, unlocks, how a run ends |
| `ConfigAndTesting.md` | BalanceConfig, DebugSeed, EditMode tests |

## Two kinds of data

- **Definitions** are ScriptableObject assets made in the Editor (Create > SwipeStory > ...): `ItemData`, `TraitData`, `AdventureData`, `BalanceConfig`. Read-only at runtime; many things can point at the same asset.
- **Runtime state** is plain C# classes that change during a run: `HeroData`, `RelationshipData`, `AdventureContext`, the result types.

Everything in `Assets/Scripts/Core/Data` is pure data (its own assembly, no references). Managers and systems live elsewhere and use it.

## Managers

Singletons on the `Managers` GameObject, reached with `ClassName.Instance`:

| Manager | Holds |
| --- | --- |
| `GameManager` | Gold, undo tokens, adventure progress, run end, the `BalanceConfig` reference (`Config`) |
| `RosterManager` | Living heroes, fallen heroes, the player hero |
| `Inventory` | Item asset references |
| `HeroGenerator` | Makes recruits and the player hero |
| `RelationshipSystem` | Every relationship between heroes |

Screens never poll these; they listen to their events (`OnGoldChanged`, `OnRosterChanged`, `OnAffinityChanged`, ...).

## Startup

1. Every manager's `Awake` sets its `Instance` (order `-200`).
2. `GameManager.Start` calls `NewGame()`: resets gold, tokens and progress, clears roster, inventory and relationships, creates the player hero.
3. `DebugSeed` (Editor only, `-150`) adds test heroes and items.
4. `ScreenManager` (partner, `-100`) shows Town.

Rule: never read another manager's `Instance` in your own `Awake`; use `Start` or later.

## The loop

```
Town > Adventure Select (party of up to 6, player hero always in)
     > Battle (one fight per encounter) > Rewards
     > EndDay > Recruit (night) > StartNextDay > Town ...
Run won: boss (3rd adventure) cleared.  Run lost: player hero dies.
```
