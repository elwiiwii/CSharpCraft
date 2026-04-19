using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Map;

internal static class LevelManager
{
    internal static void SetLevel(Level level, PlayerEntity player)
    {
        player.X = level.Stx;
        player.Y = level.Sty;
    }

    internal static void FillEne(Level level, PlayerEntity player)
    {
        level.Ene.Clear();

        for (int i = 0; i < level.Sx; i++)
        {
            for (int j = 0; j < level.Sy; j++)
            {
                var c = PcraftServices.GetDirectGr(i, j, level);
                var r = Pico8.Rnd(100);
                var ex = F32.FromInt(i * 16 + 8);
                var ey = F32.FromInt(j * 16 + 8);
                var dist = F32.Max(F32.Abs(ex - player.X), F32.Abs(ey - player.Y));
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

    internal static Level CreateLevel(int x, int y, int sx, int sy, bool isUnder, PlayerEntity player)
    {
        var level = new Level(x, y, sx, sy, isUnder);
        PcraftServices.SetLevel(level, player);
        var (holeX, holeY) = PcraftServices.CreateMap(level, player);
        PcraftServices.FillEne(level, player);
        level.Stx = F32.FromInt((holeX - level.X) * 16 + 8);
        level.Sty = F32.FromInt((holeY - level.Y) * 16 + 8);
        return level;
    }

    internal static void ResetLevel(
        PlayerEntity player, PcraftGame game,
        out Level cave, out Level island)
    {
        player.Prot   = F32.Zero;
        player.Lrot   = F32.Zero;
        player.Panim  = F32.Zero;
        player.Stam   = F32.FromInt(100);
        player.Lstam  = player.Stam;
        player.Life   = F32.FromInt(100);
        player.Llife  = player.Life;
        player.Banim  = F32.Zero;
        player.Camera.Coffx  = F32.Zero;
        player.Camera.Coffy  = F32.Zero;
        player.CurItem = null;

        player.Invent.Clear();

        game.InitRecipes();
        cave   = PcraftServices.CreateLevel(64, 0, 32, 32, true,  player);
        island = PcraftServices.CreateLevel( 0, 0, 64, 64, false, player);

        // Init RndWat on both levels
        var rndWat = PcraftServices.InitRndWat();
        foreach (var lev in new[] { cave, island })
        {
            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 16; j++)
                    lev.RndWat[i][j] = rndWat[i][j];
        }

        var workbench = new ItemEntity(PcraftData.Workbench, player.X, player.Y);
        workbench.HasCol = true;
        workbench.List   = game.WorkbenchRecipe;
        player.Invent.Add(new ItemStack(workbench.Type, list: game.WorkbenchRecipe));

        player.Invent.Add(new ItemStack(PcraftData.PickupTool));
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
            entity.Timer    = 110 + Pico8.Rnd(20);
            entities.Add(entity);
        }
    }

    internal static void UpGround(Level level, PlayerEntity player)
    {
        var camera = player.Camera;
        int ci = F32.FloorToInt((camera.Clx - F32.FromInt(64)) / F32.FromInt(16));
        int cj = F32.FloorToInt((camera.Cly - F32.FromInt(64)) / F32.FromInt(16));

        for (int i = ci; i <= ci + 8; i++)
        {
            for (int j = cj; j <= cj + 8; j++)
            {
                var gr = PcraftServices.GetDirectGr(i, j, level);
                if (gr == PcraftData.GrFarm)
                {
                    var d = PcraftServices.DirGetData(i, j, F32.Zero, level);
                    if (level.Time > d)
                        Pico8.Mset(i + level.X, j, PcraftData.GrSand.Id);
                }
            }
        }
    }
}
