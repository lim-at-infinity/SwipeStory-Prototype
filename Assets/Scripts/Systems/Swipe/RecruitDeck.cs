using System.Collections.Generic;

// Hero cards from the Inn: pick one hero to join the roster, and the night's recruiting is over.
// Every hero is rolled up front, so undoing a pass brings back the same hero
public class RecruitDeck : SwipeDeck
{
    private readonly List<HeroData> _heroes = new List<HeroData>();
    private HeroData _picked;

    public RecruitDeck(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _heroes.Add(HeroGenerator.Instance.Generate());
        }
    }

    public override int Count => _heroes.Count;
    public override string Title => "Recruit";
    public override string AcceptLabel => "Recruit";
    public override string PassLabel => "Reject";
    public override bool FinishOnAccept => true;
    public override bool CanInspect => true;
    public override string BlockedReason => RosterManager.Instance.IsFull ? "Roster is full" : null;

    public override string FinishedMessage => _picked != null
        ? _picked.Name + " the " + _picked.Class + " joined your guild!"
        : "No one joined tonight.";

    public override void ShowCurrent(SwipeCardView view)
    {
        view.ShowHero(_heroes[Index]);
    }

    public override bool TryInspect(HeroDetailPanel heroPanel)
    {
        heroPanel.Show(_heroes[Index]);
        return true;
    }

    protected override void Accept(int index)
    {
        if (RosterManager.Instance.TryAddHero(_heroes[index]))
        {
            _picked = _heroes[index];
        }
    }
}
