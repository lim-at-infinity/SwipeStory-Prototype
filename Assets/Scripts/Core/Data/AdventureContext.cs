using System;
using System.Collections.Generic;

// Carries the current adventure from AdventureSelect through Battle to Rewards, plus the run's result history.
// Lives on ScreenManager (screen-manager branch); reset on GameManager.OnNewGame
public class AdventureContext
{
    private readonly List<HeroData> _party = new List<HeroData>();
    private readonly List<BattleResult> _battles = new List<BattleResult>();
    private readonly List<AdventureResult> _history = new List<AdventureResult>();

    public AdventureData Adventure { get; private set; }
    public IReadOnlyList<HeroData> Party => _party;

    // Fights finished so far in the current adventure
    public IReadOnlyList<BattleResult> Battles => _battles;

    // Which encounter is fought next (0 = first)
    public int CurrentEncounterIndex => _battles.Count;

    // Count only; Battle decides whether to stop early after a loss
    public bool HasMoreEncounters => Adventure != null && _battles.Count < Adventure.Encounters.Count;

    public AdventureResult LastResult { get; private set; }
    public IReadOnlyList<AdventureResult> History => _history;

    // Called by AdventureSelect before showing Battle
    public void Begin(AdventureData adventure, IEnumerable<HeroData> party)
    {
        if (adventure == null)
        {
            throw new ArgumentNullException(nameof(adventure));
        }

        if (party == null)
        {
            throw new ArgumentNullException(nameof(party));
        }

        Adventure = adventure;
        _party.Clear();
        _party.AddRange(party);
        _battles.Clear();
        LastResult = null;
    }

    // Called by Battle after each fight
    public void RecordBattle(BattleResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (Adventure == null)
        {
            throw new InvalidOperationException("RecordBattle called before Begin.");
        }

        _battles.Add(result);
    }

    // Called by Battle after the last fight, or after a loss, before showing Rewards
    public void Finish(AdventureResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        LastResult = result;
        _history.Add(result);
    }

    public void Reset()
    {
        Adventure = null;
        _party.Clear();
        _battles.Clear();
        LastResult = null;
        _history.Clear();
    }
}