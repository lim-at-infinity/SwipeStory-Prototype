using System;
using System.Collections.Generic;

// Outcome of one fight (one EncounterData inside an adventure). Immutable
public class BattleResult
{
    public EncounterData Encounter { get; }
    public bool Won { get; }
    public IReadOnlyList<HeroData> Fallen { get; }

    public BattleResult(EncounterData encounter, bool won, IEnumerable<HeroData> fallen)
    {
        Encounter = encounter;
        Won = won;
        Fallen = new List<HeroData>(fallen ?? Array.Empty<HeroData>()).AsReadOnly();
    }
}