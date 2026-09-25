# SwipeStory Technical Contract

Sep 25, 2026 · @Brian Lim

## Purpose and ground rules

Everything in this doc is a shared contract: either of us can code against it today, even before the real logic exists.

- **In this doc = public.** Any class, field, method, event or enum listed here can be used from any screen or script.
- **Not in this doc = private.** Change it freely without telling the other person.
- **Stub first, fill later.** Shared classes are committed early with final signatures and simple placeholder bodies, so nobody waits on the other.
- **Changing the contract:** adding a field or method is fine (update this doc in the same PR). Renaming or removing anything listed here needs a heads-up first.
- **UI never polls.** Screens update from events, never by reading values every frame, so real logic can replace stubs without touching UI.

## Game flow and screens

The game is one Unity scene; each screen is a prefab panel shown by `ScreenManager.Show(ScreenId)`. The run ends after the 3rd adventure.

Main Menu > Town > (Recruit / Roster / Shop / Adventure Select) > Battle > Rewards > Town ... > Summary after adventure 3

| ScreenId | Screen | Opens from | Can go to |
| --- | --- | --- | --- |
| `MainMenu` | Main Menu | Game start, Summary (Restart) | Town |
| `Town` | Town Hub | Main Menu, HUD, Rewards | Recruit, Roster, Shop, AdventureSelect |
| `Recruit` | Recruitment Swipe | Town, HUD | Town (Back) |
| `Roster` | Roster | Town, HUD | Inspection, Town (Back) |
| `Inspection` | Hero Inspection | Roster | Dialogue, Roster (Back) |
| `Dialogue` | Visual Novel talk scene | Inspection (Talk button) | Inspection (Back) |
| `Shop` | Shop | Town, HUD | Town (Back) |
| `AdventureSelect` | Adventure Selection + party pick | Town, HUD | Battle, Town (Back) |
| `Battle` | Auto battle | AdventureSelect | Rewards (automatic) |
| `Rewards` | Rewards | Battle | Town, or Summary if 3 adventures done |
| `Summary` | End-of-run summary | Rewards | MainMenu (Restart) |

The **HUD / UI bar** is not a screen: it stays visible on every screen except MainMenu, Battle and Summary.

## Shared data models

These are plain C# classes (not MonoBehaviours) that every screen and system passes around.

### HeroData

| Field | Type | Range / notes |
| --- | --- | --- |
| `Id` | string | Unique, set by HeroGenerator (GUID) |
| `Name` | string | Display name |
| `Class` | HeroClass | Picks dialogue and stat bias |
| `Rarity` | Rarity | Card color on swipe screen |
| `Level` | int | 1 to 10, starts at 1 |
| `Xp` | int | 0 up to next-level threshold |
| `MaxHp` | int | Base stat |
| `CurrentHp` | int | 0 to MaxHp; reset to MaxHp before each battle |
| `Attack` | int | Base stat |
| `Defense` | int | Base stat |
| `Speed` | int | Decides battle turn order |
| `LoveScore` | int | 0 to 100, changed only through `LoveSystem` |
| `EquippedItem` | ItemData | null if none |
| `RecruitCost` | int | Gold cost shown on the swipe card |

### ItemData

| Field | Type | Range / notes |
| --- | --- | --- |
| `Id` | string | Unique per item definition |
| `Name` | string | Display name |
| `Type` | ItemType | Decides what using it does |
| `Price` | int | Shop price in gold |
| `StatBonus` | int | Used by Equipment and Potion |
| `LoveBonus` | int | Used by Gift |

### Enums

- `HeroClass`: Warrior, Mage, Rogue, Healer
- `Rarity`: Common, Rare, Epic
- `ItemType`: Equipment, Potion, Gift
- `ScreenId`: see the screen table above
- `LoveTier`: Stranger (0 to 24), Friend (25 to 49), Close (50 to 74), Devoted (75 to 100)

## Managers and public APIs

Managers are singletons on one `Managers` GameObject in the scene, reached through `ClassName.Instance`. Only the members below are shared.

### GameManager

- `int Gold { get; }`, `int UndoTokens { get; }`, `int AdventuresCompleted { get; }`
- `bool IsRunOver` (true when AdventuresCompleted == 3)
- `void NewGame()`: resets gold, tokens, adventures, roster and inventory
- `bool TrySpendGold(int amount)`: false if not enough gold
- `void AddGold(int amount)`
- `bool TryUseUndoToken()`, `void AddUndoToken()`
- `void CompleteAdventure()`

