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
