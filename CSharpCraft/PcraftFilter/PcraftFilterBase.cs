using CSharpCraft.PcraftBase.Draw;
using CSharpCraft.PcraftBase.Update;
using CSharpCraft.PcraftBase;
using PSharp8.Scene;

namespace CSharpCraft.PcraftFilter;

internal class PcraftFilterBase : PcraftSceneBase, IScene
{
    public override string? Name => "Pcraft Filter Base";

    public override void Init(ISceneSetup setup)
    {
        setup.Resolution = (128, 128);

        var state       = new WorldState();
        var game        = new PcraftGame();
        bool initialized = false;

        setup.RegisterUpdate(() =>
        {
            if (!initialized) { game.Init(); initialized = true; }
            PcraftUpdate.Update(state, game);
        }, fps: 30);

        setup.RegisterDraw(() => PcraftDraw.Draw(state, game), fps: 30);
    }
}
