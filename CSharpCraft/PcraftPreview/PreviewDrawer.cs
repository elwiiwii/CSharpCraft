using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftSeeded;

namespace CSharpCraft.PcraftPreview;

internal static class PreviewDrawer
{
    internal static void Draw(SampleResult result, F32 time)
    {
        int side = result.Tiles.GetLength(0);
        int[,] tiles = result.Tiles;
        int worldOriginX = result.CenterTileX - (side - 1) / 2;
        int worldOriginY = result.CenterTileY - (side - 1) / 2;

        // Pass 1 — write corner-blended sprite indices into the map, then render with a single Map() call.
        // Sand tiles use randomised flat sprites (no corner blending), matching BackDrawer's sand path.
        // All other tiles use BackDrawer.CornerOffset for proper edge/inner-corner blending.
        for (int ti = 0; ti < side; ti++)
        {
            for (int tj = 0; tj < side; tj++)
            {
                int mx = ti * 2;
                int my = tj * 2;
                int tId = tiles[ti, tj];

                if (tId is 1 or 11) // sand + hole — flat randomised sand sprites, no corner blending
                {
                    Pico8.Mset(mx, my, RndSand(worldOriginX + ti, worldOriginY + tj, result.RndWat));
                    Pico8.Mset(mx + 1, my, RndSand(worldOriginX + ti + 0.5, worldOriginY + tj, result.RndWat));
                    Pico8.Mset(mx, my + 1, RndSand(worldOriginX + ti, worldOriginY + tj + 0.5, result.RndWat));
                    Pico8.Mset(mx + 1, my + 1, RndSand(worldOriginX + ti + 0.5, worldOriginY + tj + 0.5, result.RndWat));
                }
                else
                {
                    int b = SpriteBase(tId);

                    bool u = SameGroup(tiles, ti, tj - 1, tId, side);
                    bool d = SameGroup(tiles, ti, tj + 1, tId, side);
                    bool l = SameGroup(tiles, ti - 1, tj, tId, side);
                    bool r = SameGroup(tiles, ti + 1, tj, tId, side);
                    bool ul = SameGroup(tiles, ti - 1, tj - 1, tId, side);
                    bool ur = SameGroup(tiles, ti + 1, tj - 1, tId, side);
                    bool dl = SameGroup(tiles, ti - 1, tj + 1, tId, side);
                    bool dr = SameGroup(tiles, ti + 1, tj + 1, tId, side);

                    int tl = PcraftServices.CornerOffset(l, u, ul, RndCenter(worldOriginX + ti, worldOriginY + tj, result.RndWat), innerCorner: 20, hOnly: 1, vOnly: 16, outer: 0);
                    int tr = PcraftServices.CornerOffset(r, u, ur, RndCenter(worldOriginX + ti + 0.5, worldOriginY + tj, result.RndWat), innerCorner: 19, hOnly: 1, vOnly: 18, outer: 2);
                    int bl = PcraftServices.CornerOffset(l, d, dl, RndCenter(worldOriginX + ti, worldOriginY + tj + 0.5, result.RndWat), innerCorner: 4, hOnly: 33, vOnly: 16, outer: 32);
                    int br = PcraftServices.CornerOffset(r, d, dr, RndCenter(worldOriginX + ti + 0.5, worldOriginY + tj + 0.5, result.RndWat), innerCorner: 3, hOnly: 33, vOnly: 18, outer: 34);

                    Pico8.Mset(mx, my, b + tl);
                    Pico8.Mset(mx + 1, my, b + tr);
                    Pico8.Mset(mx, my + 1, b + bl);
                    Pico8.Mset(mx + 1, my + 1, b + br);
                }
            }
        }

        Pico8.Map(0, 0, -8, -8, side * 2, side * 2);

        // Pass 2 — tree quad overlay, same sprite layout as BackDrawer's Spr4 for trees
        PcraftServices.SetPal(PcraftData.TileTree.SpritePal!);
        for (int ti = 0; ti < side; ti++)
        {
            for (int tj = 0; tj < side; tj++)
            {
                if (tiles[ti, tj] != 4) continue;

                int px = ti * 16;
                int py = tj * 16;
                Pico8.Spr(RndTree(worldOriginX + ti, worldOriginY + tj, result.RndWat) + 64, px - 8, py - 8);
                Pico8.Spr(RndTree(worldOriginX + ti + 0.5, worldOriginY + tj, result.RndWat) + 65, px - 8 + 8, py - 8);
                Pico8.Spr(RndTree(worldOriginX + ti, worldOriginY + tj + 0.5, result.RndWat) + 80, px - 8, py - 8 + 8);
                Pico8.Spr(RndTree(worldOriginX + ti + 0.5, worldOriginY + tj + 0.5, result.RndWat) + 81, px - 8 + 8, py - 8 + 8);
            }
        }
        Pico8.Pal();

        // Pass 3 — water animation overlay, same formula as BackDrawer.WatAnim
        for (int ti = 0; ti < side; ti++)
        {
            for (int tj = 0; tj < side; tj++)
            {
                if (tiles[ti, tj] != 0) continue;

                int px = ti * 16;
                int py = tj * 16;
                WatAnim(worldOriginX + ti, worldOriginY + tj, px - 8, py - 8, time, result.RndWat);
                WatAnim(worldOriginX + ti + 0.5, worldOriginY + tj, px - 8 + 8, py - 8, time, result.RndWat);
                WatAnim(worldOriginX + ti, worldOriginY + tj + 0.5, px - 8, py - 8 + 8, time, result.RndWat);
                WatAnim(worldOriginX + ti + 0.5, worldOriginY + tj + 0.5, px - 8 + 8, py - 8 + 8, time, result.RndWat);
            }
        }

        // Pass 4 — hole overlay: sprite 31 (left + right halves) + sprite 77 (portal), same as BackDrawer.
        for (int ti = 0; ti < side; ti++)
        {
            for (int tj = 0; tj < side; tj++)
            {
                if (tiles[ti, tj] != 11) continue;

                int px = ti * 16;
                int py = tj * 16;

                Pico8.Palt(0, false);
                Pico8.Spr(31, px - 8, py - 8, 1, 2);
                Pico8.Spr(31, px - 8 + 8, py - 8, 1, 2, true, false);
                Pico8.Palt();
                Pico8.Spr(77, px - 8 + 4, py - 8, 1, 2);
            }
        }
    }

