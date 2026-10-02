using System.Collections.Generic;

// Item cards, e.g. an adventure's rewards. Accepting puts the item in the inventory; passing leaves it behind.
// Rewards opens it with CardSwipeScreen.Open(new ItemRewardDeck(items), onFinished)
public class ItemRewardDeck : SwipeDeck
{
    private readonly List<ItemData> _items = new List<ItemData>();

    public ItemRewardDeck(IEnumerable<ItemData> items)
    {
        foreach (ItemData item in items)
        {
            if (item != null)
            {
                _items.Add(item);
            }
        }
    }

    public override int Count => _items.Count;
    public override string Title => "Rewards";
    public override string AcceptLabel => "Take";
    public override string PassLabel => "Leave";
    public override string FinishedMessage => "All rewards sorted.";

    public override void ShowCurrent(SwipeCardView view)
    {
        view.ShowItem(_items[Index]);
    }

    protected override void Accept(int index)
    {
        Inventory.Instance.Add(_items[index]);
    }
}
