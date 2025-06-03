using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using FixMath;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RaceServer;
using static System.Net.Mime.MediaTypeNames;
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
    private Pico8Functions? p8;

    public Selector(Pico8Functions _p8, (int x, int y) startpos, SelectorOption[] options)
    {
        p8 = _p8;
        StartPos = startpos;
        Options = options;
        Update(0, 0);
    }

    public void Update(float x, float y)
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

    public void Draw()
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

public class Button(Pico8Functions _p8, (int x, int y) startPos, string label, bool isActive)
{
    public (int x, int y) StartPos { get; set; } = startPos;
    public string Label { get; set; } = label;
    public bool IsActive { get; set; } = isActive;
    public bool IsHovered { get; internal set; } = false;
    private Pico8Functions? p8 = _p8;

    public void Update(float x, float y)
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

    public void Draw()
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
    private Pico8Functions? p8;

    public TextBox(Pico8Functions _p8, (int x, int y) startPos, (int min, int max) size, string label, Func<char, bool>? validator = null, int maxLength = int.MaxValue, bool isActive = false)
    {
        p8 = _p8;
        StartPos = startPos;
        Size = size;
        Label = label;
        Text = string.Empty;
        IsActive = isActive;
        inputValidator = validator;
        this.maxLength = maxLength;
        ActiveUpdate(0, 0);
    }

    public void ActiveUpdate(float x, float y)
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

    public void SetText(string text)
    {
        Text = text;
    }

    public bool HandleInput(char c)
    {
        if (!IsActive) return false;
        
        if (inputValidator is not null && !inputValidator(c)) return false;
        if (Text.Length >= maxLength) return false;
        
        Text += c;
        return true;
    }

    public bool HandlePaste(string text)
    {
        if (!IsActive) return false;
        
        var validText = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            if (inputValidator is null || inputValidator(c))
            {
                validText.Append(c);
            }
        }
        
        if (validText.Length > 0)
        {
            Text += validText.ToString();
            return true;
        }
        
        return false;
    }

    public bool HandleBackspace()
    {
        if (!IsActive || Text.Length == 0) return false;
        
        Text = Text[..^1];
        return true;
    }

    public void Draw()
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

public class PlayerList(Pico8Functions _p8, string roomName, string roomPassword, int startY)
{
    public int StartY { get; init; } = startY;
    public string RoomName { get; init; } = roomName;
    public string RoomPassword { get; init; } = roomPassword;
    public int Sel { get; private set; }
    private Pico8Functions p8 = _p8;
    private int lBound;
    private int rBound;

