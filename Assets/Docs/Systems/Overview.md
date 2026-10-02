# Systems Overview

How the game's systems fit together. For exact signatures see `DataCheatSheet.md`; for how we work see `DevPractices.md`.

| Doc | Covers |
| --- | --- |
| `Heroes.md` | HeroData, HeroFactory, HeroGenerator, RosterManager (living and fallen) |
| `Roster.md` | Roster screen, hero detail panel, how heroes are drawn (ClassVisuals, HeroPortrait) |
| `Recruitment.md` | The Inn, the card swipe screen, swipe decks |
| `Screens.md` | Screen navigation, the day and night cycle, Town, the header bar |
| `Battle.md` | Adventure Select, auto battle, Rewards, Summary |
| `Items.md` | ItemData, item types and class locks, Inventory |
| `Relationships.md` | Affinity, tiers, dating, breakups, widows |
| `Adventures.md` | Adventures, encounters, results, unlocks, how a run ends |
| `ConfigAndTesting.md` | BalanceConfig, IntRange, DebugSeed, EditMode tests |
| `ScriptableObjects.md` | What SOs are, which of our scripts are SOs, rules for working with them |

Each doc ends with a **Script reference**: every public member of its scripts with a short explanation. GameManager's is at the bottom of this file.

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
4. `ScreenManager` (`-100`) shows Town.

Rule: never read another manager's `Instance` in your own `Awake`; use `Start` or later.

## The loop

```
Day:   Town > Adventure Select (party of up to 6, player hero always in)
            > Battle (one fight per encounter) > Rewards        (or: Town > Talk)
            > EndDay
Night: Town (only the Inn open) > Inn (recruit, roster) > Sleep > next day
Recruiting at the Inn is night only, one draw per night.
Run won: boss (3rd adventure) cleared.  Run lost: player hero dies.
```

## Script reference

### GameManager.cs (manager)

| Member | What it does |
| --- | --- |
| `Config` | The BalanceConfig asset, assigned in the Inspector. Every script reads tuning values through it. |
| `Gold`, `UndoTokens` | The player's gold and undo tokens. |
| `AdventuresCompleted` | Unlock progress: how many adventures in `Config.Adventures` have been cleared, in order (0 to 3). Replays don't raise it. |
| `IsRunOver`, `RunWon` | Whether the run has ended, and if so whether it was won (boss cleared) or lost (player hero died). |
| `NewGame()` | Starts a fresh run: resets gold, tokens and progress, clears the roster, inventory and relationships, and creates the player hero. Runs automatically on Play. |
| `TrySpendGold(amount)` | Spends gold if there's enough and returns true; otherwise changes nothing and returns false. |
| `AddGold(amount)` | Adds gold. Negative amounts are ignored with a warning (use `TrySpendGold`). |
| `TryUseUndoToken()` | Uses one undo token if there is one; returns false otherwise. |
| `AddUndoToken()` | Adds one undo token. |
| `IsUnlocked(adventure)` | True if the adventure can be played: the first always, each later one once the one before is cleared. |
| `IsCleared(adventure)` | True if the adventure has been beaten at least once. |
| `CompleteAdventure(adventure)` | Call after a won adventure. Advances progress only on the first clear of the newest unlocked adventure; clearing the boss calls `EndRun(true)`. |
| `EndRun(won)` | Ends the run once (later calls are ignored), sets `IsRunOver` and `RunWon`, and fires `OnRunEnded`. Battle calls it with false when the player hero dies. |
| Events | `OnGoldChanged`, `OnUndoTokensChanged`, `OnAdventuresChanged`, `OnRunEnded`, `OnNewGame`. |
