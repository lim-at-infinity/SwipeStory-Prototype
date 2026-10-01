using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class RosterManager : MonoBehaviour
{
    public static RosterManager Instance { get; private set; }

    public event Action OnRosterChanged;
    public event Action<HeroData> OnHeroUpdated;
    public event Action<HeroData> OnHeroFell;

    private readonly List<HeroData> _heroes = new List<HeroData>();
    private readonly List<HeroData> _fallen = new List<HeroData>();

    public IReadOnlyList<HeroData> Heroes => _heroes;
    public HeroData PlayerHero { get; private set; }

    // NPC heroes who died this run, oldest first. Kept so history (e.g. Widowed relationships, Summary) can show them
    public IReadOnlyList<HeroData> FallenHeroes => _fallen;

    // Includes the player hero. 0 = unlimited
    public int Capacity => GameManager.Instance.Config.RosterCapacity;
    public bool IsFull => Capacity > 0 && _heroes.Count >= Capacity;

    private void Awake()
    {
        Instance = this;
    }

    public bool TryAddHero(HeroData hero)
    {
        if (hero == null || _heroes.Contains(hero) || IsFull)
        {
            return false;
        }

        if (hero.IsPlayer)
        {
            if (PlayerHero != null)
            {
                Debug.LogWarning("[RosterManager] The roster already has a player hero.");
                return false;
            }

            PlayerHero = hero;
        }

        _heroes.Add(hero);
        OnRosterChanged?.Invoke();
        return true;
    }

    // The player hero can't be removed; their death ends the run instead
    public void RemoveHero(HeroData hero)
    {
        if (hero == null || hero.IsPlayer)
        {
            return;
        }

        if (_heroes.Remove(hero))
        {
            OnRosterChanged?.Invoke();
        }
    }

    // NPC death: moves the hero from the roster to FallenHeroes. Their equipped Weapon and Hat are lost with them.
    // The player hero's death ends the run instead (GameManager.EndRun(false)), so it is ignored here
    public void MarkFallen(HeroData hero)
    {
        if (hero == null || hero.IsPlayer || !_heroes.Remove(hero))
        {
            return;
        }

        _fallen.Add(hero);
        OnHeroFell?.Invoke(hero);
        OnRosterChanged?.Invoke();
    }

    // Living heroes only
    public HeroData GetById(string id)
    {
        return _heroes.Find(hero => hero.Id == id);
    }

    public HeroData GetFallenById(string id)
    {
        return _fallen.Find(hero => hero.Id == id);
    }

    public void NotifyHeroUpdated(HeroData hero)
    {
        OnHeroUpdated?.Invoke(hero);
    }

    // Only GameManager.NewGame should call this
    internal void Clear()
    {
        _heroes.Clear();
        _fallen.Clear();
        PlayerHero = null;
        OnRosterChanged?.Invoke();
    }
}