    public void Update(MouseState mouseState, MouseState prevMouseState)
    {
        float x = mouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        float y = mouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        int menuWidth = 16;
        if (RoomName.Length > menuWidth) { menuWidth = Math.Min(RoomName.Length, 26); }
        foreach (var player in RoomHandler._playerDictionary.Values)
        {
            if (player.Name.Length + 10 > menuWidth) { menuWidth = Math.Min(player.Name.Length, 16) + 10; }
        }
        lBound = 63 - menuWidth * 2 - 9;
        rBound = 63 + menuWidth * 2 + 9;

        if (x > lBound * p8.Cell.Width && x < rBound * p8.Cell.Width && y > StartY * p8.Cell.Height && y < (StartY + 78) * p8.Cell.Height)
        {
            if (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue)
            {
                Sel = Math.Max(0, Sel - 1);
            }
            else if (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
            {
                Sel = Math.Min(Math.Max(0, RoomHandler._playerDictionary.Count - 7), Sel + 1);
            }
        }
    }

    public void Draw()
    {
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        Vector2 halfSize = new(p8.Cell.Width / 2f, p8.Cell.Height / 2f);

        //int menuWidth = 16;
        //if (RoomName.Length > menuWidth) { menuWidth = Math.Min(RoomName.Length, 26); }
        //foreach (var player in RoomHandler._playerDictionary.Values)
        //{
        //    if (player.Name.Length + 10 > menuWidth) { menuWidth = Math.Min(player.Name.Length, 16) + 10; }
        //}
        //int x = 63 - menuWidth * 2 - 9;
        p8.Batch.Draw(p8.TextureDictionary["LobbyPlayerListContainer"], new Vector2(x * p8.Cell.Width, StartY * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["LobbyPlayerListContainer"], new Vector2((64 - x) * p8.Cell.Width, StartY * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        p8.Rectfill(63 - RoomName.Length * 2 - 1, StartY + 1, 63 + RoomName.Length * 2 + 1, StartY + 7, 13);
        Shared.Printc(p8, RoomName, 64, StartY + 2, 7);
        Shared.Printc(p8, $"password-{RoomPassword}", 64, StartY + 12, 7);

        int i = 0;
        foreach (RoomUser player in RoomHandler._playerDictionary.Values)
        {
            if (i >= Sel && i < Sel + 7)
            {
                p8.Batch.Draw(p8.TextureDictionary[$"{player.Role}Icon"], new Vector2((x + 8) * p8.Cell.Width, (StartY + 21 + (i - Sel) * 7) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
                p8.Print(player.Name, x + 19, StartY + 21 + (i - Sel) * 7, 7);
                if (player.Ready)
                {
                    p8.Batch.Draw(p8.TextureDictionary["Tick"], new Vector2((x + 20 + player.Name.Length * 4) * p8.Cell.Width, (StartY + 21 + (i - Sel) * 7) * p8.Cell.Height), null, p8.Colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                }
                if (player.Host)
                {
                    p8.Print("[", 127 - x - 31, StartY + 21 + (i - Sel) * 7, 5);
                    p8.Print("host", 127 - x - 28, StartY + 21 + (i - Sel) * 7, 5);
                    p8.Print("]", 127 - x - 13, StartY + 21 + (i - Sel) * 7, 5);
                }
                i++;
            }
        }
        i = Math.Max(7, RoomHandler._playerDictionary.Count - 7);
        int scrollBarX = 127 - x - 8;
        int scrollBarY = StartY + 19;
        p8.Rectfill(scrollBarX, scrollBarY, scrollBarX + 2, scrollBarY + 51, 13);
        p8.Pset(F32.FromInt(scrollBarX) + 2, F32.FromInt(scrollBarY) + 51, 1);
        double range = 48.0 / Math.Max(7, RoomHandler._playerDictionary.Count);
        p8.Rectfill(scrollBarX + 1, StartY + 20 + Sel * range, scrollBarX + 1, StartY + 20 + (Sel + 7) * range, 6);
    }
}

public class Item
{
    public string Name { get; set; }
    public bool Active { get; set; }
    public Func<Task> Method { get; set; }
}

public class RoomSettings
{
    public (int x, int y) StartPos { get; init; }
    public string Title { get; init; }
    public List<Item> Items { get; init; }
    public int Sel { get; private set; }
    private Pico8Functions p8;
    private int rBound;

    public RoomSettings(Pico8Functions _p8, (int x, int y) startPos, string title, List<Item> items)
    {
        StartPos = startPos;
        Title = title;
        Items = items;
        p8 = _p8;
    }

    public void Update(MouseState mouseState, MouseState prevMouseState)
    {
        float x = mouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        float y = mouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        int menuWidth = 8;
        if (Title.Length + 1 > menuWidth) { menuWidth = Title.Length + 1; }
        foreach (var item in Items)
        {
            menuWidth = Math.Max(item.Name.Length, menuWidth);
        }
        rBound = StartPos.x + 3 + (menuWidth - 8) * 4 + 40;

        if (x > StartPos.x * p8.Cell.Width && x < rBound * p8.Cell.Width && y > StartPos.y * p8.Cell.Height && y < (StartPos.y + 40) * p8.Cell.Height)
        {
            if (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue)
            {
                Sel = Math.Max(0, Sel - 1);
            }
            else if (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
            {
                Sel = Math.Min(Math.Max(0, Items.Count - 4), Sel + 1);
            }
        }
    }

    public void Draw()
    {
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        p8.Batch.Draw(p8.TextureDictionary["LobbySettingsContainer"], new Vector2(StartPos.x * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["LobbySettingsContainer"], new Vector2((rBound - 40) * p8.Cell.Width, StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);

        int centerX = StartPos.x + (rBound - StartPos.x) / 2;
        p8.Rectfill(centerX - Title.Length * 2 - 1, StartPos.y + 2, centerX + Title.Length * 2 - 1, StartPos.y + 7, 13);
        Shared.Printc(p8, Title, centerX, StartPos.y + 2, 7);

        int i = 0;
        foreach (var item in Items)
        {
            if (i >= Sel && i < Sel + 4)
            {
                p8.Print(item.Name, StartPos.x + 4, StartPos.y + 10 + (i - Sel) * 7, item.Active ? 7 : 0);
            }
            i++;
        }
        i = Math.Max(4, Items.Count - 4);
        int scrollBarX = rBound - 6;
        int scrollBarY = StartPos.y + 3;
        p8.Rectfill(scrollBarX, scrollBarY, scrollBarX + 2, scrollBarY + 33, 13);
        p8.Pset(F32.FromInt(scrollBarX) + 2, F32.FromInt(scrollBarY), 1);
        p8.Pset(F32.FromInt(scrollBarX) + 2, F32.FromInt(scrollBarY) + 33, 1);
        double range = 29.0 / Math.Max(4, Items.Count);
        p8.Rectfill(scrollBarX + 1, StartPos.y + 5 + Sel * range, scrollBarX + 1, StartPos.y + 5 + (Sel + 4) * range, 6);
    }
}