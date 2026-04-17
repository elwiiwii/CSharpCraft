#nullable enable

namespace CSharpCraft.PcraftPreview;

internal sealed class PcraftGameScene : PcraftBase.PcraftSceneBase
{
    private readonly long _seed;

    internal PcraftGameScene(long seed)
    {
        _seed = seed;
    }

    protected override void OnGameInit(PcraftBase.WorldState state, PcraftBase.PcraftGame game)
    {
        state.LevelMgr = new SeededLevelManager(_seed);
        state.LevelMgr.ResetLevel(state, game);
    }
}
