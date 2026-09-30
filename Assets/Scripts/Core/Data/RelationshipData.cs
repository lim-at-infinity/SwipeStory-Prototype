using System;
using UnityEngine;

// One relationship between two heroes (the player hero included). Mutual: one affinity and one status for the pair.
// Created and stored by RelationshipGraph; change Affinity and Status only through RelationshipSystem
[Serializable]
public class RelationshipData
{
    [field: SerializeField] public string HeroAId { get; private set; }
    [field: SerializeField] public string HeroBId { get; private set; }

    // 0 to BalanceConfig.MaxAffinity
    [field: SerializeField] public int Affinity { get; set; }
    [field: SerializeField] public RelationshipStatus Status { get; set; }

    public RelationshipData(string heroAId, string heroBId)
    {
        HeroAId = heroAId;
        HeroBId = heroBId;
    }

    public bool Involves(string heroId)
    {
        return HeroAId == heroId || HeroBId == heroId;
    }

    // The other side of the pair, from one hero's point of view
    public string GetOtherId(string heroId)
    {
        return heroId == HeroAId ? HeroBId : HeroAId;
    }
}