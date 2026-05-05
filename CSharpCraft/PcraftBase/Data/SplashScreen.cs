using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Data;

internal sealed class SplashScreen(int spr, IReadOnlyList<string> lines)
    : IMenu
{
    internal int Spr { get; } = spr;
    internal IReadOnlyList<string> Lines { get; } = lines;

    public bool Update(PlayerEntity player)
    {
        if (Spr <= 0) return false;
        if (Pico8.Btnp(4) && !player.Lb4)
        {
            if (ReferenceEquals(this, PcraftData.MainMenu))
            {
                player.CurMenu = PcraftData.IntroMenu;
            }
            else
            {
                player.CurMenu = null;
                return true;
            }
        }
        return false;
    }

    public void Draw(PlayerEntity player, Level level)
    {
        if (Spr <= 0) return;

        Pico8.Camera();
        Pico8.Palt(0, false);
        Pico8.Rectfill(0, 0, 128, 46, 12);
        Pico8.Rectfill(0, 46, 128, 128, 1);
        Pico8.Spr(Spr, 32, 14, 8, 8);
        for (int i = 0; i < Lines.Count; i++)
            PcraftServices.PrintC(Lines[i], 64, 80 + i * 10, 6);
        int tc = 6 + F32.FloorToInt(level.Time % 2);
        PcraftServices.PrintC("press button 1", 64, 112, tc);
        level.Time += F32.FromDouble(0.1);
    }
}
