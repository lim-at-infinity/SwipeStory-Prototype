using System;
using System.Collections.Generic;

// Every relationship between two heroes, player included. One record per pair, created on first contact:
// a missing record means affinity 0 and no status. Pairs are stored in a fixed order, so (A, B) and (B, A) are the same record
public class RelationshipGraph
{
    private readonly List<RelationshipData> _relationships = new List<RelationshipData>();

    public IReadOnlyList<RelationshipData> All => _relationships;

    // Null if the two have no relationship yet
    public RelationshipData Get(string heroId, string otherId)
    {
        Order(ref heroId, ref otherId);
        return _relationships.Find(r => r.HeroAId == heroId && r.HeroBId == otherId);
    }

    public RelationshipData GetOrCreate(string heroId, string otherId)
    {
        if (string.IsNullOrEmpty(heroId) || string.IsNullOrEmpty(otherId))
        {
            throw new ArgumentException("Both hero Ids are required.");
        }

        if (heroId == otherId)
        {
            throw new ArgumentException("A hero can't have a relationship with themselves.");
        }

        RelationshipData relationship = Get(heroId, otherId);
        if (relationship == null)
        {
            Order(ref heroId, ref otherId);
            relationship = new RelationshipData(heroId, otherId);
            _relationships.Add(relationship);
        }

        return relationship;
    }

    public int GetAffinity(string heroId, string otherId)
    {
        return Get(heroId, otherId)?.Affinity ?? 0;
    }

    public RelationshipStatus GetStatus(string heroId, string otherId)
    {
        return Get(heroId, otherId)?.Status ?? RelationshipStatus.None;
    }

    public IReadOnlyList<RelationshipData> GetRelationshipsOf(string heroId)
    {
        return _relationships.FindAll(r => r.Involves(heroId));
    }

    // e.g. how many partners a hero is Dating, for the dating cap
    public int CountWithStatus(string heroId, RelationshipStatus status)
    {
        int count = 0;
        foreach (RelationshipData relationship in _relationships)
        {
            if (relationship.Status == status && relationship.Involves(heroId))
            {
                count++;
            }
        }

        return count;
    }

    // Dating partners become Widowed. Records with a status (Widowed, Ex) are kept as history; the rest are removed
    public void HandleDeath(string heroId)
    {
        foreach (RelationshipData relationship in _relationships)
        {
            if (relationship.Involves(heroId) && relationship.Status == RelationshipStatus.Dating)
            {
                relationship.Status = RelationshipStatus.Widowed;
            }
        }

        _relationships.RemoveAll(r => r.Involves(heroId) && r.Status == RelationshipStatus.None);
    }

    public void Clear()
    {
        _relationships.Clear();
    }

    // Puts the two Ids in a fixed order, so either order finds the same record
    private static void Order(ref string heroId, ref string otherId)
    {
        if (string.CompareOrdinal(heroId, otherId) > 0)
        {
            string temp = heroId;
            heroId = otherId;
            otherId = temp;
        }
    }
}