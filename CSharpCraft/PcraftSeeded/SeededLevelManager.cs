using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftSeeded;

internal static class SeededLevelManager
{
    private static long _seed;

    internal static void Initialize(long seed)
    {
        _seed = seed;
    }

    internal static Level CreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
    {
        if (theme == LevelTheme.Cave)
            return LevelManager.CreateLevel(x, y, sx, sy, theme, player);

        Level level = new(x, y, sx, sy, theme);
        PcraftServices.SetLevel(level, player);
        (int holeX, int holeY) = SeededMapGenerator.CreateMap(level, player, _seed);
        PcraftServices.FillEne(level, player);
        level.Stx = F32.FromInt(((holeX - level.X) * 16) + 8);
        level.Sty = F32.FromInt(((holeY - level.Y) * 16) + 8);
        return level;
    }
}
