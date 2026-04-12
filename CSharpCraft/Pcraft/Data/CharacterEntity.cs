namespace CSharpCraft.Pcraft.Data;

internal abstract class CharacterEntity : Entity
{
    internal F32 Life  { get; set; }
    internal F32 Prot  { get; set; }
    internal F32 Lrot  { get; set; }
    internal F32 Panim { get; set; }
    internal F32 Banim { get; set; }
    internal EnStep Step  { get; set; }
    internal F32 Dtim  { get; set; }
    internal F32 Dx    { get; set; }
    internal F32 Dy    { get; set; }
    internal F32 Ox    { get; set; }
    internal F32 Oy    { get; set; }

    protected CharacterEntity(F32 x, F32 y, F32 vx = default, F32 vy = default)
        : base(x, y, vx, vy)
    {
    }
}
