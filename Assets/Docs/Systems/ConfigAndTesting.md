# Config and Testing

## BalanceConfig

One asset, `Assets/Data/Config/BalanceConfig.asset`, holds every tuning number (no magic numbers in code). Code reaches it through `GameManager.Instance.Config`.

| Group | Examples |
| --- | --- |
| Run | Starting gold, starting undo tokens |
| Adventures | Ordered adventure list (last = boss) |
| Player hero | Default name and class (until class pick exists) |
| Roster and party | Roster capacity, party size |
| Day cycle | Recruit cards per night, talk candidates per day |
| Heroes | Stat ranges and growth per class, max level, XP table |
| Relationships | Tier thresholds and bonuses, affinity per talk and battle, ask-out and breakup thresholds, dating cap |
| Traits, Rewards, Shop | Trait pool, reward multipliers, shop items |

- **Values saved in the asset win over the defaults in code.** Changing a default in `BalanceConfig.cs` doesn't change an existing asset; edit the asset.
- Edits made in the Inspector during Play mode stay after you stop, which is handy for tuning.
- The Inspector warns about bad values (wrong array lengths, backwards ranges, duplicate adventures).

## DebugSeed (Editor only)

A component on the Managers object. On Play, after `NewGame`, it adds four test heroes with affinity 0 / 30 / 65 / 90 toward the player (one per tier; the 65 one can be asked out) and any item assets dragged into its list. Untick **Seed On Play** to test a real fresh run. It does nothing in builds.

## Tests

EditMode tests in `Assets/Tests/EditMode/` (assembly `SwipeStory.Tests.EditMode`) cover the pure data logic: tier edges, XP table, ask-out and breakup thresholds, `IntRange` rolls, item class locks, `HeroFactory` stats, `AdventureContext` and the results, `RelationshipGraph`.

Run them with Window > General > Test Runner > EditMode > Run All before opening a PR that touches `Core/Data`. Managers and screens are checked by playing with DebugSeed.
