# SwipeStory Technical Contract

Sep 25, 2026 · @Brian Lim · Updated Sep 29, 2026 (day cycle, no Main Menu, shared UI pieces; Core-Data: player hero, ScriptableObject items and traits, relationships, no rarity)

See also: `DevPractices.md` (how we work) and `Systems/` (how each system works, start with `Systems/Overview.md`).

## Purpose and ground rules

Everything in this doc is a shared contract: either of us can code against it today, even before the real logic exists.

- **In this doc = public.** Any class, field, method, event or enum listed here can be used from any screen or script.
- **Not in this doc = private.** Change it freely without telling the other person.
- **Stub first, fill later.** Shared classes are committed early with final signatures and simple placeholder bodies, so nobody waits on the other.
- **Changing the contract:** adding a field or method is fine (update this doc in the same PR). Renaming or removing anything listed here needs a heads-up first.
- **UI never polls.** Screens update from events, never by reading values every frame, so real logic can replace stubs without touching UI.

## Game flow and screens

The game is one Unity scene; each screen is a panel under `Canvas/Screens` (saved as a prefab) shown by `ScreenManager.Show(ScreenId)`. There is no Main Menu: the game starts in Town, and `GameManager.NewGame()` runs automatically on Play (it also creates the player hero). There are 3 adventures in a fixed order (the 3rd is the boss). Only the first starts unlocked; clearing one unlocks the next, and cleared adventures can be replayed. The run is won by clearing the boss and lost when the player hero dies.

**Day cycle:** each day has a Daytime and a Night phase.

- **Daytime:** the player is in Town and can freely browse Roster, Inspection and Shop. Doing **one activity** (an adventure, or talking to a hero) ends the day.
- **Night:** `DayCycle.EndDay()` opens Recruit. After 5 hero cards, `DayCycle.StartNextDay()` increments the day and returns to Town. Recruiting is free.

Town (day N) > [Adventure Select > Battle > Rewards] or [Dialogue] > EndDay > Recruit (night N, 5 cards) > StartNextDay > Town (day N+1) ... > Summary after adventure 3

| ScreenId | Value | Screen | Opens from | Can go to |
| --- | --- | --- | --- | --- |
| `None` | 0 | (no screen; initial value of `Current`) | | |
| `Town` | 1 | Town Hub (game start) | Game start, HUD, `StartNextDay` | Roster, Shop, AdventureSelect, Dialogue (talk, hidden on day 1) |
| `Recruit` | 2 | Recruitment Swipe (night only) | `EndDay` | Town (via `StartNextDay` after 5 cards) |
| `Roster` | 3 | Roster (grid + hero detail panel) | Town, HUD | Back |
| `Inspection` | 4 | Unused: inspection is a panel on Roster. Kept so later values don't shift | | |
| `Dialogue` | 5 | Visual Novel talk scene | Town (talk buttons) | Recruit (via `EndDay` when finished) |
| `Shop` | 6 | Shop | Town, HUD | Back |
| `AdventureSelect` | 7 | Adventure Selection + party pick | Town, HUD | Battle, Back |
| `Battle` | 8 | Auto battle | AdventureSelect | Rewards (automatic) |
| `Rewards` | 9 | Rewards | Battle | Recruit (via `EndDay`), or Summary if the run is over (boss cleared or player hero died) |
| `Summary` | 10 | End-of-run summary | Rewards | Town (Restart, after `GameManager.NewGame`) |

**ScreenId values are fixed numbers** because Unity saves them as ints in scenes and prefabs. Never reorder or renumber; only append new ones at the end. Planned screens (class pick, trait draw, Main Menu) get their values when they are added.

**Back history:** showing `Town` or `Recruit` clears the Back history, so Back never leaves those screens.

The **HUD / UI bar** is not a screen: it stays visible on every screen except Battle and Summary.

## Shared data

Runtime state is a **plain C# class** (`HeroData`, `BattleResult`, `AdventureResult`, `AdventureContext`). Anything designed in the Editor is a **ScriptableObject asset** (`ItemData`, `TraitData`, `AdventureData`, `BalanceConfig`), created with Create > SwipeStory > ... and **never changed at runtime**. Heroes and inventories hold references to those assets, never copies.

All types in this section live in `Assets/Scripts/Core/Data/` (assembly `SwipeStory.Data`).

### HeroData (plain class)

