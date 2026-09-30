using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class RosterManager : MonoBehaviour
{
    public static RosterManager Instance { get; private set; }

    public event Action OnRosterChanged;
    public event Action<HeroData> OnHeroUpdated;

    private readonly List<HeroData> _heroes = new List<HeroData>();

    public IReadOnlyList<HeroData> Heroes => _heroes;
    public HeroData PlayerHero { get; private set; }

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

    public HeroData GetById(string id)
    {
        return _heroes.Find(hero => hero.Id == id);
    }

    public void NotifyHeroUpdated(HeroData hero)
    {
        OnHeroUpdated?.Invoke(hero);
    }

    // Only GameManager.NewGame should call this
    internal void Clear()
    {
        _heroes.Clear();
        PlayerHero = null;
        OnRosterChanged?.Invoke();
    }
}
