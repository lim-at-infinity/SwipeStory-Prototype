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

## Script reference

### BalanceConfig.cs (ScriptableObject)

| Member | What it does |
| --- | --- |
| Value properties | One read-only property per tuning value, e.g. `StartingGold`, `PartySize`, `Adventures`, `ShopItems`, `AffinityPerTalk`. Edit the values on the asset, not in code. |
| `MaxAffinity` | Constant 100, the top of the affinity scale. |
| `GetAdventureIndex(adventure)` | The adventure's position in the unlock order (0 = first), or -1 if it isn't in the list. |
| `GetClassStats(heroClass)` | That class's `ClassStatProfile`. Throws if the class has no profile. |
| `GetXpToNextLevel(level)` | XP needed to go from `level` to the next one. Returns `int.MaxValue` at max level, so heroes can't level past it. |
| `GetTierForAffinity(affinity)` | The tier an affinity value falls in (Stranger to Devoted), using the tier thresholds. |
| `GetTierStatBonus(tier)` | The flat battle bonus for a tier. |
| `CanAskOut(affinity)` | True if affinity is above the ask-out threshold (60). |
| `ShouldBreakUp(affinity)` | True if affinity is below the breakup threshold (40). |
| Inspector warnings | Wrong array lengths, tier thresholds not going up, party bigger than the roster, breakup above ask-out, a missing or backwards class profile, empty or duplicate adventure slots. |

`ClassStatProfile` (inside BalanceConfig.cs): one per class. `MaxHp`, `Attack`, `Defense`, `Speed` are level 1 `IntRange`s; `MaxHpPerLevel`, `AttackPerLevel`, `DefensePerLevel`, `SpeedPerLevel` are flat growth for each level above 1.

### IntRange.cs (struct)

| Member | What it does |
| --- | --- |
| `Min`, `Max` | The two ends of the range, both included. |
| `IsValid` | True if `Min` is not above `Max`. |
| `Roll(rng)` | A random whole number from `Min` to `Max`, both included. Pass a seeded `System.Random` for repeatable results. |
| `Clamp(value)` | Pulls a value into the range. |
| `ToString()` | Prints like "3 to 7", for debugging. |

### DebugSeed.cs (Editor only)

| Member | What it does |
| --- | --- |
| Inspector `Seed On Play` | Untick to skip seeding and test a real fresh run. |
| Inspector `Heroes` | Test heroes to add: a class and an affinity with the player for each (default 0 / 30 / 65 / 90, one per tier). |
| Inspector `Items` | Item assets to put in the inventory. |
| On Play | Runs after `NewGame` and before Town opens: adds the heroes, sets their affinity through `RelationshipSystem`, adds the items. Does nothing in builds. |
