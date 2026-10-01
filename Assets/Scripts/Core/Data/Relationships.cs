// Saved as ints in assets: never reorder or renumber, only append

// Always calculated from affinity (thresholds in BalanceConfig), never stored
public enum RelationshipTier
{
    Stranger = 0,
    Friend = 1,
    Close = 2,
    Devoted = 3
}

// Stored on each RelationshipData (one per pair of heroes); only changed by relationship events (asking out, breakups, deaths)
public enum RelationshipStatus
{
    None = 0,
    Dating = 1,
    Ex = 2,
    Widowed = 3
}