| Member | Type | Range / notes |
| --- | --- | --- |
| `Id` | string | GUID set in the constructor, read-only |
| `Name` | string | Display name |
| `Class` | HeroClass | Picks dialogue, stat profile and which weapons fit |
| `IsPlayer` | bool | True for the player's own hero; exactly one per run |
| `Color` | Color | Random hue rolled once by `HeroFactory` (saturation and brightness from BalanceConfig). Tints the hero's class shape everywhere. White by default |
| `Level` | int | 1 to `BalanceConfig.MaxLevel` (10). Recruits arrive at the player hero's level |
| `Xp` | int | 0 up to `BalanceConfig.GetXpToNextLevel(Level)` |
| `MaxHp` | int | Base stat |
| `CurrentHp` | int | 0 to MaxHp. Survivors are restored to MaxHp when the party returns from an adventure |
| `Attack` | int | Base stat |
| `Defense` | int | Base stat |
| `Speed` | int | Decides battle turn order |
| `Weapon` | ItemData | null if none. Sword, Dagger, Staff or Cross (class locked) |
| `Hat` | ItemData | null if none. Any class |
| `Traits` | IReadOnlyList\<TraitData\> | Added with `AddTrait` |

- `HeroData(string name, HeroClass heroClass, bool isPlayer = false)`
- `void RestoreFullHp()`: Rewards calls it on every surviving party member when the party returns
- `ItemData GetEquipped(EquipSlot slot)`, `void SetEquipped(EquipSlot slot, ItemData item)`: throws if the item doesn't fit that slot. Does not check class locks; use `Inventory.UseOn` for that
- `void AddTrait(TraitData trait)`, `bool HasTrait(TraitData trait)`

Base stats never include equipment or tier bonuses; battle adds those. Affinity and relationship status are **not** on HeroData: they belong to a pair of heroes and live in `RelationshipSystem` (see RelationshipData below).

### ItemData (ScriptableObject, `Assets/Data/Items/`)

| Member | Type | Range / notes |
| --- | --- | --- |
| `Id` | string | Unique per item asset |
| `DisplayName` | string | |
| `Description` | string | |
| `Icon` | Sprite | |
| `Type` | ItemType | Decides the slot, class lock and what using it does |
| `Stars` | int | 1 to 3 |
| `Price` | int | Shop price in gold |
| `StatBonus` | int | Weapon: Attack. Hat: Defense. Potion: HP restored |
| `AffinityBonus` | int | Gift only |
| `Slot` | EquipSlot | Read-only, from `Type` |

- `bool CanBeEquippedBy(HeroClass heroClass)`

Example: `OldSword.asset` has Type Sword, 1 star, small StatBonus.

### TraitData (ScriptableObject, `Assets/Data/Traits/`)

| Member | Type | Range / notes |
| --- | --- | --- |
| `Id`, `DisplayName`, `Description`, `Icon` | | |
| `AffinityGainMultiplier` | float | 1 = normal. Charmer: 2 |
| `DatingCapOverride` | int | 0 = use `BalanceConfig.DefaultDatingCap`. Multi-dating trait (name TBD): 3 |

Effects are data only for now; nothing applies them yet.

### ClassVisuals (ScriptableObject, `Assets/Data/Config/ClassVisuals.asset`)

One `ClassVisual` per `HeroClass`: the class's shape. Color is per hero (`HeroData.Color`), not per class. Placeholder sprites live in `Assets/Art/Sprites/Placeholder/` (square, triangle, capsule, circle); swap in real art on the asset, no code changes.

| Member | Type | Notes |
| --- | --- | --- |
| `Class` | HeroClass | |
| `Sprite` | Sprite | Small token: roster tiles, battle |
| `Portrait` | Sprite | Large art: recruit cards, dialogue. Optional; the prototype leaves it empty |
| `PortraitOrSprite` | Sprite | Read-only. `Portrait`, or `Sprite` while there's no portrait. Read this instead of `Portrait` |

- `ClassVisual Get(HeroClass heroClass)`: never throws; warns and returns an empty visual (a white box) if the class is missing

### AdventureData (ScriptableObject, `Assets/Data/Adventures/`)

A series of fights, fought in order. `Id`, `DisplayName`, `Description`, `Difficulty` (1 to 5), `Encounters` (IReadOnlyList\<EncounterData\>), `GoldReward`, `XpReward`, `UndoTokenReward`, `ItemRewards` (IReadOnlyList\<ItemData\>). Rewards are base values, given once for clearing the whole adventure.

