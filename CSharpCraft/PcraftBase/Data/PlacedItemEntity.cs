namespace CSharpCraft.PcraftBase.Data;

internal sealed class PlacedItemEntity : Entity
{
    internal PlaceableItemDef Type { get; }

    internal PlacedItemEntity(PlaceableItemDef type, F32 x, F32 y)
        : base(x, y)
    {
        Type = type;
    }
}
