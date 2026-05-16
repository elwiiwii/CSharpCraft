using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftSeeded;

internal static class SeededLevelManager
{
    internal static Level CreateLevel(long islandSeed, long caveSeed, int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
    {
        Level level = new(x, y, sx, sy, theme);
        PcraftServices.SetLevel(level, player);

        (int holeX, int holeY) = theme == LevelTheme.Cave
            ? SeededMapGenerator.CreateCaveMap(level, player, caveSeed)
            : SeededMapGenerator.CreateMap(level, player, islandSeed);

        PcraftServices.FillEne(level, player);
        level.Stx = F32.FromInt(((holeX - level.X) * 16) + 8);
        level.Sty = F32.FromInt(((holeY - level.Y) * 16) + 8);
        return level;
    }
}
