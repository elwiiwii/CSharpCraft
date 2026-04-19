using CSharpCraft.PcraftBase.Map;
using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Data;

internal sealed class PlayerEntity : CharacterEntity
{
    internal F32             Stam       { get; set; } = F32.Zero;
    internal F32             Lstam      { get; set; } = F32.Zero;
    internal F32             Llife      { get; set; } = F32.Zero;
    internal bool            Lb4        { get; set; } = false;
    internal bool            Lb5        { get; set; } = false;
    internal bool            Block5     { get; set; } = false;
    internal List<ItemStack> Invent     { get; } = [];
    internal ItemStack?      CurItem    { get; set; } = null;
    internal GroundType      LastGround { get; set; } = PcraftData.GrSand;
    internal IMenu?          CurMenu    { get; set; } = null;
    internal CameraState     Camera     { get; } = new();

    internal PlayerEntity(F32 x, F32 y, F32 vx = default, F32 vy = default)
        : base(x, y, vx, vy) { }
}
