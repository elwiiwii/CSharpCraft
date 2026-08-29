using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftSeeded;
using PSharp8.Scene;

namespace CSharpCraft.PcraftPreview;

internal class PcraftPreviewBase : PcraftSceneBase
{
    public override string Name => "Pcraft Preview Base";

    /// <summary>World seed — determines the entire map deterministically.</summary>
    protected virtual long PreviewSeed => 0;

    /// <summary>
    /// Half-width of the sampled tile window. The rendered area is (2×radius+1)² tiles.
    /// </summary>
    protected virtual int PreviewRadius => 4;

    /// <summary>
    /// Override the window centre tile. When null the detected spawn tile is used.
    /// </summary>
    public virtual (int X, int Y)? CenterOverride => null;

    /// <summary>
    /// When true (default) a 30-fps time counter is registered to animate water tiles.
    /// </summary>
    public virtual bool AnimateWater => true;

    private SampleResult? _result;
    private F32 _time = F32.Zero;

    public override void Init(ISceneSetup setup)
    {
        int side = 2 * PreviewRadius;
        setup.Resolution = (16 * side, 16 * side);

        _result = PcraftWorldSampler.Sample(
            PreviewSeed, PreviewRadius,
            CenterOverride?.X, CenterOverride?.Y);

        _time = F32.Zero;
    }

    public override void Update()
    {
        if (AnimateWater)
            _time += F32.FromDouble(1.0 / 30.0);
    }

    public override void Draw()
    {
        PreviewDrawer.Draw(_result!, _time);
    }
}
