# Heroes

## HeroData

One hero's runtime state: identity (`Id`, `Name`, `Class`, `IsPlayer`), `Level` and `Xp`, stats (`MaxHp`, `CurrentHp`, `Attack`, `Defense`, `Speed`), equipment (`Weapon`, `Hat`) and `Traits`.

- It's a class, so everything holding a hero (roster, party, selected hero) shares the same object. Change it once, everyone sees it.
- `Id` is a GUID made in the constructor and never changes. Other systems store Ids, not references, when they need to outlive the hero (relationships, saves).
- **HP resets after every adventure.** Surviving heroes are restored to full HP (`RestoreFullHp()`) when the party returns; Rewards does this. Whether HP also resets between encounters inside one adventure is up to the battle side.
- **Base stats never include bonuses.** Equipment and relationship tier bonuses are added by battle at fight time, so equipping never edits `Attack`.
- Affinity and relationship status are **not** here. See `Relationships.md`.
- After changing a hero, call `RosterManager.NotifyHeroUpdated(hero)` so screens refresh.

## Making heroes

`HeroFactory.Create(config, rng, name, class, level)` builds a hero. The core of it (`HeroFactory.cs`):

```csharp
level = Mathf.Clamp(level, 1, config.MaxLevel);
int levelsGained = level - 1;
ClassStatProfile profile = config.GetClassStats(heroClass);   // one profile per class in BalanceConfig

HeroData hero = new HeroData(name, heroClass, isPlayer)
{
    Level = level,
    // roll the class's level 1 range, then add flat growth for every level above 1
    MaxHp = profile.MaxHp.Roll(rng) + profile.MaxHpPerLevel * levelsGained,
    Attack = profile.Attack.Roll(rng) + profile.AttackPerLevel * levelsGained,
    Defense = profile.Defense.Roll(rng) + profile.DefensePerLevel * levelsGained,
    Speed = profile.Speed.Roll(rng) + profile.SpeedPerLevel * levelsGained
};

hero.RestoreFullHp();   // new heroes start at full HP
```

`HeroGenerator` wraps it with a random class and name:
- `Generate()`: a recruit **at the player hero's level**, so recruits keep up.
- `CreatePlayer(class, name)`: the level 1 player hero (used by `NewGame`).
- Its `Seed` field (0 = random) repeats the same recruits every run, handy for debugging.

## Roster

`RosterManager` holds the Adventurer Guild.

| Member | Behavior |
| --- | --- |
| `Heroes` | Everyone alive, player included. Capacity from `BalanceConfig.RosterCapacity` (0 = unlimited) |
| `PlayerHero` | Exactly one per run. Can't be removed |
| `TryAddHero` | Refuses null, duplicates, a full roster, or a second player hero |
| `RemoveHero` | Non-death removal (e.g. dismissing). Ignored for the player hero |
| `MarkFallen` | NPC death: moves the hero to `FallenHeroes`, fires `OnHeroFell`. **Their equipment is lost.** Player death ends the run instead (`GameManager.EndRun(false)`) |
| `GetById` / `GetFallenById` | Lookups for living / fallen heroes |

The party is not stored here: a party is chosen per adventure (see `Adventures.md`).

## Script reference

### HeroClass.cs

Enum `HeroClass`: Warrior = 0, Mage = 1, Rogue = 2, Healer = 3. Picks a hero's stat profile and which weapons they can use. Saved as numbers, so only ever append new values.

### HeroData.cs (plain class)

| Member | What it does |
| --- | --- |
| `Id` | Unique GUID created with the hero. Never changes; other systems use it to refer to the hero. |
| `Name`, `Class`, `IsPlayer` | Display name, class, and whether this is the player's own hero (one per run). |
| `Level`, `Xp` | Progress. Level starts at 1; leveling up isn't built yet. |
| `MaxHp`, `CurrentHp`, `Attack`, `Defense`, `Speed` | Base stats, without equipment or relationship bonuses. |
| `Weapon`, `Hat` | The equipped item assets, or null when empty. |
| `Traits` | Read-only list of the hero's traits. |
| `HeroData(name, heroClass, isPlayer)` | Creates a hero with a new Id. Stats start at 0, so use `HeroFactory` to make a real one. |
| `RestoreFullHp()` | Sets `CurrentHp` to `MaxHp`. Called for survivors when the party returns from an adventure. |
| `GetEquipped(slot)` | Returns the item in the Weapon or Hat slot, or null. |
| `SetEquipped(slot, item)` | Puts an item in a slot, or empties it when `item` is null. Throws if the item doesn't fit the slot; class locks are checked by `Inventory.UseOn`, not here. |
| `AddTrait(trait)` | Adds a trait. Null is ignored; duplicates are currently allowed. |
| `HasTrait(trait)` | True if the hero has that trait asset. |

### HeroFactory.cs (static)

| Member | What it does |
| --- | --- |
| `Create(config, rng, name, heroClass, level, isPlayer)` | Builds a complete hero: rolls each stat from the class's level 1 range, adds growth for every level above 1, and starts at full HP. Level is clamped to 1..MaxLevel; throws if `config` or `rng` is null. |

### HeroGenerator.cs (manager)

| Member | What it does |
| --- | --- |
| `Generate()` | Makes a random recruit (random class and name) at the player hero's current level. Used by Recruit. |
| `CreatePlayer(heroClass, name)` | Makes the level 1 player hero. Called by `GameManager.NewGame`. |
| Inspector `Seed` | 0 gives different recruits every run; any other number repeats the same sequence, useful for reproducing bugs. |
| Inspector `Names` | The pool recruit names are picked from (repeats are possible). |

### RosterManager.cs (manager)

| Member | What it does |
| --- | --- |
| `Heroes` | Every living hero, player included. |
| `PlayerHero` | The player's own hero. |
| `FallenHeroes` | NPC heroes who died this run, oldest first. |
| `Capacity`, `IsFull` | Roster limit from BalanceConfig (0 = unlimited), and whether it's been reached. |
| `TryAddHero(hero)` | Adds a hero and fires `OnRosterChanged`. Returns false for null, a hero already in the roster, a full roster, or a second player hero. |
| `RemoveHero(hero)` | Removes a hero for non-death reasons (e.g. dismissing). Ignored for the player hero. |
| `MarkFallen(hero)` | NPC death: moves the hero from `Heroes` to `FallenHeroes` (their gear is lost) and fires `OnHeroFell` and `OnRosterChanged`. Ignored for the player hero, whose death ends the run instead. |
| `GetById(id)`, `GetFallenById(id)` | Finds a living / fallen hero by Id, or returns null. |
| `NotifyHeroUpdated(hero)` | Fires `OnHeroUpdated` so screens refresh. Call it after changing any hero field. |
| `Clear()` (internal) | Empties the roster and fallen list. Only `GameManager.NewGame` calls it. |
| Events | `OnRosterChanged`, `OnHeroUpdated`, `OnHeroFell`. |
