using CSharpCraft.Pico8;

namespace CSharpCraft.Pcraft;

public class PcraftSingleplayer : PcraftBase
{
    public override string SceneName => "pcraft";

    public override void Init()
    {
        base.Init();
    }
}

public class DeluxeSingleplayer : DeluxeBase
{
    public override string SceneName => "deluxe";

    public override void Init()
    {
        base.Init();
    }
}

public class PcraftSpeedrun : SpeedrunBase
{
    public override string SceneName => "speedrun";

    public override void Init()
    {
        base.Init();
    }
}
