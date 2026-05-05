namespace CSharpCraft.PcraftBase.Data;

internal sealed class DroppedItemEntity : Entity
{
    internal ItemDef Type  { get; }
    internal F32     Timer { get; set; }

    internal DroppedItemEntity(ItemDef type, F32 x, F32 y, F32 timer, F32 vx = default, F32 vy = default)
        : base(x, y, vx, vy)
    {
        Type  = type;
        Timer = timer;
    }
}
