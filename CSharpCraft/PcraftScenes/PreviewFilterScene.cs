using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftFilter;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftPreview;
using CSharpCraft.PcraftSeeded;
using PSharp8.Scene;

namespace CSharpCraft.PcraftScenes;

/// <summary>
/// Minimal tile-preview rendered via <see cref="PreviewDrawer"/>, with world
/// generation constrained by a <see cref="FilterSet"/>.
/// Combine a filtered preview with a button-launch by subclassing and adding
/// a RegisterUpdate call that calls <see cref="LaunchGame"/>.
/// </summary>
internal class PreviewFilterScene : PcraftSceneBase
{
    public override string Name => "Preview (Filtered)";

    protected virtual long PreviewSeed => 0;
    protected virtual int PreviewRadius => 4;
    protected virtual (int X, int Y)? CenterOverride => null;
    protected virtual bool AnimateWater => true;

    protected virtual FilterSet BuildFilters() => new([]);

    public override void Init(ISceneSetup setup)
    {
        int side = 2 * PreviewRadius;
        setup.Resolution = (16 * side, 16 * side);

        FilterSet filters = BuildFilters();
        SampleResult result = FilteredWorldSampler.Sample(
            PreviewSeed, PreviewRadius, filters,
            CenterOverride?.X, CenterOverride?.Y);

        F32 time = F32.Zero;

        if (AnimateWater)
            _ = setup.RegisterUpdate(() => time += F32.FromDouble(1.0 / 30.0), fps: 30);

        _ = setup.RegisterDraw(() => PreviewDrawer.Draw(result, time), fps: 30);
    }

    protected void LaunchGame(FilterSet filters)
    {
        Pico8.ScheduleScene(() => new FilteredGameScene(PreviewSeed, filters));
    }
}
