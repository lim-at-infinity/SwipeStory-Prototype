using System;
using System.Collections.Generic;
using UnityEngine;

// Owns every relationship between heroes (a RelationshipGraph) and is the only place that changes affinity or status.
// Methods without a second hero mean "with the player hero", which is all the prototype uses.
// Not yet: Charmer (AffinityGainMultiplier), Ex party debuffs, NPC-to-NPC affinity sources
[DefaultExecutionOrder(-200)]
public class RelationshipSystem : MonoBehaviour
{
    public static RelationshipSystem Instance { get; private set; }

    public event Action<HeroData, HeroData, int> OnAffinityChanged;                // (hero, other, newAffinity)
    public event Action<HeroData, HeroData, RelationshipTier> OnTierChanged;       // (hero, other, newTier)
    public event Action<HeroData, HeroData, RelationshipStatus> OnStatusChanged;   // (hero, other, newStatus)

    private readonly RelationshipGraph _graph = new RelationshipGraph();

    private BalanceConfig Config => GameManager.Instance.Config;
    private HeroData Player => RosterManager.Instance.PlayerHero;

    // Every relationship record, for UI that shows the whole web
    public IReadOnlyList<RelationshipData> Relationships => _graph.All;

    private void Awake()
    {
        Instance = this;
    }

    // Start, not OnEnable: RosterManager shares this execution order, so its Instance may not exist during OnEnable
    private void Start()
    {
        RosterManager.Instance.OnHeroFell += HandleHeroFell;
    }

    private void OnDestroy()
    {
        if (RosterManager.Instance != null)
        {
            RosterManager.Instance.OnHeroFell -= HandleHeroFell;
        }
    }

    // With the player hero

    public int GetAffinity(HeroData hero) => GetAffinity(hero, Player);
    public RelationshipStatus GetStatus(HeroData hero) => GetStatus(hero, Player);
    public RelationshipTier GetTier(HeroData hero) => GetTier(hero, Player);
    public void AddAffinity(HeroData hero, int amount) => AddAffinity(hero, Player, amount);
    public bool CanAskOut(HeroData hero) => CanAskOut(hero, Player);
    public bool TryStartDating(HeroData hero) => TryStartDating(hero, Player);

    // Flat battle bonus from the hero's tier with the player. 0 for the player hero
    public int GetStatBonus(HeroData hero)
    {
        if (hero == null || hero.IsPlayer)
        {
            return 0;
        }

        return Config.GetTierStatBonus(GetTier(hero));
    }

    // Between any two heroes

    public int GetAffinity(HeroData hero, HeroData other)
    {
        return IsPair(hero, other) ? _graph.GetAffinity(hero.Id, other.Id) : 0;
    }

    public RelationshipStatus GetStatus(HeroData hero, HeroData other)
    {
        return IsPair(hero, other) ? _graph.GetStatus(hero.Id, other.Id) : RelationshipStatus.None;
    }

    public RelationshipTier GetTier(HeroData hero, HeroData other)
    {
        return Config.GetTierForAffinity(GetAffinity(hero, other));
    }

    public IReadOnlyList<RelationshipData> GetRelationshipsOf(HeroData hero)
    {
        return hero == null ? Array.Empty<RelationshipData>() : _graph.GetRelationshipsOf(hero.Id);
    }

    // Clamps to 0..MaxAffinity. A Dating pair whose affinity drops below the breakup threshold becomes Ex
    public void AddAffinity(HeroData hero, HeroData other, int amount)
    {
        if (!IsPair(hero, other) || amount == 0)
        {
            return;
        }

        RelationshipData relationship = _graph.GetOrCreate(hero.Id, other.Id);
        int newAffinity = Mathf.Clamp(relationship.Affinity + amount, 0, BalanceConfig.MaxAffinity);
        if (newAffinity == relationship.Affinity)
        {
            return;
        }

        RelationshipTier oldTier = Config.GetTierForAffinity(relationship.Affinity);
        relationship.Affinity = newAffinity;
        OnAffinityChanged?.Invoke(hero, other, newAffinity);

        RelationshipTier newTier = Config.GetTierForAffinity(newAffinity);
        if (newTier != oldTier)
        {
            OnTierChanged?.Invoke(hero, other, newTier);
        }

        if (relationship.Status == RelationshipStatus.Dating && Config.ShouldBreakUp(newAffinity))
        {
            SetStatus(relationship, hero, other, RelationshipStatus.Ex);
        }

        NotifyBoth(hero, other);
    }

    // Affinity above the ask-out threshold, no status yet (Ex and Widowed can't restart, for now),
    // and both heroes under their dating cap
    public bool CanAskOut(HeroData hero, HeroData other)
    {
        if (!IsPair(hero, other))
        {
            return false;
        }

        RelationshipData relationship = _graph.Get(hero.Id, other.Id);
        return relationship != null
            && relationship.Status == RelationshipStatus.None
            && Config.CanAskOut(relationship.Affinity)
            && _graph.CountWithStatus(hero.Id, RelationshipStatus.Dating) < GetDatingCap(hero)
            && _graph.CountWithStatus(other.Id, RelationshipStatus.Dating) < GetDatingCap(other);
    }

    public bool TryStartDating(HeroData hero, HeroData other)
    {
        if (!CanAskOut(hero, other))
        {
            return false;
        }

        SetStatus(_graph.Get(hero.Id, other.Id), hero, other, RelationshipStatus.Dating);
        NotifyBoth(hero, other);
        return true;
    }

    // BalanceConfig.DefaultDatingCap, or the highest DatingCapOverride among the hero's traits
    public int GetDatingCap(HeroData hero)
    {
        int cap = Config.DefaultDatingCap;
        foreach (TraitData trait in hero.Traits)
        {
            if (trait != null && trait.DatingCapOverride > cap)
            {
                cap = trait.DatingCapOverride;
            }
        }

        return cap;
    }

    // Only GameManager.NewGame should call this
    internal void Clear()
    {
        _graph.Clear();
    }

    // Dating partners of the fallen hero become Widowed (the graph keeps those records as history)
    private void HandleHeroFell(HeroData fallen)
    {
        List<HeroData> widowed = new List<HeroData>();
        foreach (RelationshipData relationship in _graph.GetRelationshipsOf(fallen.Id))
        {
            if (relationship.Status == RelationshipStatus.Dating)
            {
                HeroData partner = RosterManager.Instance.GetById(relationship.GetOtherId(fallen.Id));
                if (partner != null)
                {
                    widowed.Add(partner);
                }
            }
        }

        _graph.HandleDeath(fallen.Id);

        foreach (HeroData partner in widowed)
        {
            OnStatusChanged?.Invoke(partner, fallen, RelationshipStatus.Widowed);
            RosterManager.Instance.NotifyHeroUpdated(partner);
        }
    }

    private void SetStatus(RelationshipData relationship, HeroData hero, HeroData other, RelationshipStatus status)
    {
        relationship.Status = status;
        OnStatusChanged?.Invoke(hero, other, status);
    }

    private static bool IsPair(HeroData hero, HeroData other)
    {
        return hero != null && other != null && hero != other;
    }

    private static void NotifyBoth(HeroData hero, HeroData other)
    {
        RosterManager.Instance.NotifyHeroUpdated(hero);
        RosterManager.Instance.NotifyHeroUpdated(other);
    }
}
