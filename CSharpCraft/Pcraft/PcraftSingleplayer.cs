using CSharpCraft.Pico8;

namespace CSharpCraft.Pcraft;

public class PcraftSingleplayer : PcraftBase
{
    public override string SceneName => "pcraft";

    public override void Init(Pico8Functions pico8)
    {
        base.Init(pico8);
    }
}

public class DeluxeSingleplayer : DeluxeBase
{
    public override string SceneName => "deluxe";

    public override void Init(Pico8Functions pico8)
    {
        base.Init(pico8);
    }
}

public class PcraftSpeedrun : SpeedrunBase
{
    public override string SceneName => "speedrun";

    public override void Init(Pico8Functions pico8)
    {
        base.Init(pico8);
    }
}
