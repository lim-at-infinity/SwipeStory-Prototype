# Adventures

Adventure and battle **mechanics** belong to Zihui Yang. This doc covers the data and progress they build on; the result types are drafts he can reshape.

## Data

The important members (`AdventureData.cs`, `EncounterData.cs`):

```csharp
// AdventureData: one asset per adventure in Assets/Data/Adventures/
public IReadOnlyList<EncounterData> Encounters;   // one fight each, fought in order
public int GoldReward;                            // rewards are given once, for clearing the whole adventure
public int XpReward;
public int UndoTokenReward;
public IReadOnlyList<ItemData> ItemRewards;

// EncounterData: one fight, edited inside the AdventureData asset
public string Name;                               // also the element's label in the Inspector
public IReadOnlyList<EnemyData> Enemies;          // EnemyData: Name, MaxHp, Attack, Defense, Speed
```

Prototype content: Adventure 1 = 2 encounters, Adventure 2 = 3, Adventure 3 (boss) = 1. The number of fights is just how many encounters the asset has.

## Unlocks and progress

`BalanceConfig.Adventures` is the ordered list; the last entry is the boss.

- `GameManager.AdventuresCompleted` = how many in the list have been cleared, in order (0 to 3). Replays don't count.
- `IsUnlocked(adventure)`: the first is always unlocked; each next one unlocks when the one before is cleared.
- `IsCleared(adventure)`: already beaten (can still be replayed).
- `CompleteAdventure(adventure)`: call after a **won** adventure. Advances progress only on a first clear of the newest unlocked one.

## How a run ends

`GameManager.EndRun(won)` ends the run once (later calls are ignored), sets `IsRunOver` and `RunWon`, fires `OnRunEnded`.

- **Won:** `CompleteAdventure` on the boss calls `EndRun(true)` automatically.
- **Lost:** battle calls `EndRun(false)` when the player hero dies.

## Passing an adventure between screens

