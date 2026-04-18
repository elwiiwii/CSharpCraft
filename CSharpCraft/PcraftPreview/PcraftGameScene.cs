#nullable enable
using PSharp8.Scene;

namespace CSharpCraft.PcraftPreview;

internal sealed class PcraftGameScene : PcraftBase.PcraftSceneBase
{
    private readonly long _seed;

    internal PcraftGameScene(long seed)
    {
        _seed = seed;
    }

    public override void Init(ISceneSetup setup)
    {
        new SeededServices(_seed); // registers itself via SetServices(this) before base.Init fires
        base.Init(setup);
    }
}
