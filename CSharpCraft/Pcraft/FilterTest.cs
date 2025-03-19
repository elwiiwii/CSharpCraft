using CSharpCraft.Pico8;
using FixMath;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSharpCraft.Pcraft
{
    public class FilterTest : PcraftBase
    {
        public override string SceneName => "filter";

        private class Condition
        {
            public int Rlb { get; set; }
            public int Rub { get; set; }
            public double Clb { get; set; }
            public double Cub { get; set; }
        }

        private bool Filter(Condition? water = null, Condition? sand = null, Condition? grass = null, Condition? stone = null, Condition? tree = null, Condition? iron = null, Condition? gold = null, Condition? gem = null)
        {
            int[] counts = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            Condition?[] targets = [water, sand, grass, stone, tree, null, null, null, iron, gold, gem];
            int radius = Math.Min(levelsx/2-1, MaxRadius(targets));
            int center = levelsx/2;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    Condition? curtile = targets[F32.FloorToInt(level[center + i][center + j])];
                    if (curtile is null || Math.Abs(i) < curtile.Rlb || Math.Abs(i) > curtile.Rub || Math.Abs(j) < curtile.Rlb || Math.Abs(j) > curtile.Rub)
                    {
                        continue;
                    }
                    counts[F32.FloorToInt(level[center + i][center + j])] += 1;
                }
            }

            for (int i = 0; i < counts.Length; i++)
            {
                if (targets[i] is null) { continue; }
                int area = ((targets[i].Rub + targets[i].Rub * targets[i].Rub) - (targets[i].Rlb + targets[i].Rlb * targets[i].Rlb)) * 4;

                if (counts[i] < area * (targets[i].Clb / 100.0) || counts[i] > area * (targets[i].Cub / 100.0))
                {
                    return true;
                }
            }

            return false;
        }

        private int MaxRadius(Condition?[] targets)
        {
            int mval = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] is not null && targets[i].Rub > mval)
                {
                    mval = targets[i].Rub;
                }
            }
            return mval;
        }

        public override void CreateMap()
        {
            bool needmap = true;

            while (needmap)
            {
                needmap = false;
                holex = levelsx / 2 + levelx;
                holey = levelsy / 2 + levely;

                if (levelUnder)
                {
                    level = CreateMapStep(levelsx, levelsy, 3, 8, 1, 9, 10);

                    if (typeCount[8] < 30) { needmap = true; }
                    if (typeCount[9] < 20) { needmap = true; }
                    if (typeCount[10] < 15) { needmap = true; }
                }
                else
                {
                    level = CreateMapStep(levelsx, levelsy, 0, 1, 2, 3, 4);

                    if (typeCount[3] < 30) { needmap = true; }
                    if (typeCount[4] < 30) { needmap = true; }
                    needmap = Filter(new Condition { Rlb = 0, Rub = 8, Clb = 0.1, Cub = 5 }, // water
                        new Condition { Rlb = 0, Rub = 1, Clb = 0, Cub = 100 }, // sand
                        new Condition { Rlb = 0, Rub = 1, Clb = 0, Cub = 100 }, // grass
                        new Condition { Rlb = 1, Rub = 3, Clb = 20, Cub = 30 }, // stone
                        new Condition { Rlb = 0, Rub = 10, Clb = 0, Cub = 100 }); // tree
                }

                if (!needmap)
                {
                    plx = F32.Neg1;
                    ply = F32.Neg1;

                    for (int i = 0; i <= 500; i++)
                    {
                        int depx = F32.FloorToInt(levelsx / 2 - 4 + p8.Rnd(9));
                        int depy = F32.FloorToInt(levelsy / 2 - 4 + p8.Rnd(9));
                        F32 c = level[depx][depy];

                        if ((c == 1 || c == 2) && !(depx == holex && depy == holey))
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

        public override void Init(Pico8Functions pico8)
        {
            base.Init(pico8);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}
