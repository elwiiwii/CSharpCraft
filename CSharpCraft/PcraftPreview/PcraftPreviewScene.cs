using PSharp8.Scene;

namespace CSharpCraft.PcraftPreview;

internal class PcraftPreviewScene : PcraftPreviewBase
{
    public override void Init(ISceneSetup setup)
    {
        base.Init(setup);

        bool launched = false;
        setup.RegisterUpdate(() =>
        {
            if (!launched && AnyButton())
            {
                launched = true;
                LaunchGame();
            }
        }, fps: 30);
    }

    protected virtual void LaunchGame()
        => Pico8.ScheduleScene(() => new PcraftGameScene(PreviewSeed));

    protected virtual bool AnyButton()
        => Pico8.Btn(0) || Pico8.Btn(1) || Pico8.Btn(2)
        || Pico8.Btn(3) || Pico8.Btn(4) || Pico8.Btn(5);
}
