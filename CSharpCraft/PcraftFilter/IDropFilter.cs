namespace CSharpCraft.PcraftFilter;

/// <summary>
/// Hook point for per-material drop customisation in the seeded path.
/// Both methods have identity implementations (return their inputs unchanged).
/// Future implementations can apply piecewise curves over <paramref name="harvestIndex"/>
/// to bias drop rates or counts.
/// </summary>
internal interface IDropFilter
{
    /// <summary>
    /// Transforms a raw [0,1) roll before it is compared against <c>dropChance</c>.
    /// Identity: return <paramref name="rawRoll"/> unchanged.
    /// </summary>
    double NudgeRoll(double rawRoll, int harvestIndex);

    /// <summary>
    /// Adjusts the count range for this specific harvest event.
    /// Identity: return <c>(baseMin, baseMax)</c> unchanged.
    /// </summary>
    (int min, int max) ModifyCount(int baseMin, int baseMax, int harvestIndex);
}
