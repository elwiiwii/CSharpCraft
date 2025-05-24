using System.Runtime.CompilerServices;
using CSharpCraft.Pico8;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public class Icon
{
    public (int x, int y) StartPos { get; init; }
    public (int x, int y) EndPos { get; init; }
    public (float x, float y) Offset { get; init; } = (-0.6f, -0.6f);
    public string Label { get; init; } = string.Empty;
    public string? ShadowTexture { get; init; }
    public string? IconTexture { get; init; }
    public IScene? Scene { get; init; }
}

public class SelectorOption
{
    public string Name { get; init; } = string.Empty;
    public (int x1, int x2) Area { get; set; }
    public SelectorOption[]? Options { get; init; } = null;
}

public class Selector
{
    public (int x, int y) StartPos { get; internal init; }
    public int Sel { get; set; } = 0;
    public SelectorOption[] Options { get; internal init; } = [];

    public Selector(Pico8Functions p8, (int x, int y) startpos, SelectorOption[] options)
    {
        StartPos = startpos;
        Options = options;
        Update(p8, 0, 0);
    }

    public void Update(Pico8Functions p8, float x, float y)
    {
        for (int i = 0; i < Options.Length; i++)
        {
            if (x > Options[i].Area.x1 * p8.Cell.Width && x < Options[i].Area.x2 * p8.Cell.Width && y > StartPos.y * p8.Cell.Height && y < (StartPos.y + 9) * p8.Cell.Height)
            {
                Sel = i;
            }
        }
        int x1;
        int x2 = StartPos.x;
        for (int i = 0; i < Options.Length; i++)
        {
            x1 = x2;
            x2 += (Options[i].Name.Length * 4) + 1 + (Sel == i ? 10 : 5);
            Options[i].Area = (x1, x2);
        }
    }

    public void Draw(Pico8Functions p8)
    {
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        for (int i = 0; i < Options.Length; i++)
        {
            int x = (i == 0 || i == Sel) ? 5 : -4;
            int w = i == Sel ? Options[i].Name.Length * 4 + 1 : Options[i].Name.Length * 4 + 5;
            p8.Batch.Draw(p8.TextureDictionary[$"10px{(i == Sel ? "Highlight" : "Background")}Center"], new((Options[i].Area.x1 + x) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, new Vector2(p8.Cell.Width * w, p8.Cell.Height), SpriteEffects.None, 0);
            p8.Print(Options[i].Name, Options[i].Area.x1 + ((i == 0 || i == Sel) ? 6 : 1), StartPos.y + 2, i == Sel ? 15 : 29);
        }
        for (int i = 0; i < Options.Length; i++)
        {
            int x = (i == 0 || i == Sel) ? 5 : -4;
            if (Sel >= i) { p8.Batch.Draw(p8.TextureDictionary[$"10px{(i == Sel ? "Highlight" : "Background")}{(i <= 0 ? "" : "Overlap")}Edge"], new((Options[i].Area.x1 + x - 5) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
            if (Sel <= i) { p8.Batch.Draw(p8.TextureDictionary[$"10px{(i == Sel ? "Highlight" : "Background")}{(i >= Options.Length - 1 ? "" : "Overlap")}Edge"], new((Options[i].Area.x2 - 5) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0); }
        }
    }
}

public class Button
{
    public (int x, int y) StartPos { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsHovered { get; internal set; } = false;

    public Button((int x, int y) startPos, string label, bool isActive)
    {
        StartPos = startPos;
        Label = label;
        IsActive = isActive;
        IsHovered = false;
    }

    public void Update(Pico8Functions p8, float x, float y)
    {
        if (IsActive && x > StartPos.x * p8.Cell.Width && x < (StartPos.x + (Label.Length * 4) + 11) * p8.Cell.Width && y > StartPos.y * p8.Cell.Height && y < (StartPos.y + 9) * p8.Cell.Height)
        {
            IsHovered = true;
        }
        else
        {
            IsHovered = false;
        }
    }

    public void Draw(Pico8Functions p8)
    {
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        p8.Batch.Draw(p8.TextureDictionary[$"10px{(IsHovered ? "Highlight" : "Background")}Center"], new((StartPos.x + 5) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, new Vector2(p8.Cell.Width * (Label.Length * 4 + 1), p8.Cell.Height), SpriteEffects.None, 0);
        p8.Print(Label, StartPos.x + 6, StartPos.y + 2, IsHovered ? 15 : 29);
        p8.Batch.Draw(p8.TextureDictionary[$"10px{(IsHovered ? "Highlight" : "Background")}Edge"], new(StartPos.x * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary[$"10px{(IsHovered ? "Highlight" : "Background")}Edge"], new((StartPos.x + (Label.Length * 4) + 6) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
    }
}

public class TextBox
{
    public (int x, int y) StartPos { get; set; }
    public (int min, int max) Size { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public bool IsActive { get; set; } = false;
    private int frameCount;
    private Func<char, bool>? inputValidator;
    private int maxLength;

    public TextBox(Pico8Functions p8, (int x, int y) startPos, (int min, int max) size, string label, Func<char, bool>? validator = null, int maxLength = int.MaxValue, bool isActive = false)
    {
        StartPos = startPos;
        Size = size;
        Label = label;
        Text = string.Empty;
        IsActive = isActive;
        inputValidator = validator;
        this.maxLength = maxLength;
        ActiveUpdate(p8, 0, 0);
    }

    public void ActiveUpdate(Pico8Functions p8, float x, float y)
    {
        if (x > StartPos.x * p8.Cell.Width && x < Math.Max(Math.Min((Label + Text).Length + 1, StartPos.x + Size.max), StartPos.x + Size.min) * p8.Cell.Width && y > StartPos.y * p8.Cell.Height && y < (StartPos.y + 9) * p8.Cell.Height)
        {
            IsActive = true;
            frameCount = 0;
        }
        else
        {
            IsActive = false;
        }
    }

    public bool HandleInput(char c)
    {
        if (!IsActive) return false;
        
        if (inputValidator is not null && !inputValidator(c)) return false;
        if (Text.Length >= maxLength) return false;
        
        Text += c;
        return true;
    }

    public bool HandleBackspace()
    {
        if (!IsActive || Text.Length == 0) return false;
        
        Text = Text[..^1];
        return true;
    }

    public void Draw(Pico8Functions p8)
    {
        frameCount++;
        string indicator = " ";
        if (IsActive && frameCount / 30 % 2 == 0) { indicator = "|"; }
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        p8.Batch.Draw(p8.TextureDictionary[$"10pxBackgroundCenter"], new((StartPos.x + 5) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, new Vector2(p8.Cell.Width * Math.Max(Math.Min((Label + Text).Length + 1, Size.max - 10), Size.min - 10), p8.Cell.Height), SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary[$"10pxBackgroundEdge"], new(StartPos.x * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary[$"10pxBackgroundEdge"], new(Math.Max(Math.Min((Label + Text).Length + 1, StartPos.x + Size.max - 5), StartPos.x + Size.min - 5) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        string s = Label + Text + indicator;
        int startIndex = !IsActive ? 0 : Math.Max(0, s.Length - ((Size.max - 12) / 4));
        int length = s.Length > ((Size.max - 11) / 4) ? ((Size.max - 11) / 4) : s.Length;
        if (startIndex + length > s.Length) { length = s.Length - startIndex; }
        p8.Print(s.Substring(startIndex, length), StartPos.x + 6, StartPos.y + 2, 29);
    }
}