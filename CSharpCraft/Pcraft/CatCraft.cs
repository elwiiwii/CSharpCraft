using CSharpCraft.Pico8;
using FixMath;
using System.Diagnostics;

namespace CSharpCraft.Pcraft;

public class CatCraft : PcraftBase
{
    public override string SceneName => "CatCraft";

    protected static readonly Material kitty = Item(null, 2);
    protected static List<Entity> ThisCat = [];

    protected virtual void FillCat(Level l)
    {
        l.Cat = [Entity(player, F32.Zero, F32.Zero, F32.Zero, F32.Zero)];
        ThisCat = l.Cat;
        for (F32 i = F32.Zero; i < levelsx; i++)
        {
            for (F32 j = F32.Zero; j < levelsy; j++)
            {
                Ground c = GetDirectGr(i, j);
                F32 r = p8.Rnd(100);
                F32 ex = i * 16 + 8;
                F32 ey = j * 16 + 8;
                F32 dist = F32.Max(F32.Abs(ex - plx), F32.Abs(ey - ply));
                if (r < 90 && c != grwater && c != grrock && !c.IsTree && dist > 20)
                {
                    Entity ncat = Entity(zombi, ex, ey, F32.Zero, F32.Zero);
                    ncat.Life = F32.FromInt(10);
                    ncat.Prot = F32.Zero;
                    ncat.Lrot = F32.Zero;
                    ncat.Panim = F32.Zero;
                    ncat.Banim = F32.Zero;
                    ncat.Dtim = F32.Zero;
                    ncat.Step = 0;
                    ncat.Ox = F32.Zero;
                    ncat.Oy = F32.Zero;
                    p8.Add(l.Cat, ncat);
                }
            }
        }
    }
    protected override Level CreateLevel(int xx, int yy, int sizex, int sizey, bool IsUnderground)
    {
        Level l = new() { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = IsUnderground, Ent = [], Ene = [], Cat = [], Dat = new F32[8192]};
        SetLevel(l);
        levelUnder = IsUnderground;
        CreateMap();
        FillEne(l);
        FillCat(l);
        l.Stx = F32.FromInt((holex - levelx) * 16 + 8);
        l.Sty = F32.FromInt((holey - levely) * 16 + 8);
        return l;
    }
    protected override void SetLevel(Level l)
    {
        currentLevel = l;
        levelx = l.X;
        levely = l.Y;
        levelsx = l.Sx;
        levelsy = l.Sy;
        levelUnder = l.IsUnder;
        entities = l.Ent;
        enemies = l.Ene;
        ThisCat = l.Cat;
        data = l.Dat;
        plx = l.Stx;
        ply = l.Sty;
    }
        protected virtual void Dcat()
    {
        Sorty(ThisCat);

        foreach (Entity e in ThisCat)
        {
            if (e.Type == player)
            {
                p8.Pal();
                Dplayer(plx, ply, prot, panim, banim, true);
            }
            else
            {
                if (IsIn(e, 72))
                {
                    p8.Pal();
                    p8.Pal(13, 3);
                    p8.Pal(7, 1);
                    p8.Pal(7, 8);
                    p8.Pal(3, 1);

                    Dkitty(e.X, e.Y, e.Prot, e.Panim, e.Banim, false);
                }
            }
        }
    }
    protected virtual void Dkitty(F32 x, F32 y)

    public override string SpriteImage => "SpriteSheet1";
}
   