using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8.Graphics;

public class DrawState
{
    public int CursorX { get; set; }
    public int CursorY { get; set; }
    public Color ForegroundColor { get; set; } = Color.White;
    public bool Wide { get; set; }
    public bool Tall { get; set; }
    public bool Invert { get; set; }
    public bool SolidBackground { get; set; }
    public bool Underline { get; set; }
    public bool Padding { get; set; }
    public Color BackgroundColor { get; set; } = Color.Black;
    public int TabWidth { get; set; } = 4;
    public Color? OutlineColor { get; set; }
    public byte OutlineMask { get; set; } = 0xFF;
    public int HomeX { get; set; }
    public int HomeY { get; set; }
    public int RightBorder { get; set; } = int.MaxValue;

    public void Reset()
    {
        Wide            = false;
        Tall            = false;
        Invert          = false;
        SolidBackground = false;
        Underline       = false;
        Padding         = false;
        BackgroundColor = Color.Black;
        TabWidth        = 4;
        OutlineColor    = null;
        OutlineMask     = 0xFF;
        HomeX           = CursorX;
        HomeY           = CursorY;
        RightBorder     = int.MaxValue;
    }
}
