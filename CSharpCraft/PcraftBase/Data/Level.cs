namespace CSharpCraft.PcraftBase.Data;

internal sealed class Level
{
    internal int X { get; }
    internal int Y { get; }
    internal int Sx { get; }
    internal int Sy { get; }
    internal bool IsUnder { get; }
    internal List<Entity> Ent { get; } = [];
    internal List<CharacterEntity> Ene { get; } = [];
    internal Dictionary<int, F32> Dat { get; } = [];
    internal F32 Stx { get; set; }
    internal F32 Sty { get; set; }
    internal F32 Time { get; set; } = F32.Zero;
    internal F32[][] RndWat { get; }

    internal Level(int x, int y, int sx, int sy, bool isUnder)
    {
        X = x;
        Y = y;
        Sx = sx;
        Sy = sy;
        IsUnder = isUnder;
        RndWat = new F32[16][];
        for (int i = 0; i < 16; i++)
            RndWat[i] = new F32[16];
    }
}
