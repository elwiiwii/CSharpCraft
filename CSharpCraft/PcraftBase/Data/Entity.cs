namespace CSharpCraft.PcraftBase.Data;

internal abstract class Entity(F32 x, F32 y, F32 vx = default, F32 vy = default)
{
    internal F32 X { get; set; } = x;
    internal F32 Y { get; set; } = y;
    internal F32 Vx { get; set; } = vx;
    internal F32 Vy { get; set; } = vy;
}

