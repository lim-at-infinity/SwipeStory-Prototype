using System;
using UnityEngine;

// Builds heroes from BalanceConfig. Plain logic so it can be tested without a scene
public static class HeroFactory
{
    // Stats = level 1 roll + growth per level gained. Level is clamped to 1..MaxLevel
    public static HeroData Create(BalanceConfig config, System.Random rng, string name, HeroClass heroClass, int level, bool isPlayer = false)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (rng == null)
        {
            throw new ArgumentNullException(nameof(rng));
        }

        level = Mathf.Clamp(level, 1, config.MaxLevel);
        int levelsGained = level - 1;
        ClassStatProfile profile = config.GetClassStats(heroClass);

        HeroData hero = new HeroData(name, heroClass, isPlayer)
        {
            Level = level,
            MaxHp = profile.MaxHp.Roll(rng) + profile.MaxHpPerLevel * levelsGained,
            Attack = profile.Attack.Roll(rng) + profile.AttackPerLevel * levelsGained,
            Defense = profile.Defense.Roll(rng) + profile.DefensePerLevel * levelsGained,
            Speed = profile.Speed.Roll(rng) + profile.SpeedPerLevel * levelsGained
        };

        hero.RestoreFullHp();
        return hero;
    }
}