Prototype content: Adventure 1 has 2 encounters, Adventure 2 has 3, Adventure 3 (boss) has 1. The unlock order is `BalanceConfig.Adventures`.

**EncounterData** (plain class, authored inside AdventureData): one fight. `Name`, `Enemies` (IReadOnlyList\<EnemyData\>).

**EnemyData** (plain class, authored inside EncounterData): `Name`, `MaxHp`, `Attack`, `Defense`, `Speed`. Battle makes its own copy to track HP.

Battle rules (HP carry-over between encounters, turn order, when fallen heroes are removed) belong to the battle side, not this data.

### BattleResult (plain class, immutable)

Outcome of **one fight** (one encounter). `Encounter` (EncounterData), `Won`, `Fallen` (NPC heroes who died in this fight).

Draft: the battle side can add fields (turns, damage, and so on) as the battle design needs.

### AdventureResult (plain class, immutable)

Outcome of **one whole adventure**. `Adventure`, `Party`, `Battles` (IReadOnlyList\<BattleResult\>, one per encounter fought), `GoldEarned`, `XpEarned`, `UndoTokensEarned`, `ItemsEarned`. Rewards are given once and already scaled by BalanceConfig multipliers.

- `Won` (derived): every encounter was fought and won. Losing any fight, or stopping early, means false
- `Fallen` (derived): everyone who fell in any of the battles

Draft: the battle side can reshape it as needed.

### AdventureContext (plain class)

Carries one adventure from AdventureSelect through Battle to Rewards, plus the run's history for Summary.

- `AdventureData Adventure`, `IReadOnlyList<HeroData> Party`
- `IReadOnlyList<BattleResult> Battles`: fights finished so far in the current adventure
- `int CurrentEncounterIndex`: which encounter is fought next (0 = first). `bool HasMoreEncounters`: count only; Battle decides whether to stop early after a loss
- `AdventureResult LastResult`, `IReadOnlyList<AdventureResult> History`
- `void Begin(AdventureData adventure, IEnumerable<HeroData> party)`: AdventureSelect, before showing Battle. Clears `Battles` and `LastResult`
- `void RecordBattle(BattleResult result)`: Battle, after each fight. Throws if called before `Begin`
- `void Finish(AdventureResult result)`: Battle, after the last fight or a loss, before showing Rewards
- `void Reset()`: on `GameManager.OnNewGame`

Battle loop: while `HasMoreEncounters`, fight `Adventure.Encounters[CurrentEncounterIndex]`, `RecordBattle`, stop on a loss or player hero death; then `Finish(new AdventureResult(...))` and show Rewards.

### RelationshipData (plain class) and RelationshipGraph

One **mutual** relationship per pair of heroes, the player hero included: `HeroAId`, `HeroBId`, `Affinity` (0 to 100), `Status` (RelationshipStatus). Helpers: `bool Involves(string heroId)`, `string GetOtherId(string heroId)`.

`RelationshipGraph` stores them. A missing record means affinity 0 and no status; records are created on first contact, and (A, B) and (B, A) are the same record. Methods: `Get`, `GetOrCreate`, `GetAffinity`, `GetStatus`, `GetRelationshipsOf`, `CountWithStatus`, `HandleDeath`, `Clear`. Screens don't use the graph directly; they go through `RelationshipSystem`.

The prototype only uses player-to-NPC pairs. NPC-to-NPC affinity (battle and interaction events) comes later, with no data change needed.

### Small helpers

- **IntRange** (serializable struct): `Min`, `Max`, `int Roll(System.Random rng)` (both ends included), `int Clamp(int value)`
- **ClassStatProfile** (serializable struct in BalanceConfig): level 1 `IntRange` for MaxHp / Attack / Defense / Speed, plus flat growth per level
- **ItemTypeRules** (static): `EquipSlot GetSlot(ItemType)`, `bool TryGetRequiredClass(ItemType, out HeroClass)`, `bool CanEquip(ItemType, HeroClass)`
- **HeroFactory** (static): `HeroData Create(BalanceConfig config, System.Random rng, string name, HeroClass heroClass, int level, bool isPlayer = false)`. Stats = level 1 roll + growth per level gained

### Enums (explicit values, append only)

