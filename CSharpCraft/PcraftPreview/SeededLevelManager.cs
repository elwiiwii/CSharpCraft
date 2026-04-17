using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftPreview;

internal static class LevelManager
{
    internal static Level CreateLevel(int x, int y, int sx, int sy, bool isUnder, WorldState state)
    {
        var level = new Level(x, y, sx, sy, isUnder);
        SetLevel(level, state);
        var (holeX, holeY) = MapGenerator.CreateMap(state);
        FillEne(level, state);
        level.Stx = F32.FromInt((holeX - state.LevelX) * 16 + 8);
        level.Sty = F32.FromInt((holeY - state.LevelY) * 16 + 8);
        return level;
    }

    private static Level CreateSeededIsland(long seed, WorldState state)
    {
        var level = new Level(0, 0, 64, 64, isUnder: false);
        SetLevel(level, state);
        var (holeX, holeY) = SeededMapGenerator.CreateMap(state, seed);
        FillEne(level, state);
        level.Stx = F32.FromInt((holeX - state.LevelX) * 16 + 8);
        level.Sty = F32.FromInt((holeY - state.LevelY) * 16 + 8);
        return level;
    }

    internal static void ResetLevel(WorldState state, PcraftGame game, long seed)
    {
        state.Prot  = F32.Zero;
        state.Lrot  = F32.Zero;
        state.Panim = F32.Zero;
        state.Pstam = F32.FromInt(100);
        state.Lstam = state.Pstam;
        state.Plife = F32.FromInt(100);
        state.Llife = state.Plife;
        state.Banim = F32.Zero;
        state.Coffx = F32.Zero;
        state.Coffy = F32.Zero;
        state.Time  = F32.Zero;
        state.SwitchLevel    = false;
        state.CanSwitchLevel = false;
        state.CurItem        = null;

        state.Invent.Clear();

        var rndWat = SeededMapGenerator.InitRndWat(seed);
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                state.RndWat[i][j] = rndWat[i][j];

        game.InitRecipes();
        state.Cave   = CreateLevel(64, 0, 32, 32, isUnder: true, state);
        state.Island = CreateSeededIsland(seed, state);

        var workbench = new ItemEntity(PcraftData.Workbench, state.Plx, state.Ply);
        workbench.HasCol = true;
        workbench.List   = game.WorkbenchRecipe;
        state.Invent.Add(new ItemStack(workbench.Type, list: game.WorkbenchRecipe));

        state.Invent.Add(new ItemStack(PcraftData.PickupTool));
    }
}
