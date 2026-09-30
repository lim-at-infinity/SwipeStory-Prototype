using System;
using UnityEngine;

// The only place that changes HeroData.Affinity.
// Not yet: trait effects (Charmer), dating, breakups, Ex debuffs
[DefaultExecutionOrder(-200)]
public class RelationshipSystem : MonoBehaviour
{
    public static RelationshipSystem Instance { get; private set; }

    public event Action<HeroData, int> OnAffinityChanged;            // (hero, newAffinity)
    public event Action<HeroData, RelationshipTier> OnTierChanged;   // (hero, newTier)

    private BalanceConfig Config => GameManager.Instance.Config;

    private void Awake()
    {
        Instance = this;
    }

    // Clamps to 0..MaxAffinity. Ignored for the player hero
    public void AddAffinity(HeroData hero, int amount)
    {
        if (hero == null || hero.IsPlayer || amount == 0)
        {
            return;
        }

        int newAffinity = Mathf.Clamp(hero.Affinity + amount, 0, BalanceConfig.MaxAffinity);
        if (newAffinity == hero.Affinity)
        {
            return;
        }

        RelationshipTier oldTier = GetTier(hero);
        hero.Affinity = newAffinity;
        OnAffinityChanged?.Invoke(hero, newAffinity);

        RelationshipTier newTier = GetTier(hero);
        if (newTier != oldTier)
        {
            OnTierChanged?.Invoke(hero, newTier);
        }

        RosterManager.Instance.NotifyHeroUpdated(hero);
    }

    public RelationshipTier GetTier(HeroData hero)
    {
        return Config.GetTierForAffinity(hero.Affinity);
    }

    // Flat battle bonus for the hero's tier
    public int GetStatBonus(HeroData hero)
    {
        return Config.GetTierStatBonus(GetTier(hero));
    }
}
