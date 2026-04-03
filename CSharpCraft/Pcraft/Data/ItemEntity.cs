namespace CSharpCraft.Pcraft.Data;

internal sealed class ItemEntity : Entity
{
    public ItemDef Type { get; }
    internal ItemDef? GiveItem { get; set; }
    internal F32? Timer { get; set; }
    internal bool HasCol { get; set; }
    internal List<Recipe>? List { get; set; }
    internal F32 TextValue { get; set; }
    internal int TextColor { get; set; }

    internal ItemEntity(ItemDef type, F32 x, F32 y, F32 vx = default, F32 vy = default)
        : base(x, y, vx, vy)
    {
        Type = type;
    }
}
