// One line of a fight, for playback on the Battle screen.
// Target and HpAfter are set when the line changes someone's HP
public class BattleEvent
{
    public string Text { get; }
    public BattleUnit Target { get; }
    public int HpAfter { get; }

    public BattleEvent(string text, BattleUnit target = null, int hpAfter = 0)
    {
        Text = text;
        Target = target;
        HpAfter = hpAfter;
    }
}