- `HeroClass`: Warrior = 0, Mage = 1, Rogue = 2, Healer = 3
- `ItemType`: Hat = 0, Sword = 1, Dagger = 2, Staff = 3, Cross = 4, Potion = 5, Gift = 6
- `EquipSlot`: None = 0, Weapon = 1, Hat = 2
- `RelationshipTier`: Stranger = 0 (affinity 0 to 24), Friend = 1 (25 to 49), Close = 2 (50 to 74), Devoted = 3 (75 to 100). Always calculated from affinity, never stored
- `RelationshipStatus`: None = 0, Dating = 1, Ex = 2, Widowed = 3. Stored per pair on RelationshipData
- `ScreenId`: see the screen table above

**Class locks:**

| ItemType | Slot | Who can equip |
| --- | --- | --- |
| Hat | Hat | Any class |
| Sword | Weapon | Warrior |
| Dagger | Weapon | Rogue |
| Staff | Weapon | Mage |
| Cross | Weapon | Healer |
| Potion, Gift | None | Used, not equipped |

## Managers and public APIs

Managers are singletons on one `Managers` GameObject in the scene, reached through `ClassName.Instance`. Only the members below are shared.

**Startup order:** data managers (GameManager, RosterManager, Inventory, HeroGenerator, RelationshipSystem) run at `[DefaultExecutionOrder(-200)]`, DebugSeed at `-150`, ScreenManager and DayCycle at `-100`. `GameManager.Start` calls `NewGame()`, so the roster and player hero exist before Town opens.

### GameManager

- `BalanceConfig Config { get; }`: the one BalanceConfig asset, assigned in the Inspector
- `int Gold { get; }`, `int UndoTokens { get; }`, `int AdventuresCompleted { get; }`
- `AdventuresCompleted` is **unlock progress**: how many adventures in `Config.Adventures` have been cleared, in order (0 to 3). Replays don't count
- `bool IsRunOver`, `bool RunWon`: set by `EndRun`
- `void NewGame()`: resets gold, tokens, adventures, roster and inventory, then creates the player hero (`Config.DefaultPlayerClass` until class pick exists)
- `bool TrySpendGold(int amount)`: false if not enough gold
- `void AddGold(int amount)`
- `bool TryUseUndoToken()`, `void AddUndoToken()`
- `bool IsUnlocked(AdventureData adventure)`, `bool IsCleared(AdventureData adventure)`
- `void CompleteAdventure(AdventureData adventure)`: call after a **won** adventure. Advances progress only on the first clear of the newest unlocked adventure; clearing the boss calls `EndRun(true)`
- `void EndRun(bool won)`: ends the run once (later calls are ignored) and fires OnRunEnded. Battle calls `EndRun(false)` when the player hero dies

### RosterManager

