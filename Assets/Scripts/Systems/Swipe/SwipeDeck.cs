// One go at the card swipe screen: a fixed list of cards, decided one at a time.
// Accept (swipe right) does whatever the deck is for, Pass (swipe left) skips the card,
// and an undo token brings back the card that was just passed.
// Subclasses decide what the cards are and what accepting does: RecruitDeck (heroes), ItemRewardDeck (items)
public abstract class SwipeDeck
{
    private bool _lastWasPass;

    // The current card. Equal to Count once every card has been decided
    public int Index { get; private set; }
    public abstract int Count { get; }
    public bool IsFinished => Index >= Count;

    // Shown at the top of the swipe screen, and on the accept button
    public abstract string Title { get; }
    public abstract string AcceptLabel { get; }

    // Why the current card can't be accepted right now (e.g. "Roster is full"), or null if it can
    public virtual string BlockedReason => null;

    public bool CanAccept => !IsFinished && BlockedReason == null;

    // Only the card passed most recently can come back, and only before another card is decided
    public bool CanUndo => _lastWasPass;

    // Draws the current card
    public abstract void ShowCurrent(SwipeCardView view);

    // Swipe up: shows more about the current card. False if this deck has nothing more to show
    public virtual bool TryInspect(HeroDetailPanel heroPanel)
    {
        return false;
    }

    public bool TryAccept()
    {
        if (!CanAccept)
        {
            return false;
        }

        Accept(Index);
        _lastWasPass = false;
        Index++;
        return true;
    }

    public void Pass()
    {
        if (IsFinished)
        {
            return;
        }

        _lastWasPass = true;
        Index++;
    }

    // Spends one undo token
    public bool TryUndo()
    {
        if (!_lastWasPass || !GameManager.Instance.TryUseUndoToken())
        {
            return false;
        }

        _lastWasPass = false;
        Index--;
        return true;
    }

    protected abstract void Accept(int index);
}
