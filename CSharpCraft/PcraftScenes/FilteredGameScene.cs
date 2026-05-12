using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftFilter;
using CSharpCraft.PcraftFilter.Filters;
using PSharp8.Scene;

namespace CSharpCraft.PcraftScenes;

/// <summary>
/// Launches a game session whose level is generated with the same filter
/// constraints that were used in the preview, guaranteeing the map the
/// player plays on is identical to what they saw.
/// </summary>
internal sealed class FilteredGameScene : PcraftSceneBase
{
    private readonly long _seed;
    private readonly FilterSet _filters;

    internal FilteredGameScene(long seed, FilterSet filters)
    {
        _seed = seed;
        _filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }

    public override void Init(ISceneSetup setup)
    {
        _ = new FilteredSeededServices(_seed, _filters); // registers itself via SetServices(this) before base.Init fires
        base.Init(setup);
    }
}