- `IReadOnlyList<HeroData> Heroes`
- `HeroData PlayerHero { get; }`
- `int Capacity`: includes the player hero; 0 = unlimited. `bool IsFull`: always false when unlimited
- `bool TryAddHero(HeroData hero)`: false if full, null, already in the roster, or a second player hero
- `void RemoveHero(HeroData hero)`: ignored for the player hero. For non-death removals; deaths use `MarkFallen`
- `void MarkFallen(HeroData hero)`: NPC death. Moves the hero from `Heroes` to `FallenHeroes`, fires OnHeroFell and OnRosterChanged. Their equipped items are lost with them. Ignored for the player hero (their death is `GameManager.EndRun(false)`)
- `IReadOnlyList<HeroData> FallenHeroes`: NPC heroes who died this run, oldest first; cleared by `NewGame`
- `HeroData GetById(string id)`: living heroes only. `HeroData GetFallenById(string id)`: fallen heroes (e.g. to show a Widowed partner's name)
- `void NotifyHeroUpdated(HeroData hero)`: call after changing any hero field; fires OnHeroUpdated

### HeroGenerator

- `HeroData Generate()`: one random recruit (random class and name) at the player hero's level
- `HeroData CreatePlayer(HeroClass heroClass, string name)`: level 1 player hero

### Inventory

- `IReadOnlyList<ItemData> Items`: references to item assets; the same asset can appear more than once
- `void Add(ItemData item)`, `bool Remove(ItemData item)`
- `bool UseOn(ItemData item, HeroData hero)`: Weapon or Hat equips if the class allows it (the old item returns to the inventory); Potion restores HP; Gift adds affinity through RelationshipSystem (not on the player hero). Removes the used item. False if nothing happened

### ScreenManager

- `ScreenId Current { get; }`: `None` until the start screen (Town) is shown
- `void Show(ScreenId id)`: showing Town or Recruit clears the Back history
- `void Back()`: returns to the previous screen
- *Planned, added on screen-manager now that the data types exist:*
  - `HeroData SelectedHero { get; set; }`: the hero passed between Roster and Inspection, and from Town's talk buttons into Dialogue
  - `AdventureContext Adventure { get; }`: created once; `Reset()` it on `GameManager.OnNewGame`

### DayCycle

- `int Day { get; }`: starts at 1
- `bool IsNight { get; }`
- `void EndDay()`: call after the day's one activity (Rewards or a finished conversation); switches to night and shows Recruit. Ignored at night
- `void StartNextDay()`: call when the night's recruit cards are used up; increments Day and shows Town. Ignored during the day

### RelationshipSystem

Owns every relationship (a `RelationshipGraph`) and is the only place that changes affinity or status. **Every method comes in two forms: with one hero, it means "with the player hero"** (all the prototype needs), and with two heroes, it's between any pair.

| With the player hero | Between two heroes | Notes |
| --- | --- | --- |
| `int GetAffinity(hero)` | `GetAffinity(hero, other)` | 0 if they've never interacted |
| `RelationshipStatus GetStatus(hero)` | `GetStatus(hero, other)` | |
| `RelationshipTier GetTier(hero)` | `GetTier(hero, other)` | Calculated from affinity |
| `void AddAffinity(hero, amount)` | `AddAffinity(hero, other, amount)` | Clamps 0 to 100. Fires OnAffinityChanged, OnTierChanged when the tier changes. A Dating pair falling below 40 becomes Ex (fires OnStatusChanged) |
| `bool CanAskOut(hero)` | `CanAskOut(hero, other)` | Affinity above 60, no status yet (Ex and Widowed can't restart for now), both under their dating cap |
| `bool TryStartDating(hero)` | `TryStartDating(hero, other)` | Sets Dating if `CanAskOut`; fires OnStatusChanged |

- `int GetStatBonus(HeroData hero)`: flat battle bonus from the hero's tier with the player; 0 for the player hero
- `int GetDatingCap(HeroData hero)`: `DefaultDatingCap` (1), or the highest `DatingCapOverride` among the hero's traits
- `IReadOnlyList<RelationshipData> Relationships`, `GetRelationshipsOf(HeroData hero)`: for UI that shows the relationship web
- Deaths are automatic: it listens to `RosterManager.OnHeroFell`, and the fallen hero's Dating partners become Widowed

### DialogueScreen

- `void Open(HeroData hero)`: loads the conversation for the hero's class and shows the Dialogue screen

### BalanceConfig (ScriptableObject, `Assets/Data/Config/`)

All tuning numbers live here, never hardcoded in scripts.

| Group | Values |
| --- | --- |
| Run | `StartingGold` 100, `StartingUndoTokens` 1 |
| Adventures | `Adventures`: ordered list of AdventureData, last = boss. `GetAdventureIndex(adventure)` returns its position or -1 |
| Player hero | `DefaultPlayerName`, `DefaultPlayerClass` (until class pick exists) |
| Roster and party | `RosterCapacity` 50 (every hero in the Adventurer Guild, including the player; 0 = unlimited), `PartySize` 6 (most heroes sent on one adventure, including the player hero) |
| Day cycle | `RecruitCardsPerNight` 5, `TalkCandidatesPerDay` 2 |
| Heroes | `MaxLevel` 10, one `ClassStatProfile` per class, XP table |
| Hero colors | `HeroColorSaturation` 0.6, `HeroColorValue` 0.9 (every hero gets a random hue at this saturation and brightness) |
| Relationships | `MaxAffinity` 100 (const), tier thresholds 0 / 25 / 50 / 75, tier stat bonus 0 / 1 / 2 / 4, `AffinityPerTalk` 5, `AffinityPerBattle` 3, `AskOutAffinityThreshold` 60, `BreakupAffinityThreshold` 40, `DefaultDatingCap` 1 |
| Traits | `TraitCardsPerLevelUp` 6, `TraitPool` |
| Rewards | `GoldRewardMultiplier`, `XpRewardMultiplier` |
| Shop | `ShopItems` |

- `ClassStatProfile GetClassStats(HeroClass heroClass)`: throws `KeyNotFoundException` if missing
- `int GetXpToNextLevel(int level)`: `int.MaxValue` at max level
- `RelationshipTier GetTierForAffinity(int affinity)`, `int GetTierStatBonus(RelationshipTier tier)`
- `bool CanAskOut(int affinity)` (above 60), `bool ShouldBreakUp(int affinity)` (below 40)

## Shared UI pieces

Reusable components any screen prefab can use.

- **ScreenBase** (MonoBehaviour): base class for every screen. Set its `Id` (ScreenId) in the Inspector; ScreenManager finds all ScreenBase children of `Canvas/Screens` on Awake. Each screen script derives from it (`TownScreen : ScreenBase`). A screen with no logic can use ScreenBase directly.
- **NavButton** (requires Button): pick a target `ScreenId` in the Inspector, or tick `Go Back` to call `ScreenManager.Back()`. Use this for plain navigation instead of writing a script.
- **DayCycleButton** (requires Button): pick `EndDay` or `StartNextDay` in the Inspector.
- **HeroPortrait** (requires Image; prefab `Prefabs/UI/HeroUI/HeroPortrait`): `void Show(HeroData hero)` draws the hero's class shape from `ClassVisuals`, tinted with `hero.Color`; null hides it. Tick `Use Portrait` for the large art. Use this prefab anywhere a hero is drawn (cards, battle, dialogue) instead of building your own.
- **HeroDetailPanel** (prefab `Prefabs/UI/HeroUI/Panels/HeroDetailPanel`): read-only view of one hero (portrait, level and class, HP, stats, gear, traits, affinity with the player). `void Show(HeroData hero)`, `void Hide()`, `HeroData Hero { get; }`. Refreshes itself on `OnHeroUpdated`. Works for heroes not in the roster yet (affinity reads 0)
- **RosterSlot** (requires Button; prefab `Prefabs/UI/HeroUI/RosterSlot`): one roster tile. `void Bind(HeroData hero, Action<HeroData> onClick)`, `void Refresh()`, `void SetSelected(bool selected)`, `HeroData Hero { get; }`

## Events

All events are C# `event System.Action<...>` on the owning class. Listeners subscribe in `OnEnable` and unsubscribe in `OnDisable`.

| Event | Fired by | Payload | Listened to by |
| --- | --- | --- | --- |
| `OnGoldChanged` | GameManager | int newGold | HUD, Shop |
| `OnUndoTokensChanged` | GameManager | int newCount | HUD, Recruit |
| `OnAdventuresChanged` | GameManager | int completed | HUD, AdventureSelect (refresh locks) |
| `OnRunEnded` | GameManager | bool won | Screens that need to react to a run ending (e.g. go to Summary) |
| `OnNewGame` | GameManager | none | All screens (reset their views), ScreenManager (reset AdventureContext) |
| `OnRosterChanged` | RosterManager | none | Roster, AdventureSelect, HUD, Town |
| `OnHeroFell` | RosterManager | HeroData | Relationships (Dating becomes Widowed), Summary, any UI showing the fallen |
| `OnHeroUpdated` | RosterManager | HeroData | Roster tiles, HeroDetailPanel (stats, level, equipment, affinity changed) |
| `OnInventoryChanged` | Inventory | none | Shop, Inspection |
| `OnAffinityChanged` | RelationshipSystem | HeroData hero, HeroData other, int newAffinity | Inspection affinity meter, Dialogue |
| `OnTierChanged` | RelationshipSystem | HeroData hero, HeroData other, RelationshipTier | Dialogue (tier-up message) |
| `OnStatusChanged` | RelationshipSystem | HeroData hero, HeroData other, RelationshipStatus | Dialogue (dating events), Inspection, relationship web |
| `OnScreenChanged` | ScreenManager | ScreenId from, ScreenId to | HUD (hide/show), screens (refresh on open) |
| `OnPhaseChanged` | DayCycle | int day, bool isNight | HUD day label, Town |
| `OnDialogueFinished` | DialogueScreen | HeroData | Town (refresh talk buttons) |

## Screen and feature dependencies

This is what each screen or feature reads, calls and listens to. If something you need is in another row's "Provides" column, code against the stub and it will just work later.

| Screen / feature | Reads | Calls | Listens to | Provides to others |
| --- | --- | --- | --- | --- |
| HUD / UI bar | Gold, UndoTokens, AdventuresCompleted, Day, IsNight | `ScreenManager.Show` (quick nav) | OnGoldChanged, OnUndoTokensChanged, OnAdventuresChanged, OnScreenChanged, OnPhaseChanged | Always-on status display |
| Town Hub | AdventuresCompleted, `DayCycle.Day`, `RosterManager.Heroes` (talk candidates, not the player hero) | `ScreenManager.Show`, `DialogueScreen.Open` | OnAdventuresChanged, OnPhaseChanged, OnRosterChanged, OnDialogueFinished | Entry point to every daytime feature, talk entry point |
| Recruitment Swipe (night) | UndoTokens, `RosterManager.IsFull`, `Config.RecruitCardsPerNight` | `HeroGenerator.Generate`, `TryAddHero`, `TryUseUndoToken`, `DayCycle.StartNextDay` | OnUndoTokensChanged | New HeroData in roster |
| Roster | `RosterManager.Heroes`, `PlayerHero`, `GetById` | `HeroDetailPanel.Show` for the clicked tile (player hero by default) | OnRosterChanged, OnHeroUpdated | Nothing yet |
| Hero detail panel (on Roster; reused by Recruit's swipe-up inspect) | HeroData, `RelationshipSystem.GetAffinity`, `GetTier`, `GetStatus`, `ClassVisuals` | none (read-only for now; equip via `Inventory.UseOn` is planned) | OnHeroUpdated | Reusable hero view |
| Affinity meter (prefab) | `RelationshipSystem.GetAffinity(hero)`, `GetTier(hero)`, `GetStatus(hero)` | none | OnAffinityChanged, OnStatusChanged | Reusable meter for Inspection and Dialogue |
| Dialogue (VN) | HeroData (Name, Class), `RelationshipSystem.GetAffinity(hero)`, `GetStatus(hero)`, `CanAskOut(hero)` | `RelationshipSystem.AddAffinity(hero, Config.AffinityPerTalk)`, `TryStartDating(hero)`, `DayCycle.EndDay` | OnTierChanged, OnStatusChanged | OnDialogueFinished |
| Relationship system | RelationshipGraph, BalanceConfig, hero traits | `RosterManager.NotifyHeroUpdated` | RosterManager.OnHeroFell | Affinity, tiers, dating, battle stat bonus |
| Shop | Gold, `Config.ShopItems` | `TrySpendGold`, `Inventory.Add` | OnGoldChanged, OnInventoryChanged | Items in inventory |
| Adventure Select | `Config.Adventures`, `IsUnlocked`, `IsCleared`, `RosterManager.Heroes`, `Config.PartySize` (player hero always included) | `AdventureContext.Begin`, `ScreenManager.Show(Battle)` | OnRosterChanged, OnAdventuresChanged | Party + AdventureData in AdventureContext |
| Battle | `AdventureContext` (Party, Adventure and its Encounters), `RelationshipSystem.GetStatBonus`, equipped items | `AdventureContext.RecordBattle` (each fight), `AdventureContext.Finish`, `GameManager.EndRun(false)` if the player hero dies | none | BattleResult per fight, AdventureResult |
| Rewards | `AdventureContext.LastResult` (an AdventureResult) | `AddGold`, `AddUndoToken`, `Inventory.Add`, `AddAffinity` (`Config.AffinityPerBattle`), `MarkFallen` (each hero in `Fallen`), `RestoreFullHp` (each survivor), `CompleteAdventure(adventure)` if won, then `Show(Summary)` if `IsRunOver`, else `DayCycle.EndDay` | none | Updated heroes, gold, items |
| Summary | Roster, `FallenHeroes`, Gold, `RunWon`, `AdventureContext.History` | `GameManager.NewGame`, `ScreenManager.Show(Town)` | none | Restart |

**Hand-offs between screens:** Town to Dialogue passes the hero through `ScreenManager.SelectedHero`. Roster doesn't need it: inspection is a panel on the same screen. Adventure Select to Battle to Rewards pass the party, adventure and result through `ScreenManager.Adventure` (an `AdventureContext`).

## Stubs and placeholder data

| Class | Behavior now |
| --- | --- |
| GameManager | Real |
| RosterManager | Real (capacity 50, set in BalanceConfig) |
| HeroGenerator | Real (random class and name, stats from BalanceConfig at the player's level) |
| Inventory | Real (equip with class locks, potions, gifts) |
| RelationshipSystem | Real: affinity, tiers, asking out, dating caps (with trait overrides), breakups to Ex, Widowed on death. Not yet: Charmer's affinity multiplier, Ex party debuffs, NPC-to-NPC affinity sources |
| ScreenManager | Real: enables one panel, disables the rest; SelectedHero and AdventureContext not added yet |
| DayCycle | Real |
| Roster | Real: grid of every living hero plus a read-only detail panel. No equip or dismiss yet |
| Town | Talk buttons hidden on day 1, shown from day 2 (until it picks up to `TalkCandidatesPerDay` random NPC heroes) |
| Recruit (`RecruitStubScreen`) | Pass and Recruit both just advance a counter; after 5 cards calls `StartNextDay` |
| DialogueScreen | Shows one hardcoded line and a Close button |
| Battle | Goes to Rewards after 1 second (no results recorded yet) |

**DebugSeed** (component on the Managers object, only runs in the Editor):

- On Play, after `NewGame` has created the player hero, adds one NPC per relationship tier, with that affinity toward the player hero (set through `RelationshipSystem.AddAffinity`): Warrior (0), Mage (30), Rogue (65, can be asked out), Healer (90). Editable in the Inspector
- Adds any item assets dragged into its list (e.g. one of each ItemType)
- Skip-to-screen toggle: added once screen-manager is merged, since `ScreenId` isn't on Core-Data yet

## Conventions

Details and reasons in `DevPractices.md`.

**Code**

- PascalCase for classes, methods, properties and events; `_camelCase` for private fields; global namespace
- Braces on their own line
- Events start with `On`; methods that can fail start with `Try` and return bool
- Game logic (battle, rewards, generation) in plain C# classes; MonoBehaviours only for screens and managers
- Every screen script derives from `ScreenBase`; plain navigation uses `NavButton` instead of custom code
- No magic numbers in scripts: tuning values go in BalanceConfig
- Randomness: pass a `System.Random` in, don't call `UnityEngine.Random`
- ScriptableObject assets are read-only at runtime

**Folders**

- `Assets/Scripts/Core` (managers) and `Assets/Scripts/Core/Data` (assembly `SwipeStory.Data`: enums, data classes, ScriptableObject definitions; no references)
- `Assets/Scripts/Screens` (one script per screen, plus `ScreenBase`)
- `Assets/Scripts/Systems` (RelationshipSystem, battle, rewards)
- `Assets/Scripts/UI` (shared widget scripts: NavButton, DayCycleButton, DayLabel)
- `Assets/Scripts/Debug` (DebugSeed)
- `Assets/Tests/EditMode` (assembly `SwipeStory.Tests.EditMode`)
- `Assets/Prefabs/Screens`, `Assets/Prefabs/UI` (shared widgets like hero card, stat bar, affinity meter)
- `Assets/Data/Config`, `Assets/Data/Items`, `Assets/Data/Traits`, `Assets/Data/Adventures`, `Assets/Data/Dialogue` (ScriptableObject assets)

**Scene and git**

- One scene (`Main.unity`). Each screen is its own prefab; edit the prefab, not the scene
- Only one person edits `Main.unity` at a time; say so in chat before touching it
- One branch per task (`T15-swipe-input`), PR into `main`, the other person reviews
- Pull before starting work; commit small and often

## Open questions

- [x] ~~Roster capacity and party size~~ Party: up to 6 heroes sent on an adventure. Roster: every hero in the Adventurer Guild, up to 50
- [x] ~~Does the player hero have to be in every party?~~ Yes, for now
- [x] ~~Can a hero die or leave the roster after a lost battle?~~ NPC death is permanent; player death ends the run
- [x] ~~Does love score ever go down?~~ Yes (breakups need it); what lowers it is still open
- [x] ~~Can each hero be talked to once per round, or unlimited times?~~ One talk per day (talking is the day's activity)
- [x] ~~How many swipe cards per visit to the Recruit screen, and does the deck refresh after each adventure?~~ 5 new cards every night, free
- [x] ~~Is there a protagonist hero in the roster from day 1?~~ Yes, the player hero (`IsPlayer`); can't be removed or talked to
- [x] ~~Can a lost adventure be retried?~~ Yes, and cleared adventures can be replayed; only a first win unlocks the next one
- [x] ~~Does the run end after 3 adventures, or after a fixed number of days?~~ Won by clearing the boss (adventure 3), lost when the player hero dies. No day limit
- [ ] Battle side: does HP carry over between encounters? Do rewards change on replays?
- [ ] Can the player leave Dialogue without ending the day?
- [ ] Should `GameManager.NewGame` also reset `DayCycle` to day 1, daytime? (needs `DayCycle.Reset()`)
- [ ] Does the player hero draw traits on level-up?
