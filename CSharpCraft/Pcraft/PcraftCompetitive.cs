using System;
using CSharpCraft.Competitive;
using CSharpCraft.Pico8;
using FixMath;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Pcraft;

public class PcraftCompetitive : SpeedrunBase
{
    public override string SceneName => "comp_pcraft";

    public override void Init(Pico8Functions pico8)
    {
        base.Init(pico8);
    }

    protected override void CreateMap()
    {
        if (levelUnder)
        {
            level = CreateMapStep(levelsx, levelsy, 3, 8, 1, 9, 10, worldSeed + RoomHandler._curMatch.GameReports[^1].CaveIndex);
        }
        else
        {
            level = CreateMapStep(levelsx, levelsy, 0, 1, 2, 3, 4, worldSeed + RoomHandler._curMatch.GameReports[^1].SurfaceIndex);
        }

        plx = F32.FromInt((levelsx / 2 + 1) * 16 + 8);
        ply = F32.FromInt((levelsy / 2) * 16 + 8);

        List<(int x, int y)> spawnableTiles = [];

        for (int i = -4; i <= 4; i++)
        {
            if (i == 0) { continue; }
            for (int j = -4; j <= 4; j++)
            {
                if (j == 0) { continue; }
                int depx = levelsx / 2 + i;
                int depy = levelsy / 2 + j;
                F32 c = level[depx][depy];

                if (c == 1 || c == 2)
                {
                    spawnableTiles.Add((depx, depy));
                }
            }
        }

        if (spawnableTiles.Count > 0)
        {
            int indx = F32.FloorToInt(p8.Rnd(spawnableTiles.Count, pSpawnRng));
            (int x, int y) tile = spawnableTiles[indx];

            plx = F32.FromInt(tile.x * 16 + 8);
            ply = F32.FromInt(tile.y * 16 + 8);
        }

        for (int i = 0; i < levelsx; i++)
        {
            for (int j = 0; j < levelsy; j++)
            {
                p8.Mset(i + levelx, j + levely, level[i][j].Double);
            }
        }

        holex = levelsx / 2 + levelx;
        holey = levelsy / 2 + levely;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                p8.Mset(holex + i, holey + j, levelUnder ? 1 : 3);
            }
        }

        p8.Mset(holex, holey, 11);

        clx = plx;
        cly = ply;

        cmx = plx;
        cmy = ply;
    }
}
