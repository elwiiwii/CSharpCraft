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
        bool needmap = true;

        while (needmap)
        {
            needmap = false;

            if (levelUnder)
            {
                level = CreateMapStep(levelsx, levelsy, 3, 8, 1, 9, 10, worldSeed + RoomHandler._curMatch.GameReports[^1].CaveIndex);

                if (typeCount[8] < 30) { needmap = true; }
                if (typeCount[9] < 20) { needmap = true; }
                if (typeCount[10] < 15) { needmap = true; }
            }
            else
            {
                level = CreateMapStep(levelsx, levelsy, 0, 1, 2, 3, 4, worldSeed + RoomHandler._curMatch.GameReports[^1].SurfaceIndex);

                if (typeCount[3] < 30) { needmap = true; }
                if (typeCount[4] < 30) { needmap = true; }
            }

            if (!needmap)
            {
                plx = F32.Neg1;
                ply = F32.Neg1;

                for (int i = 0; i <= 500; i++)
                {
                    int depx = F32.FloorToInt(levelsx / 8 + p8.Rnd(levelsx * 6 / 8, pSpawnRng));
                    int depy = F32.FloorToInt(levelsy / 8 + p8.Rnd(levelsy * 6 / 8, pSpawnRng));
                    F32 c = level[depx][depy];

                    if (c == 1 || c == 2)
                    {
                        plx = F32.FromInt(depx * 16 + 8);
                        ply = F32.FromInt(depy * 16 + 8);
                        break;
                    }
                }

                if (plx < 0)
                {
                    needmap = true;
                }
            }
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
