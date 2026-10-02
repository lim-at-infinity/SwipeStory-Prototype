using System;
using System.Collections.Generic;

// Plays out one fight instantly and writes what happened into a log. No Unity calls, so it is easy to test and replay.
// Each round every living unit acts once, fastest first. Damage = max(1, attack - defense).
// Heroes hit the enemy with the lowest HP; enemies hit a random hero.
// A Healer heals the most hurt ally below half HP (by its attack) instead of attacking.
public class BattleSimulator
{
    public const int MaxRounds = 30;

    private readonly Random _rng;

    public BattleSimulator(Random rng)
    {
        _rng = rng;
    }

    // Returns true if the heroes won. Heroes keep their CurrentHp, so the next fight starts where this one ended
    public bool Fight(List<BattleUnit> heroes, List<BattleUnit> enemies, List<BattleEvent> log)
    {
        for (int round = 1; round <= MaxRounds; round++)
        {
            log.Add(new BattleEvent("Round " + round));

            List<BattleUnit> order = new List<BattleUnit>();
            order.AddRange(heroes);
            order.AddRange(enemies);
            order.Sort((a, b) => b.Speed.CompareTo(a.Speed));

            foreach (BattleUnit unit in order)
            {
                if (!unit.IsAlive)
                {
                    continue;
                }

                if (unit.IsHero)
                {
                    HeroTurn(unit, heroes, enemies, log);
                }
                else
                {
                    Hit(unit, RandomAlive(heroes), log);
                }

                if (!AnyAlive(enemies))
                {
                    log.Add(new BattleEvent("Victory!"));
                    return true;
                }

                if (!AnyAlive(heroes))
                {
                    log.Add(new BattleEvent("Defeat..."));
                    return false;
                }
            }
        }

        log.Add(new BattleEvent("The fight dragged on too long. Retreat!"));
        return false;
    }

    private void HeroTurn(BattleUnit hero, List<BattleUnit> heroes, List<BattleUnit> enemies, List<BattleEvent> log)
    {
        if (hero.IsHealer)
        {
            BattleUnit hurt = MostHurt(heroes);
            if (hurt != null && hurt.CurrentHp * 2 < hurt.MaxHp)
            {
                int amount = Math.Min(hero.Attack, hurt.MaxHp - hurt.CurrentHp);
                hurt.CurrentHp += amount;
                log.Add(new BattleEvent(hero.Name + " heals " + hurt.Name + " for " + amount, hurt, hurt.CurrentHp));
                return;
            }
        }

        Hit(hero, LowestHp(enemies), log);
    }

    private static void Hit(BattleUnit attacker, BattleUnit target, List<BattleEvent> log)
    {
        int damage = Math.Max(1, attacker.Attack - target.Defense);
        target.CurrentHp = Math.Max(0, target.CurrentHp - damage);

        string text = attacker.Name + " hits " + target.Name + " for " + damage;
        if (!target.IsAlive)
        {
            text += ". " + target.Name + " falls!";
        }

        log.Add(new BattleEvent(text, target, target.CurrentHp));
    }

    // Lowest current HP among the living
    private static BattleUnit LowestHp(List<BattleUnit> units)
    {
        BattleUnit lowest = null;
        foreach (BattleUnit unit in units)
        {
            if (unit.IsAlive && (lowest == null || unit.CurrentHp < lowest.CurrentHp))
            {
                lowest = unit;
            }
        }

        return lowest;
    }

    // Lowest share of max HP among the living
    private static BattleUnit MostHurt(List<BattleUnit> units)
    {
        BattleUnit hurt = null;
        foreach (BattleUnit unit in units)
        {
            if (unit.IsAlive && (hurt == null || unit.CurrentHp * hurt.MaxHp < hurt.CurrentHp * unit.MaxHp))
            {
                hurt = unit;
            }
        }

        return hurt;
    }

    private BattleUnit RandomAlive(List<BattleUnit> units)
    {
        List<BattleUnit> alive = units.FindAll(unit => unit.IsAlive);
        return alive[_rng.Next(alive.Count)];
    }

    private static bool AnyAlive(List<BattleUnit> units)
    {
        foreach (BattleUnit unit in units)
        {
            if (unit.IsAlive)
            {
                return true;
            }
        }

        return false;
    }
}