using CSharpCraft.PcraftBase.Data;
using PSharp8.Graphics;

namespace CSharpCraft.PcraftBase.Draw;

internal static class EnemiesDrawer
{
    internal static void DrawEnemies(PlayerEntity player, Level level)
    {
        CameraState camera = player.Camera;
        // Build combined Y-sortable list: player + visible enemies
        List<CharacterEntity> drawList = new(level.Ene.Count + 1)
        {
            player
        };

        foreach (CharacterEntity e in level.Ene)
        {
            if (PcraftServices.IsIn(e, F32.FromInt(72), camera.Clx, camera.Cly))
                drawList.Add(e);
        }

        PcraftServices.SortY(drawList);

        foreach (CharacterEntity e in drawList)
        {
            if (e is PlayerEntity p)
            {
                Pico8.Pal();
                PcraftServices.DrawPlayer(p.X, p.Y, p.Prot, p.Panim, p.Banim, isPlayer: true, p, level);
            }
            else
            {
                Pico8.Pal();
                Pico8.Pal(PicoColor._15LightPeach, PicoColor._03DarkGreen);
                Pico8.Pal(PicoColor._04Brown, PicoColor._01DarkBlue);
                Pico8.Pal(PicoColor._02DarkPurple, PicoColor._08Red);
                Pico8.Pal(PicoColor._01DarkBlue, PicoColor._01DarkBlue);
                PcraftServices.DrawPlayer(e.X, e.Y, e.Prot, e.Panim, e.Banim, isPlayer: false, player, level);
            }
        }
    }

    internal static void DrawPlayer(F32 x, F32 y, F32 rot, F32 anim, F32 subAnim, bool isPlayer, PlayerEntity player, Level level)
    {
        F32 cr = Pico8.Cos(rot);
        F32 sr = Pico8.Sin(rot);
        F32 cv = -sr;
        F32 sv = cr;

        F32 fxF = F32.Floor(x);
        F32 fyF = F32.Floor(y - 4);

        F32 lan = Pico8.Sin(anim * 2) * F32.FromDouble(1.5);
        Tile belTile = PcraftServices.GetTile(x, y, level);

        if (belTile.Type == PcraftData.TileWater)
        {
            fyF += 4;
            Pico8.Circ(fxF + (cv * 3) + (cr * lan),
                       fyF + (sv * 3) + (sr * lan),
                       F32.FromInt(3), PicoColor._06LightGrey);
            Pico8.Circ(fxF - (cv * 3) - (cr * lan),
                       fyF - (sv * 3) - (sr * lan),
                       F32.FromInt(3), PicoColor._06LightGrey);

            F32 anc = 3 + (level.Time * 3 % F32.One * 3);
            Pico8.Circ(fxF + (cv * 3) + (cr * lan),
                       fyF + (sv * 3) + (sr * lan),
                       anc, PicoColor._06LightGrey);
            Pico8.Circ(fxF - (cv * 3) - (cr * lan),
                       fyF - (sv * 3) - (sr * lan),
                       anc, PicoColor._06LightGrey);
        }
        else
        {
            Pico8.Circfill(fxF + (cv * 2) - (cr * lan),
                           fyF + 3 + (sv * 2) - (sr * lan),
                           F32.FromInt(3), PicoColor._01DarkBlue);
            Pico8.Circfill(fxF - (cv * 2) + (cr * lan),
                           fyF + 3 - (sv * 2) + (sr * lan),
                           F32.FromInt(3), PicoColor._01DarkBlue);
        }

        F32 blade = (rot + F32.FromDouble(0.25)) % F32.One;
        if (subAnim > F32.Zero)
            blade = blade - F32.FromDouble(0.3) + (subAnim * F32.FromDouble(0.04));

        F32 bcr = Pico8.Cos(blade);
        F32 bsr = Pico8.Sin(blade);
        (int mx, int my) = PcraftServices.Mirror(blade);

        int weap = 75;
        if (isPlayer && player.CurItem is not null)
        {
            Pico8.Pal();
            weap = player.CurItem.Type.Spr;
            if (player.CurItem is ToolItem curTool)
            {
                int pw = curTool.Power - 1;
                if (pw >= 0 && pw < PcraftData.PwrPal.Length)
                    PcraftServices.SetPal(PcraftData.PwrPal[pw]);
            }
            if (player.CurItem.Type.Pal is not null)
                PcraftServices.SetPal(player.CurItem.Type.Pal);
        }

        Pico8.Spr(weap,
            fxF.Double + (bcr.Double * 4) - (cr.Double * lan.Double) - (mx * 8) + 1,
            fyF.Double + (bsr.Double * 4) - (sr.Double * lan.Double) + (my * 8) - 7,
            1, 1, mx == 1, my == 1);

        if (isPlayer) Pico8.Pal();

        if (belTile.Type != PcraftData.TileWater)
        {
            Pico8.Circfill(fxF + (cv * 3) + (cr * lan),
                           fyF + (sv * 3) + (sr * lan),
                           F32.FromInt(3), PicoColor._02DarkPurple);
            Pico8.Circfill(fxF - (cv * 3) - (cr * lan),
                           fyF - (sv * 3) - (sr * lan),
                           F32.FromInt(3), PicoColor._02DarkPurple);

            (int my2, int mx2) = PcraftServices.Mirror((rot + F32.FromDouble(0.75)) % F32.One);
            Pico8.Spr(75,
                fxF.Double + (cv.Double * 4) + (cr.Double * lan.Double) - 8 + (mx2 * 8) + 1,
                fyF.Double + (sv.Double * 4) + (sr.Double * lan.Double) + (my2 * 8) - 7,
                1, 1, mx2 == 0, my2 == 1);
        }

        Pico8.Circfill(fxF + cr, fyF + sr - 2, F32.FromInt(4), PicoColor._02DarkPurple);
        Pico8.Circfill(fxF + cr, fyF + sr, F32.FromInt(4), PicoColor._02DarkPurple);
        Pico8.Circfill(fxF + (cr * F32.FromDouble(1.5)), fyF + (sr * F32.FromDouble(1.5)) - 2, F32.FromDouble(2.5), PicoColor._15LightPeach);
        Pico8.Circfill(fxF - cr, fyF - sr - 3, F32.FromInt(3), PicoColor._04Brown);
    }

    internal static void SortY(List<CharacterEntity> enemies)
    {
        int tv = enemies.Count - 1;
        for (int i = 0; i < tv; i++)
        {
            CharacterEntity t1 = enemies[i];
            CharacterEntity t2 = enemies[i + 1];
            if (t1.Y > t2.Y)
            {
                enemies[i] = t2;
                enemies[i + 1] = t1;
            }
        }
    }
}
