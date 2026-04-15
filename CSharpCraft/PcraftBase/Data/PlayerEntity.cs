namespace CSharpCraft.PcraftBase.Data;

internal sealed class PlayerEntity : CharacterEntity
{
    internal PlayerEntity(F32 x, F32 y, F32 vx = default, F32 vy = default)
        : base(x, y, vx, vy)
    {
    }
}
