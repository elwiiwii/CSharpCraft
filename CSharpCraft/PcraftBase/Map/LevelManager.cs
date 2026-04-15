using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Map;

internal static class LevelManager
{
    internal static void SetLevel(Level level, WorldState state)
    {
        state.SetLevel(level);
        state.Plx = level.Stx;
        state.Ply = level.Sty;
    }

    internal static void FillEne(Level level, WorldState state)
    {
        level.Ene.Clear();
        level.Ene.Add(new PlayerEntity(F32.Zero, F32.Zero));
        state.Enemies = level.Ene;

        for (int i = 0; i < state.LevelSx; i++)
        {
            for (int j = 0; j < state.LevelSy; j++)
            {
                var c = MapOps.GetDirectGr(i, j, state);
                var r = Pico8.Rnd(100);
                var ex = F32.FromInt(i * 16 + 8);
                var ey = F32.FromInt(j * 16 + 8);
                var dist = F32.Max(F32.Abs(ex - state.Plx), F32.Abs(ey - state.Ply));
                if (r < 3 &&
                    c != PcraftData.GrWater &&
                    c != PcraftData.GrRock &&
                    !c.IsTree &&
                    dist > F32.FromInt(50))
                {
                    var zombie = new ZombieEntity(ex, ey);
                    zombie.Life  = F32.FromInt(10);
                    zombie.Prot  = F32.Zero;
                    zombie.Lrot  = F32.Zero;
                    zombie.Panim = F32.Zero;
                    zombie.Banim = F32.Zero;
                    zombie.Dtim  = F32.Zero;
                    zombie.Step  = 0;
                    zombie.Ox    = F32.Zero;
                    zombie.Oy    = F32.Zero;
                    level.Ene.Add(zombie);
                }
            }
        }
    }

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

    internal static void ResetLevel(WorldState state, PcraftGame game)
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

        var rndWat = MapGenerator.InitRndWat();
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                state.RndWat[i][j] = rndWat[i][j];

        game.InitRecipes();
        state.Cave   = CreateLevel(64, 0, 32, 32, isUnder: true,  state);
        state.Island = CreateLevel( 0, 0, 64, 64, isUnder: false, state);

        var workbench = new ItemEntity(PcraftData.Workbench, state.Plx, state.Ply);
        workbench.HasCol = true;
        workbench.List   = game.WorkbenchRecipe;
        state.Invent.Add(new ItemStack(workbench.Type, list: game.WorkbenchRecipe));

        state.Invent.Add(new ItemStack(PcraftData.PickupTool));
    }

    internal static void AddItem(ItemDef mat, int count, F32 hitX, F32 hitY, List<ItemEntity> entities)
    {
        int tileX = F32.FloorToInt(hitX / F32.FromInt(16)) * 16;
        int tileY = F32.FloorToInt(hitY / F32.FromInt(16)) * 16;

        for (int k = 0; k < count; k++)
        {
            var ex = tileX + Pico8.Rnd(14) + 1;
            var ey = tileY + Pico8.Rnd(14) + 1;
            var entity = new ItemEntity(mat, ex, ey);
            entity.GiveItem = mat;
            entity.HasCol   = true;
            entity.Timer    = 110 + (Pico8.Rnd(20));
            entities.Add(entity);
        }
    }

    internal static void UpGround(WorldState state)
    {
        int ci = F32.FloorToInt((state.Clx - F32.FromInt(64)) / F32.FromInt(16));
        int cj = F32.FloorToInt((state.Cly - F32.FromInt(64)) / F32.FromInt(16));

        for (int i = ci; i <= ci + 8; i++)
        {
            for (int j = cj; j <= cj + 8; j++)
            {
                var gr = MapOps.GetDirectGr(i, j, state);
                if (gr == PcraftData.GrFarm)
                {
                    var d = MapOps.DirGetData(i, j, F32.Zero, state);
                    if (state.Time > d)
                        Pico8.Mset(i + state.LevelX, j, PcraftData.GrSand.Id);
                }
            }
        }
    }
}
