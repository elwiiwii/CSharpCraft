using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftPreview;

namespace CSharpCraft.PcraftFilter;

/// <summary>
/// Renders a <see cref="SampleResult"/> using <c>Sspr</c> at 7px per sprite cell
/// (14px per tile). Reuses all sprite index and blending logic from
/// <see cref="PcraftPreview.PreviewDrawer"/> without requiring the map work-area.
/// </summary>
internal static class FilterPreviewDrawer
{
    private const int GridSize = 64; // render [0..63] only; index 64 is OOB
    private const int CellPx = 7;  // dest pixels per 8×8 sprite cell
    private const int TilePx = CellPx * 2; // dest pixels per 16×16 tile

    internal static void Draw(SampleResult result, int offsetX, int offsetY)
    {
        int[,] tiles = result.Tiles;

        // Background
        for (int ti = 0; ti < GridSize; ti++)
        {
            for (int tj = 0; tj < GridSize; tj++)
            {
                int tId = tiles[ti, tj];
                int bx = offsetX + (ti * TilePx);
                int by = offsetY + (tj * TilePx);

                if (tId is 1 or 11)
                {
                    SsprCell(RndSand(ti, tj, result.RndWat), bx, by);
                    SsprCell(RndSand(ti + 0.5, tj, result.RndWat), bx + CellPx, by);
                    SsprCell(RndSand(ti, tj + 0.5, result.RndWat), bx, by + CellPx);
                    SsprCell(RndSand(ti + 0.5, tj + 0.5, result.RndWat), bx + CellPx, by + CellPx);
                }
                else
                {
                    int b = SpriteBase(tId);
                    bool u = SameGroup(tiles, ti, tj - 1, tId);
                    bool d = SameGroup(tiles, ti, tj + 1, tId);
                    bool l = SameGroup(tiles, ti - 1, tj, tId);
                    bool r = SameGroup(tiles, ti + 1, tj, tId);
                    bool ul = SameGroup(tiles, ti - 1, tj - 1, tId);
                    bool ur = SameGroup(tiles, ti + 1, tj - 1, tId);
                    bool dl = SameGroup(tiles, ti - 1, tj + 1, tId);
                    bool dr = SameGroup(tiles, ti + 1, tj + 1, tId);

                    int tl = PcraftServices.CornerOffset(l, u, ul, RndCenter(ti, tj, result.RndWat), innerCorner: 20, hOnly: 1, vOnly: 16, outer: 0);
                    int tr = PcraftServices.CornerOffset(r, u, ur, RndCenter(ti + 0.5, tj, result.RndWat), innerCorner: 19, hOnly: 1, vOnly: 18, outer: 2);
                    int bl = PcraftServices.CornerOffset(l, d, dl, RndCenter(ti, tj + 0.5, result.RndWat), innerCorner: 4, hOnly: 33, vOnly: 16, outer: 32);
                    int br = PcraftServices.CornerOffset(r, d, dr, RndCenter(ti + 0.5, tj + 0.5, result.RndWat), innerCorner: 3, hOnly: 33, vOnly: 18, outer: 34);

                    SsprCell(b + tl, bx, by);
                    SsprCell(b + tr, bx + CellPx, by);
                    SsprCell(b + bl, bx, by + CellPx);
                    SsprCell(b + br, bx + CellPx, by + CellPx);
                }
            }
        }

        // Trees
        PcraftServices.SetPal(PcraftData.TileTree.SpritePal!);
        for (int ti = 0; ti < GridSize; ti++)
        {
            for (int tj = 0; tj < GridSize; tj++)
            {
                if (tiles[ti, tj] != 4) continue;

                int bx = offsetX + (ti * TilePx);
                int by = offsetY + (tj * TilePx);

                SsprCell(RndTree(ti, tj, result.RndWat) + 64, bx, by);
                SsprCell(RndTree(ti + 0.5, tj, result.RndWat) + 65, bx + CellPx, by);
                SsprCell(RndTree(ti, tj + 0.5, result.RndWat) + 80, bx, by + CellPx);
                SsprCell(RndTree(ti + 0.5, tj + 0.5, result.RndWat) + 81, bx + CellPx, by + CellPx);
            }
        }
        Pico8.Pal();

        // Hole
        for (int ti = 0; ti < GridSize; ti++)
        {
            for (int tj = 0; tj < GridSize; tj++)
            {
                if (tiles[ti, tj] != 11) continue;

                int bx = offsetX + (ti * TilePx);
                int by = offsetY + (tj * TilePx);

                Pico8.Palt(0, false);
                Pico8.Sspr(120, 8, 8, 16, bx, by, CellPx, TilePx);
                Pico8.Sspr(120, 8, 8, 16, bx + CellPx, by, CellPx, TilePx, flipX: true);
                Pico8.Palt();

                Pico8.Sspr(104, 32, 8, 16, bx + (CellPx / 2), by, CellPx, TilePx);
            }
        }

        // Spawn marker
        if (result.SpawnTileX >= 0)
            Pico8.Circfill(
                offsetX + (result.SpawnTileX * TilePx) + (TilePx / 2),
                offsetY + (result.SpawnTileY * TilePx) + (TilePx / 2),
                5, 8);
    }

    // -----------------------------------------------------------------------
    // Sspr helper: draws sprite N scaled to CellPx × CellPx at (destX, destY).
    // -----------------------------------------------------------------------

    private static void SsprCell(int n, int destX, int destY)
    {
        Pico8.Sspr(n % 16 * 8, n / 16 * 8, 8, 8, destX, destY, CellPx, CellPx);
    }

    private static int SpriteBase(int tileId)
    {
        return tileId switch { 0 => 26, 3 => 21, _ => 16 };
    }

    private static bool SameGroup(int[,] tiles, int ti, int tj, int tileId)
    {
        if (ti < 0 || ti >= GridSize || tj < 0 || tj >= GridSize) return true;
        static int Group(int id)
        {
            return id switch { 0 => 0, 1 => 1, 11 => 1, 3 => 3, _ => 2 };
        }

        return Group(tiles[ti, tj]) == Group(tileId);
    }

    private static F32 WatVal(double i, double j, double[,] rndWat)
    {
        int xi = (int)(i * 2 % 16);
        int yj = (int)(j * 2 % 16);
        if (xi < 0) xi += 16;
        if (yj < 0) yj += 16;
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
}
