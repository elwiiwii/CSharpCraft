#nullable enable

namespace CSharpCraft.PcraftPreview;

internal sealed class PcraftGameScene : PcraftBase.PcraftSceneBase
{
    private readonly long _seed;

    internal PcraftGameScene(long seed)
    {
        _seed = seed;
    }

    //protected override PcraftBase.PcraftServices CreateServices() => new SeededServices(_seed);
}
