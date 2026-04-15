namespace CSharpCraft.PcraftBase.Data;

internal sealed class ZombieEntity : CharacterEntity
{
    internal ZombieEntity(F32 x, F32 y, F32 vx = default, F32 vy = default)
        : base(x, y, vx, vy)
    {
    }
}
