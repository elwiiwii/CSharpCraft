namespace CSharpCraft.PcraftBase.Data;

internal abstract class Entity
{
    internal F32 X  { get; set; }
    internal F32 Y  { get; set; }
    internal F32 Vx { get; set; }
    internal F32 Vy { get; set; }

    protected Entity(F32 x, F32 y, F32 vx = default, F32 vy = default)
    {
        X = x; Y = y; Vx = vx; Vy = vy;
    }
}

