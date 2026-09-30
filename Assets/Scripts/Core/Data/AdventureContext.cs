using System;
using System.Collections.Generic;

// Carries the current adventure from AdventureSelect through Battle to Rewards, plus the run's result history.
// Lives on ScreenManager (screen-manager branch); reset on GameManager.OnNewGame
public class AdventureContext
{
    private readonly List<HeroData> _party = new List<HeroData>();
    private readonly List<BattleResult> _history = new List<BattleResult>();

    public AdventureData Adventure { get; private set; }
    public IReadOnlyList<HeroData> Party => _party;
    public BattleResult LastResult { get; private set; }
    public IReadOnlyList<BattleResult> History => _history;

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
        LastResult = null;
    }

    // Called by Battle before showing Rewards
    public void Finish(BattleResult result)
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
        LastResult = null;
        _history.Clear();
    }
}
