using CSharpCraft.PcraftPreview;
using PSharp8.Input;
using PSharp8.Scene;

namespace CSharpCraft.PcraftScenes;

internal class PcraftPreviewScene : PcraftPreviewBase
{
    private bool _launched;

    public override void Init(ISceneSetup setup)
    {
        base.Init(setup);
        _launched = false;
    }

    public override void Update()
    {
        base.Update();
        if (!_launched && AnyButton())
        {
            _launched = true;
            LaunchGame();
        }
    }

    protected virtual void LaunchGame()
    {
        Pico8.ScheduleScene(() => new PcraftGameScene(PreviewSeed));
    }

    protected virtual bool AnyButton()
    {
        return (Pico8.Btn() & ~(1 << (int)PicoButton.Pause)) > 0;
    }
}
