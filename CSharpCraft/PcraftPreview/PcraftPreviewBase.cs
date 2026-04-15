using CSharpCraft.PcraftBase.Draw;
using CSharpCraft.PcraftBase.Update;
using CSharpCraft.PcraftBase;
using PSharp8.Scene;

namespace CSharpCraft.PcraftPreview;

internal abstract class PcraftPreviewBase : PcraftSceneBase, IScene
{
    public override string? Name => "Pcraft Preview Base";

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
