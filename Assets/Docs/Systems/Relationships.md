# Relationships

## The model

A relationship belongs to a **pair of heroes** (the player hero counts as a hero). Each pair has one `RelationshipData`:

| Field | Meaning |
| --- | --- |
| `HeroAId`, `HeroBId` | The two heroes, by Id |
| `Affinity` | 0 to 100, **mutual** (one number for the pair) |
| `Status` | None, Dating, Ex, Widowed |

`RelationshipGraph` stores them. A pair with no record has affinity 0 and no status; records are created the first time two heroes interact. (A, B) and (B, A) are the same record.

The prototype only uses **player-to-NPC** pairs. NPC-to-NPC works the same way and needs no data change later.

## Using it

Always go through `RelationshipSystem.Instance`. Every method has two forms:

- **One hero** = with the player hero: `GetAffinity(hero)`, `AddAffinity(hero, 5)`, `CanAskOut(hero)`, ...
- **Two heroes** = any pair: `GetAffinity(hero, other)`, `AddAffinity(hero, other, 5)`, ...

## Tiers

Calculated from affinity, never stored (thresholds in `BalanceConfig`):

| Tier | Affinity | Battle bonus |
| --- | --- | --- |
| Stranger | 0 to 24 | +0 |
| Friend | 25 to 49 | +1 |
| Close | 50 to 74 | +2 |
| Devoted | 75 to 100 | +4 |

`GetStatBonus(hero)` gives the bonus from the hero's tier with the player.

## Status rules

```
None --(TryStartDating)--> Dating --(affinity drops below 40)--> Ex
                             |
                             +--(partner dies)--> Widowed
```

- **Asking out** (`CanAskOut` / `TryStartDating`): affinity above 60, no status yet, and both heroes under their dating cap.
- **Dating cap:** 1 partner, or a trait's `DatingCapOverride` (e.g. 3) if higher.
- **Breakup:** automatic inside `AddAffinity` when a Dating pair falls below 40.
- **Death:** `RelationshipSystem` listens to `RosterManager.OnHeroFell`. The fallen hero's Dating records become Widowed; Ex and Widowed records are kept as history; records with no status are deleted.
- Ex and Widowed pairs can't restart dating (for now).

## Events

`OnAffinityChanged`, `OnTierChanged`, `OnStatusChanged`, each with `(hero, other, newValue)`.

## Not yet

Charmer's affinity multiplier, Ex party debuffs, NPC-to-NPC affinity sources (battles, interactions).

## Script reference

### Relationships.cs

Enums `RelationshipTier` (Stranger, Friend, Close, Devoted; always calculated from affinity) and `RelationshipStatus` (None, Dating, Ex, Widowed; stored on each pair).

### TraitData.cs (ScriptableObject)

| Member | What it does |
| --- | --- |
| `Id`, `DisplayName`, `Description`, `Icon` | Identity and display, like ItemData. |
| `AffinityGainMultiplier` | 1 = normal; Charmer uses 2. Stored but not applied yet. |
| `DatingCapOverride` | 0 = use the default cap. A higher number (e.g. 3) lets the hero date that many people. |

### RelationshipData.cs (plain class)

| Member | What it does |
| --- | --- |
| `HeroAId`, `HeroBId` | The two heroes, by Id, stored in a fixed order. They never change. |
| `Affinity`, `Status` | The pair's mutual affinity (0 to 100) and status. Only `RelationshipSystem` should change them. |
| `Involves(heroId)` | True if that hero is one of the two. |
| `GetOtherId(heroId)` | The other hero's Id, seen from one side (e.g. "who is Aria dating?"). |

### RelationshipGraph.cs (plain class)

| Member | What it does |
| --- | --- |
| `All` | Every relationship record. |
| `Get(heroId, otherId)` | The pair's record, or null if they've never interacted. The order of the two Ids doesn't matter. |
| `GetOrCreate(heroId, otherId)` | The pair's record, creating it if needed. Throws if an Id is missing or both are the same hero. |
| `GetAffinity(heroId, otherId)`, `GetStatus(heroId, otherId)` | The pair's affinity or status; 0 or None if there's no record. |
| `GetRelationshipsOf(heroId)` | Every record that involves the hero. |
| `CountWithStatus(heroId, status)` | How many of the hero's relationships have that status, e.g. how many people they're dating. |
| `HandleDeath(heroId)` | The dead hero's Dating records become Widowed, Ex and Widowed records are kept, and records with no status are deleted. |
| `Clear()` | Removes every record. |

### RelationshipSystem.cs (manager)

Methods marked "1 or 2" come in two forms: with one hero they mean "with the player hero", with two heroes they work for any pair.

| Member | What it does |
| --- | --- |
| `GetAffinity` (1 or 2) | The pair's affinity, 0 to 100 (0 if they've never interacted). |
| `GetStatus` (1 or 2) | The pair's status: None, Dating, Ex or Widowed. |
| `GetTier` (1 or 2) | The tier for the pair's affinity (Stranger to Devoted). |
| `AddAffinity` (1 or 2) | Changes affinity by an amount, clamped to 0..100, and fires `OnAffinityChanged` (plus `OnTierChanged` if the tier changed). A Dating pair that falls below the breakup threshold becomes Ex. |
| `CanAskOut` (1 or 2) | True if affinity is above the ask-out threshold, the pair has no status yet, and both are under their dating cap. |
| `TryStartDating` (1 or 2) | Sets the pair to Dating if `CanAskOut` allows it and fires `OnStatusChanged`; returns false otherwise. |
| `GetStatBonus(hero)` | The flat battle bonus from the hero's tier with the player; 0 for the player hero. |
| `GetDatingCap(hero)` | How many partners the hero may have: `DefaultDatingCap`, or the highest `DatingCapOverride` among their traits. |
| `Relationships`, `GetRelationshipsOf(hero)` | All records, or one hero's, for UI that shows the relationship web. |
| `Clear()` (internal) | Removes every relationship. Only `GameManager.NewGame` calls it. |
| Deaths (automatic) | Listens to `RosterManager.OnHeroFell`; the fallen hero's partners become Widowed and `OnStatusChanged` fires for each. |
| Events | `OnAffinityChanged`, `OnTierChanged`, `OnStatusChanged`, each with `(hero, other, newValue)`. |
