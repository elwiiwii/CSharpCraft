using CSharpCraft.PcraftBase;
using PSharp8.Scene;

namespace CSharpCraft.PcraftFilter;

internal class PcraftFilterBase : PcraftSceneBase, IScene
{
    public override string? Name => "Pcraft Filter Base";

    public override void Init(ISceneSetup setup)
    {
        base.Init(setup);
    }
}
