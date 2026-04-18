using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Data;

internal sealed class MenuState(ItemDef type, int spr, string? text, string? text2)
    : IMenu
{
    internal ItemDef Type { get; } = type;
    internal int Spr { get; } = spr;
    internal string? Text { get; } = text;
    internal string? Text2 { get; } = text2;

    public void Update(WorldState state, PcraftGame game)
    {
        if (Spr <= 0) return;
        if (Pico8.Btnp(4) && !state.Lb4)
        {
            if (ReferenceEquals(this, PcraftData.MainMenu))
            {
                state.CurMenu = PcraftData.IntroMenu;
            }
            else
            {
                PcraftServices.ResetLevel(state, game);
                state.CurMenu = null;
                Pico8.Music(1);
            }
        }
    }

    public void Draw(WorldState state)
    {
        if (Spr <= 0) return;

        Pico8.Camera();
        Pico8.Palt(0, false);
        Pico8.Rectfill(0, 0, 128, 46, 12);
        Pico8.Rectfill(0, 46, 128, 128, 1);
        Pico8.Spr(Spr, 32, 14, 8, 8);
        PcraftServices.PrintC(Text ?? "", 64, 80, 6);
        PcraftServices.PrintC(Text2 ?? "", 64, 90, 6);
        int tc = 6 + F32.FloorToInt(state.Time % 2);
        PcraftServices.PrintC("press button 1", 64, 112, tc);
        state.Time += F32.FromDouble(0.1);
    }
}