### RosterManager

- `IReadOnlyList<HeroData> Heroes`
- `int Capacity`
- `bool TryAddHero(HeroData hero)`: false if roster is full
- `void RemoveHero(HeroData hero)`
- `HeroData GetById(string id)`
- `void NotifyHeroUpdated(HeroData hero)`: call after changing any hero field; fires OnHeroUpdated

### HeroGenerator

- `HeroData Generate()`: one random hero using BalanceConfig ranges

### Inventory

- `IReadOnlyList<ItemData> Items`
- `void Add(ItemData item)`, `bool Remove(ItemData item)`
- `bool UseOn(ItemData item, HeroData hero)`: equips Equipment, applies Potion, gives Gift (calls LoveSystem)

### ScreenManager

- `ScreenId Current { get; }`
- `void Show(ScreenId id)`
- `void Back()`: returns to the previous screen
- `HeroData SelectedHero { get; set; }`: the hero passed between Roster, Inspection and Dialogue

### LoveSystem

- `void AddLove(HeroData hero, int amount)`: clamps 0 to 100, fires OnLoveChanged
- `LoveTier GetTier(HeroData hero)`
- `int GetStatBonus(HeroData hero)`: flat bonus used in battle per tier

### DialogueScreen

- `void Open(HeroData hero)`: loads the conversation for the hero's class and shows the Dialogue screen

### BalanceConfig (ScriptableObject)

All tuning numbers live here, never hardcoded in scripts: starting gold, starting undo tokens, roster capacity, recruit costs by rarity, stat ranges, love gains (talk, gift, battle), tier thresholds, reward multipliers.

## Events

All events are C# `event System.Action<...>` on the owning class. Listeners subscribe in `OnEnable` and unsubscribe in `OnDisable`.

| Event | Fired by | Payload | Listened to by |
| --- | --- | --- | --- |
| `OnGoldChanged` | GameManager | int newGold | HUD, Shop, Recruit |
| `OnUndoTokensChanged` | GameManager | int newCount | HUD, Recruit |
| `OnAdventuresChanged` | GameManager | int completed | HUD, AdventureSelect |
| `OnNewGame` | GameManager | none | All screens (reset their views) |
| `OnRosterChanged` | RosterManager | none | Roster, AdventureSelect, HUD |
| `OnHeroUpdated` | RosterManager | HeroData | Inspection, Roster (stats, level, item changed) |
| `OnInventoryChanged` | Inventory | none | Shop, Inspection |
| `OnLoveChanged` | LoveSystem | HeroData, int newScore | Inspection love meter, Dialogue |
| `OnLoveTierChanged` | LoveSystem | HeroData, LoveTier | Dialogue (tier-up message) |
| `OnScreenChanged` | ScreenManager | ScreenId from, ScreenId to | HUD (hide/show), screens (refresh on open) |
| `OnDialogueFinished` | DialogueScreen | HeroData | Inspection (refresh) |

## Screen and feature dependencies

This is what each screen or feature reads, calls and listens to. If something you need is in another row's "Provides" column, code against the stub and it will just work later.

