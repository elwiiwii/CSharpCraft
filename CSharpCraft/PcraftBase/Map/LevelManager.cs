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
                Tile tile = PcraftServices.GetDirectTile(i, j, level);
                F32 r = Pico8.Rnd(100);
                var ex = F32.FromInt((i * 16) + 8);
                var ey = F32.FromInt((j * 16) + 8);
                var dist = F32.Max(F32.Abs(ex - player.X), F32.Abs(ey - player.Y));
                if (r < 3 &&
                    tile.Type is not WallTileType &&
                    tile.Type != PcraftData.TileWater &&
                    dist > F32.FromInt(50))
                {
                    ZombieEntity zombie = new(ex, ey)
                    {
                        Life = F32.FromInt(10),
                        Prot = F32.Zero,
                        Lrot = F32.Zero,
                        Panim = F32.Zero,
                        Banim = F32.Zero,
                        Dtim = F32.Zero,
                        Step = 0,
                        Ox = F32.Zero,
                        Oy = F32.Zero
                    };
                    level.Ene.Add(zombie);
                }
            }
        }
    }

    internal static Level CreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
    {
        Level level = new(x, y, sx, sy, theme);
        PcraftServices.SetLevel(level, player);
        (int holeX, int holeY) = PcraftServices.CreateMap(level, player);
        PcraftServices.FillEne(level, player);
        level.Stx = F32.FromInt(((holeX - level.X) * 16) + 8);
        level.Sty = F32.FromInt(((holeY - level.Y) * 16) + 8);
        return level;
    }

    internal static void ResetLevel(PlayerEntity player)
    {
        player.Prot = F32.Zero;
        player.Lrot = F32.Zero;
        player.Panim = F32.Zero;
        player.Stam = F32.FromInt(100);
        player.Lstam = player.Stam;
        player.Life = F32.FromInt(100);
        player.Llife = player.Life;
        player.Banim = F32.Zero;
        player.Camera.Coffx = F32.Zero;
        player.Camera.Coffy = F32.Zero;
        player.CurItem = null;

        player.Invent.Clear();

        PcraftSession session = PcraftSession.Current;
        session.Cave = PcraftServices.CreateLevel(64, 0, 32, 32, LevelTheme.Cave, player);
        session.Island = PcraftServices.CreateLevel(0, 0, 64, 64, LevelTheme.Surface, player);

        // Init RndWat on both levels
        F32[][] rndWat = PcraftServices.InitRndWat();
        foreach (Level? lev in new[] { session.Cave, session.Island })
        {
            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 16; j++)
                    lev.RndWat[i][j] = rndWat[i][j];
        }

        player.Invent.Add(new UnstackableItem(PcraftData.Workbench));
        player.Invent.Add(new UnstackableItem(PcraftData.PickupTool));

        player.CurrentLevel = session.Island;
        player.SwitchLevel = false;
        player.CanSwitchLevel = false;
    }

    internal static void AddItem(ItemDef mat, int count, F32 hitX, F32 hitY, List<Entity> entities)
    {
        int tileX = F32.FloorToInt(hitX / F32.FromInt(16)) * 16;
        int tileY = F32.FloorToInt(hitY / F32.FromInt(16)) * 16;

        for (int k = 0; k < count; k++)
        {
            F32 ex = tileX + Pico8.Rnd(14) + 1;
            F32 ey = tileY + Pico8.Rnd(14) + 1;
            F32 vx = Pico8.Rnd(3) - F32.FromDouble(1.5);
            F32 vy = Pico8.Rnd(3) - F32.FromDouble(1.5);
            DroppedItemEntity entity = new(mat, ex, ey, timer: 110 + Pico8.Rnd(20), vx: vx, vy: vy);
            entities.Add(entity);
        }
    }

    internal static void UpGround(Level level, PlayerEntity player)
    {
        CameraState camera = player.Camera;
        int ci = F32.FloorToInt((camera.Clx - F32.FromInt(64)) / F32.FromInt(16));
        int cj = F32.FloorToInt((camera.Cly - F32.FromInt(64)) / F32.FromInt(16));

        for (int i = ci; i <= ci + 8; i++)
        {
            for (int j = cj; j <= cj + 8; j++)
            {
                if (MapOps.OutOfBounds(i, j, level)) continue;
                Tile tile = level.Map[i, j];
                if (tile.Type == PcraftData.TileFarm && tile.GrowthTimer.HasValue && level.Time > tile.GrowthTimer.Value)
                    level.SetTile(i, j, new Tile(PcraftData.TileSand));
            }
        }
    }
}