    // --- Sprite base values matching BackDrawer's 'b' variable ---
    private static int SpriteBase(int tileId)
    {
        return tileId switch
        {
            0 => 26,
            3 => 21,
            _ => 16
        };
    }

    // --- Blending group: tiles that should blend together share a group.
    // Rare (2) and tree (4) are both grass-base tiles and blend with each other.
    // Out-of-bounds neighbours are treated as same group to avoid hard edges at the viewport rim.
    private static bool SameGroup(int[,] tiles, int ti, int tj, int tileId, int side)
    {
        if (ti < 0 || ti >= side || tj < 0 || tj >= side) return true;
        static int Group(int id)
        {
            return id switch
            {
                0 => 0,
                1 => 1,
                11 => 1,
                3 => 3,
                _ => 2
            };
        }

        return Group(tiles[ti, tj]) == Group(tileId);
    }

    // --- Rnd helpers — mirror of BackDrawer private helpers, adapted to double[,] RndWat ---
    // i and j are world tile coordinates so the lookup matches PcraftBase.WatVal exactly.
    internal static F32 WatVal(double i, double j, double[,] rndWat)
    {
        int xi = (int)(Math.Abs(i * 2) % 16);
        int yj = (int)(Math.Abs(j * 2) % 16);
        return F32.FromDouble(rndWat[xi, yj]);
    }

    private static int RndCenter(double i, double j, double[,] rndWat)
    {
        return (F32.FloorToInt(WatVal(i, j, rndWat) / F32.FromInt(34)) + 18) % 20;
    }

    private static int RndSand(double i, double j, double[,] rndWat)
    {
        return F32.FloorToInt(WatVal(i, j, rndWat) / F32.FromInt(34)) + 1;
    }

    private static int RndTree(double i, double j, double[,] rndWat)
    {
        return F32.FloorToInt(WatVal(i, j, rndWat) / F32.FromInt(51)) * 32;
    }

    private static void WatAnim(double i, double j, int px, int py, F32 time, double[,] rndWat)
    {
        F32 a = ((time * F32.FromFloat(0.6f)) + (WatVal(i, j, rndWat) / F32.FromInt(100))) % F32.One * F32.FromInt(19);
        if (a > F32.FromInt(16))
            Pico8.Spr(F32.FloorToInt(F32.FromInt(13) + a - F32.FromInt(16)), px, py);
    }
}
