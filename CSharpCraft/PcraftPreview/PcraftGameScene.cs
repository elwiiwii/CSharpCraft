#nullable enable
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftPreview;

internal sealed class PcraftGameScene : PcraftBase.PcraftSceneBase
{
    private readonly long _seed;

    internal PcraftGameScene(long seed)
    {
        _seed = seed;
    }

    protected override void OnGameInit(PcraftBase.WorldState state, PcraftBase.PcraftGame game)
        => LevelManager.ResetLevel(state, game, _seed);
}
