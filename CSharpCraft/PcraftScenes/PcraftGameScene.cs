using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftSeeded;
using PSharp8.Scene;

namespace CSharpCraft.PcraftScenes;

internal sealed class PcraftGameScene : PcraftSceneBase
{
    private readonly long _seed;

    internal PcraftGameScene(long seed)
    {
        _seed = seed;
    }

    public override void Init(ISceneSetup setup)
    {
        _ = new SeededServices(_seed); // registers itself via SetServices(this) before base.Init fires
        base.Init(setup);
    }
}
