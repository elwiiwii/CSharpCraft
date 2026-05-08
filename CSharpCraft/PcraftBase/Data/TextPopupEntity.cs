namespace CSharpCraft.PcraftBase.Data;

internal sealed class TextPopupEntity : Entity
{
    internal F32 TextValue { get; }
    internal int TextColor { get; }
    internal F32 Timer { get; set; }

    internal TextPopupEntity(F32 textValue, int textColor, F32 x, F32 y, F32 vy)
        : base(x, y, vx: F32.Zero, vy: vy)
    {
        TextValue = textValue;
        TextColor = textColor;
        Timer = F32.FromInt(20);
    }
}
