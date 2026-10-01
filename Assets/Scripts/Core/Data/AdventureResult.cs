using System;
using System.Collections.Generic;

// Outcome of one whole adventure: every fight fought, plus the rewards (given once).
// Immutable; rewards are already scaled by BalanceConfig multipliers
public class AdventureResult
{
    public AdventureData Adventure { get; }
    public IReadOnlyList<HeroData> Party { get; }
    public IReadOnlyList<BattleResult> Battles { get; }
    public int GoldEarned { get; }
    public int XpEarned { get; }
    public int UndoTokensEarned { get; }
    public IReadOnlyList<ItemData> ItemsEarned { get; }

    // Won only if every encounter was fought and won
    public bool Won
    {
        get
        {
            if (Adventure == null || Battles.Count < Adventure.Encounters.Count)
            {
                return false;
            }

            foreach (BattleResult battle in Battles)
            {
                if (!battle.Won)
                {
                    return false;
                }
            }

            return true;
        }
    }

    // Everyone who fell in any of this adventure's battles
    public IReadOnlyList<HeroData> Fallen
    {
        get
        {
            List<HeroData> fallen = new List<HeroData>();
            foreach (BattleResult battle in Battles)
            {
                fallen.AddRange(battle.Fallen);
            }

            return fallen;
        }
    }

    public AdventureResult(AdventureData adventure, IEnumerable<HeroData> party, IEnumerable<BattleResult> battles,
        int goldEarned, int xpEarned, int undoTokensEarned, IEnumerable<ItemData> itemsEarned)
    {
        Adventure = adventure;
        Party = new List<HeroData>(party ?? Array.Empty<HeroData>()).AsReadOnly();
        Battles = new List<BattleResult>(battles ?? Array.Empty<BattleResult>()).AsReadOnly();
        GoldEarned = goldEarned;
        XpEarned = xpEarned;
        UndoTokensEarned = undoTokensEarned;
        ItemsEarned = new List<ItemData>(itemsEarned ?? Array.Empty<ItemData>()).AsReadOnly();
    }
}