public enum ScreenId
{
    None = 0,
    Town = 1,
    Recruit = 2,        // The Inn: night hub that DayCycle.EndDay opens. Recruiting itself happens on CardSwipe
    Roster = 3,
    Inspection = 4,
    Dialogue = 5,
    Shop = 6,
    AdventureSelect = 7,
    Battle = 8,
    Rewards = 9,
    Summary = 10,
    CardSwipe = 11      // Swipe through a SwipeDeck (recruits or item rewards). Opened with CardSwipeScreen.Open
}