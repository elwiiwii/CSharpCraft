using CSharpCraft.PcraftPreview;
using PSharp8.Input;
using PSharp8.Scene;

namespace CSharpCraft.PcraftScenes;

internal class PcraftPreviewScene : PcraftPreviewBase
{
    public override void Init(ISceneSetup setup)
    {
        base.Init(setup);

        bool launched = false;
        _ = setup.RegisterUpdate(() =>
        {
            if (!launched && (Pico8.Btn() & ~(1 << (int)PicoButton.Pause)) > 0)
            {
                launched = true;
                LaunchGame();
            }
        }, fps: 30);
    }

    protected virtual void LaunchGame()
    {
        Pico8.ScheduleScene(() => new PcraftGameScene(PreviewSeed));
    }
}
