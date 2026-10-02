# Battle, Rewards and Summary

An adventure runs through three screens: **Adventure Select** picks the adventure and party, **Battle** fights every encounter, and **Rewards** applies the result. When the run ends, **Summary** shows how it went. The adventure data itself (adventures, encounters, results, unlocks) is described in `Adventures.md`.

## Adventure Select

- Lists the adventures from `BalanceConfig.Adventures` in order. Locked ones can't be picked; cleared ones are marked and can be replayed.
- **The party is picked automatically:** the player hero, plus the NPC heroes with the highest affinity, up to `PartySize` (6).
- Picking an adventure starts it on `ScreenManager.Adventure` and opens Battle.

## How a fight works

`BattleSimulator` plays a whole fight instantly, then the Battle screen plays the log back line by line.

| Rule | Detail |
| --- | --- |
| Stats | Hero: base stats, plus weapon (Attack), hat (Defense) and relationship tier bonus (both). Enemy: from its `EnemyData`. |
| Turn order | Each round, every living fighter acts once, fastest first. |
| Damage | `max(1, attack - defense)`. |
| Targets | Heroes hit the enemy with the lowest HP. Enemies hit a random hero. |
| Healers | Heal the most hurt ally below half HP (by their Attack) instead of attacking. |
| Time limit | After 30 rounds the party retreats, which counts as a loss. |

- Encounters are fought in order, and **HP carries over** between them within one adventure.
- A lost fight ends the adventure. **If the player hero dies, the run ends** (`GameManager.EndRun(false)`).
- NPC heroes who die are listed as fallen.
- Skip finishes the playback instantly.

## Rewards

Applies the adventure's result once, then shows what happened:

| Result | Effect |
| --- | --- |
| Cleared (every fight won) | Gold (`GoldReward` × multiplier), undo tokens and item rewards; the next adventure unlocks on a first clear. Clearing the boss wins the run. |
| Lost | No rewards. |
| Survivors | Restored to full HP, and gain `AffinityPerBattle` affinity with the player. |
| Fallen | Leave the roster for good; their gear is lost and their partners become Widowed. |

Continue ends the day, or opens Summary if the run is over. XP and leveling are not built yet.

## Summary

Shows whether the run was won (boss defeated) or lost (player hero fell), the days taken, adventures cleared and attempted, the guild's size and the fallen. **New Run** starts over on day 1 with a fresh roster.

## Script reference

### BattleSimulator.cs (plain class)

| Member | What it does |
| --- | --- |
| `BattleSimulator(rng)` | Takes a `System.Random`, so fights can be repeated in tests. |
| `Fight(heroes, enemies, log)` | Plays one fight, writing each action to `log`. Returns true if the heroes won. Heroes keep their HP afterwards. |
| `MaxRounds` | 30. A fight that lasts longer is a loss. |

### BattleUnit.cs (plain class)

| Member | What it does |
| --- | --- |
| `FromHero(hero, tierBonus)` | A fighter from a hero, with equipment and relationship bonuses added. |
| `FromEnemy(enemy, name)` | A fighter from an enemy definition, at full HP. |
| `Name`, `MaxHp`, `CurrentHp`, `Attack`, `Defense`, `Speed` | Fight stats. Only `CurrentHp` changes during a fight. |
| `Hero`, `IsHero`, `IsHealer`, `IsAlive` | The hero behind the fighter (null for enemies) and quick checks. |

### BattleEvent.cs (plain class)

One line of the fight log: `Text`, plus the `Target` and its `HpAfter` when the line changes someone's HP.

### BattleScreen.cs (screen)

Fights each encounter of the current adventure, plays the log back, records each fight, writes HP back to the heroes, then opens Rewards. Inspector values set the playback speed and how many log lines show.

### BattleUnitRow.cs (UI component)

One row in the Battle screen's hero or enemy list: portrait (heroes only) and "Name hp/max". Fallen fighters fade out.

### AdventureSelectScreen.cs, RewardsScreen.cs, SummaryScreen.cs (screens)

Described above. Rewards remembers the last result it applied, so reopening the screen never applies a result twice.
