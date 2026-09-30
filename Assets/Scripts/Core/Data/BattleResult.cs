using System;
using System.Collections.Generic;

// Outcome of one battle. Immutable; rewards are already scaled by BalanceConfig multipliers
public class BattleResult
{
    public bool Won { get; }
    public AdventureData Adventure { get; }
    public IReadOnlyList<HeroData> Party { get; }
    public IReadOnlyList<HeroData> Fallen { get; }
    public int GoldEarned { get; }
    public int XpEarned { get; }
    public int UndoTokensEarned { get; }
    public IReadOnlyList<ItemData> ItemsEarned { get; }

    public BattleResult(bool won, AdventureData adventure, IEnumerable<HeroData> party, IEnumerable<HeroData> fallen,
        int goldEarned, int xpEarned, int undoTokensEarned, IEnumerable<ItemData> itemsEarned)
    {
        Won = won;
        Adventure = adventure;
        Party = new List<HeroData>(party ?? Array.Empty<HeroData>()).AsReadOnly();
        Fallen = new List<HeroData>(fallen ?? Array.Empty<HeroData>()).AsReadOnly();
        GoldEarned = goldEarned;
        XpEarned = xpEarned;
        UndoTokensEarned = undoTokensEarned;
        ItemsEarned = new List<ItemData>(itemsEarned ?? Array.Empty<ItemData>()).AsReadOnly();
    }
}