| Screen / feature | Reads | Calls | Listens to | Provides to others |
| --- | --- | --- | --- | --- |
| Main Menu | none | `GameManager.NewGame`, `ScreenManager.Show(Town)` | none | Run start |
| HUD / UI bar | Gold, UndoTokens, AdventuresCompleted | `ScreenManager.Show` (quick nav) | OnGoldChanged, OnUndoTokensChanged, OnAdventuresChanged, OnScreenChanged | Always-on status display |
| Town Hub | AdventuresCompleted | `ScreenManager.Show` | OnAdventuresChanged | Entry point to every feature |
| Recruitment Swipe | Gold, UndoTokens, Roster Capacity | `HeroGenerator.Generate`, `TrySpendGold`, `TryAddHero`, `TryUseUndoToken` | OnGoldChanged, OnUndoTokensChanged | New HeroData in roster |
| Roster | `RosterManager.Heroes` | Sets `ScreenManager.SelectedHero`, `Show(Inspection)` | OnRosterChanged, OnHeroUpdated | Selected hero |
| Hero Inspection | `SelectedHero`, `LoveSystem.GetTier`, Inventory.Items | `DialogueScreen.Open`, `Inventory.UseOn` | OnHeroUpdated, OnLoveChanged, OnInventoryChanged, OnDialogueFinished | Talk + equip entry point |
| Love meter (prefab) | `HeroData.LoveScore`, `GetTier` | none | OnLoveChanged | Reusable meter for Inspection and Dialogue |
| Dialogue (VN) | HeroData (Name, Class, LoveScore) | `LoveSystem.AddLove`, `ScreenManager.Back` | OnLoveTierChanged | OnDialogueFinished |
| Love system | `HeroData.LoveScore`, BalanceConfig | `RosterManager.NotifyHeroUpdated` | none | Tiers, battle stat bonus |
| Shop | Gold, BalanceConfig item list | `TrySpendGold`, `Inventory.Add` | OnGoldChanged, OnInventoryChanged | Items in inventory |
| Adventure Select | `RosterManager.Heroes`, AdventuresCompleted | `ScreenManager.Show(Battle)` with chosen party | OnRosterChanged, OnAdventuresChanged | Party (List of HeroData) + AdventureData |
| Battle | Party, AdventureData, `LoveSystem.GetStatBonus` | none | none | BattleResult |
| Rewards | BattleResult | `AddGold`, `AddUndoToken`, `Inventory.Add`, `AddLove`, `CompleteAdventure` | none | Updated heroes, gold, items |
| Summary | Roster, Gold, results history | `GameManager.NewGame` | none | Restart |

**Hand-offs between screens:** Roster to Inspection to Dialogue pass the hero through `ScreenManager.SelectedHero`. Adventure Select to Battle to Rewards pass `Party`, `AdventureData` and `BattleResult` through a small `AdventureContext` holder on ScreenManager.

## Stubs and placeholder data

The first PR adds every class in this doc with final signatures and the stub behavior below, so both of us can build screens right away.

| Class | Stub behavior until real logic lands |
| --- | --- |
| GameManager | Gold = 100, UndoTokens = 1; methods change the values and fire events (this one is small enough to be real from day 1) |
| RosterManager | Real list; `TryAddHero` ignores capacity |
| HeroGenerator | Returns a hero from a fixed list of 6, cycling |
| Inventory | Real list; `UseOn` only removes the item |
| LoveSystem | `AddLove` clamps and fires events; `GetStatBonus` returns 0 |
| ScreenManager | Real: enables one panel, disables the rest |
| DialogueScreen | Shows one hardcoded line and a Close button |
| Battle | Returns a win BattleResult after 1 second |

**DebugSeed** (component on the Managers object, only active in the Editor):

- Adds 3 heroes on Play: a Warrior (LoveScore 0), a Mage (LoveScore 45) and a Healer (LoveScore 90), so every love tier can be tested
- Adds 1 of each ItemType to the Inventory
- Has a toggle to skip straight to any ScreenId, so a screen can be tested without clicking through the flow

## Conventions

**Code**

- PascalCase for classes, methods, properties and events; `_camelCase` for private fields
- Braces on their own line
- Events start with `On`; methods that can fail start with `Try` and return bool
- Game logic (battle, rewards, generation) in plain C# classes; MonoBehaviours only for screens and managers
- No magic numbers in scripts: tuning values go in BalanceConfig

**Folders**

- `Assets/Scripts/Core` (data models, managers, BalanceConfig)
- `Assets/Scripts/Screens` (one script per screen)
- `Assets/Scripts/Systems` (LoveSystem, battle, rewards)
- `Assets/Prefabs/Screens`, `Assets/Prefabs/UI` (shared widgets like hero card, stat bar, love meter)
- `Assets/Data` (ScriptableObject assets: BalanceConfig, adventures, dialogue)

**Scene and git**

- One scene (`Main.unity`). Each screen is its own prefab; edit the prefab, not the scene
- Only one person edits `Main.unity` at a time; say so in chat before touching it
- One branch per task (`T15-swipe-input`), PR into `main`, the other person reviews
- Pull before starting work; commit small and often

## Open questions

- [ ] Roster capacity and party size (suggested: roster 6, party 3)
- [ ] Can a hero die or leave the roster after a lost battle?
- [ ] Does love score ever go down (bad dialogue choice, losing a battle)?
- [ ] Can each hero be talked to once per round, or unlimited times?
- [ ] How many swipe cards per visit to the Recruit screen, and does the deck refresh after each adventure?
- [ ] Can a lost adventure be retried, or does it still count toward the 3?
