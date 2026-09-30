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