`AdventureContext` carries one adventure from Adventure Select to Battle to Rewards. It will live on `ScreenManager.Adventure` (added on Zihui's `screen-manager` branch). Below is how each screen is expected to use it; the fight simulation itself is Zihui's.

The two result types:
- `BattleResult`: one fight. `Encounter`, `Won`, `Fallen` (NPC heroes who died in it).
- `AdventureResult`: the whole adventure. `Battles` plus the rewards. `Won` and `Fallen` are calculated from its battles, never passed in.

### Adventure Select

```csharp
AdventureContext context = ScreenManager.Instance.Adventure;

// party: up to Config.PartySize heroes, the player hero always included
context.Begin(adventure, party);   // also clears Battles and LastResult from the previous adventure
ScreenManager.Instance.Show(ScreenId.Battle);
```

### Battle

```csharp
AdventureContext context = ScreenManager.Instance.Adventure;
AdventureData adventure = context.Adventure;

while (context.HasMoreEncounters)
{
    EncounterData encounter = adventure.Encounters[context.CurrentEncounterIndex];

    // Zihui's fight simulation decides these
    bool won = ...;
    List<HeroData> fallen = ...;          // NPC heroes who died in this fight
    bool playerHeroDied = ...;

    context.RecordBattle(new BattleResult(encounter, won, fallen));   // CurrentEncounterIndex moves to the next fight

    if (playerHeroDied)
    {
        GameManager.Instance.EndRun(false);   // run lost
        break;
    }

    if (!won)
    {
        break;                                // a lost fight ends the adventure early
    }
}

// Rewards only for a cleared adventure (every encounter fought and won); already scaled by the multipliers
BalanceConfig config = GameManager.Instance.Config;
bool cleared = !context.HasMoreEncounters && context.Battles[context.Battles.Count - 1].Won;

context.Finish(new AdventureResult(adventure, context.Party, context.Battles,
    goldEarned: cleared ? Mathf.RoundToInt(adventure.GoldReward * config.GoldRewardMultiplier) : 0,
    xpEarned: cleared ? Mathf.RoundToInt(adventure.XpReward * config.XpRewardMultiplier) : 0,
    undoTokensEarned: cleared ? adventure.UndoTokenReward : 0,
    itemsEarned: cleared ? adventure.ItemRewards : null));

ScreenManager.Instance.Show(ScreenId.Rewards);
```

### Rewards

```csharp
AdventureResult result = ScreenManager.Instance.Adventure.LastResult;
BalanceConfig config = GameManager.Instance.Config;

GameManager.Instance.AddGold(result.GoldEarned);
for (int i = 0; i < result.UndoTokensEarned; i++)
{
    GameManager.Instance.AddUndoToken();
}

foreach (ItemData item in result.ItemsEarned)
{
    Inventory.Instance.Add(item);
}

// Survivors come home: back to full HP, and a bit closer to the player (the player hero is ignored for affinity)
foreach (HeroData hero in result.Party)
{
    if (!result.Fallen.Contains(hero))
    {
        hero.RestoreFullHp();
        RelationshipSystem.Instance.AddAffinity(hero, config.AffinityPerBattle);
    }
}

// Dead heroes leave the roster; their gear is lost and their partners become Widowed
foreach (HeroData hero in result.Fallen)
{
    RosterManager.Instance.MarkFallen(hero);
}

// XP and leveling: not built yet

if (result.Won)
{
    GameManager.Instance.CompleteAdventure(result.Adventure);   // unlocks the next one; clearing the boss wins the run
}

if (GameManager.Instance.IsRunOver)
{
    ScreenManager.Instance.Show(ScreenId.Summary);   // boss cleared or player hero died
}
else
{
    DayCycle.Instance.EndDay();                       // night: Recruit
}
```

`context.History` keeps every AdventureResult of the run for Summary.

Open for the battle side: HP carry-over between encounters, rewards on replays.

## Script reference

GameManager's adventure members (`IsUnlocked`, `IsCleared`, `CompleteAdventure`, `EndRun`) are in `Overview.md`.

### AdventureData.cs (ScriptableObject)

| Member | What it does |
| --- | --- |
| `Id`, `DisplayName`, `Description` | Identity and display. |
| `Difficulty` | A 1 to 5 label. Nothing uses it yet. |
| `Encounters` | The fights, in the order they're fought. |
| `GoldReward`, `XpReward`, `UndoTokenReward`, `ItemRewards` | Base rewards for clearing the whole adventure. Gold and XP get scaled by the BalanceConfig multipliers when the AdventureResult is made. |
| Inspector warnings | Missing Id, no encounters, or an encounter with no enemies. |

### EncounterData.cs (plain class, edited inside AdventureData)

| Member | What it does |
| --- | --- |
| `Name` | The fight's name, also shown as its label in the Inspector list. |
| `Enemies` | The enemies fought together in this fight. |

### EnemyData.cs (plain class, edited inside EncounterData)

| Member | What it does |
| --- | --- |
| `Name`, `MaxHp`, `Attack`, `Defense`, `Speed` | The enemy's stats (MaxHp defaults to 10). Battle makes its own copy to track damage. |
| `EnemyData(name, maxHp, attack, defense, speed)` | Makes an enemy from code, e.g. in tests. |

### BattleResult.cs (plain class, can't change after creation)

| Member | What it does |
| --- | --- |
| `Encounter`, `Won`, `Fallen` | Which fight this was, whether the party won it, and the NPC heroes who died in it. |
| `BattleResult(encounter, won, fallen)` | Records one fight. `fallen` can be null for "nobody". |

### AdventureResult.cs (plain class, can't change after creation)

| Member | What it does |
| --- | --- |
| `Adventure`, `Party`, `Battles` | Which adventure, who went, and one BattleResult per fight fought. |
| `GoldEarned`, `XpEarned`, `UndoTokensEarned`, `ItemsEarned` | The rewards, already scaled. Given once for the whole adventure. |
| `Won` | Calculated: true only if every encounter was fought and won. |
| `Fallen` | Calculated: everyone who fell in any of the battles. |
| `AdventureResult(adventure, party, battles, goldEarned, xpEarned, undoTokensEarned, itemsEarned)` | Records the whole adventure. Lists are copied, so later changes elsewhere don't affect it. |

### AdventureContext.cs (plain class, lives on ScreenManager)

| Member | What it does |
| --- | --- |
| `Adventure`, `Party` | The adventure being played and the heroes sent. |
| `Battles` | Fights finished so far in the current adventure. |
| `CurrentEncounterIndex` | Which encounter is fought next (0 = first). |
| `HasMoreEncounters` | True while there are encounters left. Battle decides whether to stop early after a loss. |
| `LastResult`, `History` | The latest AdventureResult, and every AdventureResult this run (for Summary). |
| `Begin(adventure, party)` | Starts an adventure: sets the adventure and party, clears `Battles` and `LastResult`. Called by Adventure Select. |
| `RecordBattle(result)` | Adds one fight's result. Throws if called before `Begin`. |
| `Finish(result)` | Stores the AdventureResult as `LastResult` and adds it to `History`. Called by Battle before showing Rewards. |
| `Reset()` | Clears everything, including `History`. Call it on `GameManager.OnNewGame`. |
