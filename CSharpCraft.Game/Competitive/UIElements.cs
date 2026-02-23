using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using FixMath;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
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

    public Selector((int x, int y) startpos, SelectorOption[] options)
    {
        StartPos = startpos;
        Options = options;
        Update(0, 0);
    }

    public void Update(float x, float y)
    {
        for (int i = 0; i < Options.Length; i++)
        {
            if (x > Options[i].Area.x1 * CellWidth && x < Options[i].Area.x2 * CellWidth && y > StartPos.y * CellHeight && y < (StartPos.y + 9) * CellHeight)
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
        for (int i = 0; i < Options.Length; i++)
        {
            int x = (i == 0 || i == Sel) ? 5 : -4;
            int w = i == Sel ? Options[i].Name.Length * 4 + 1 : Options[i].Name.Length * 4 + 5;
            GameRendering.Current.Draw($"10px{(i == Sel ? "Highlight" : "Background")}Center", Options[i].Area.x1 + x, StartPos.y, Color.White, scaleX: w);
            Print(Options[i].Name, Options[i].Area.x1 + ((i == 0 || i == Sel) ? 6 : 1), StartPos.y + 2, i == Sel ? 15 : 29);
        }
        for (int i = 0; i < Options.Length; i++)
        {
            int x = (i == 0 || i == Sel) ? 5 : -4;
            if (Sel >= i) { GameRendering.Current.Draw($"10px{(i == Sel ? "Highlight" : "Background")}{(i <= 0 ? "" : "Overlap")}Edge", Options[i].Area.x1 + x - 5, StartPos.y, Color.White); }
            if (Sel <= i) { GameRendering.Current.Draw($"10px{(i == Sel ? "Highlight" : "Background")}{(i >= Options.Length - 1 ? "" : "Overlap")}Edge", Options[i].Area.x2 - 5, StartPos.y, Color.White, flipX: true); }
        }
    }
}

public class Button((int x, int y) startPos, string label, bool isActive)
{
    public (int x, int y) StartPos { get; set; } = startPos;
    public string Label { get; set; } = label;
    public bool IsActive { get; set; } = isActive;
    public bool IsHovered { get; internal set; } = false;

    public void Update(float x, float y)
    {
        if (IsActive && x > StartPos.x * CellWidth && x < (StartPos.x + (Label.Length * 4) + 11) * CellWidth && y > StartPos.y * CellHeight && y < (StartPos.y + 9) * CellHeight)
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
        GameRendering.Current.Draw($"10px{(IsHovered ? "Highlight" : "Background")}Center", StartPos.x + 5, StartPos.y, Color.White, scaleX: Label.Length * 4 + 1);
        Print(Label, StartPos.x + 6, StartPos.y + 2, IsHovered ? 15 : 29);
        GameRendering.Current.Draw($"10px{(IsHovered ? "Highlight" : "Background")}Edge", StartPos.x, StartPos.y, Color.White);
        GameRendering.Current.Draw($"10px{(IsHovered ? "Highlight" : "Background")}Edge", StartPos.x + (Label.Length * 4) + 6, StartPos.y, Color.White, flipX: true);
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

    public TextBox((int x, int y) startPos, (int min, int max) size, string label, Func<char, bool>? validator = null, int maxLength = int.MaxValue, bool isActive = false)
    {
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
        if (x > StartPos.x * CellWidth && x < Math.Max(Math.Min((Label + Text).Length + 1, StartPos.x + Size.max), StartPos.x + Size.min) * CellWidth && y > StartPos.y * CellHeight && y < (StartPos.y + 9) * CellHeight)
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
        GameRendering.Current.Draw($"10pxBackgroundCenter", StartPos.x + 5, StartPos.y, Color.White, scaleX: Math.Max(Math.Min((Label + Text).Length + 1, Size.max - 10), Size.min - 10));
        GameRendering.Current.Draw($"10pxBackgroundEdge", StartPos.x, StartPos.y, Color.White);
        GameRendering.Current.Draw($"10pxBackgroundEdge", Math.Max(Math.Min((Label + Text).Length + 1, StartPos.x + Size.max - 5), StartPos.x + Size.min - 5), StartPos.y, Color.White, flipX: true);
        string s = Label + Text + indicator;
        int startIndex = !IsActive ? 0 : Math.Max(0, s.Length - ((Size.max - 12) / 4));
        int length = s.Length > ((Size.max - 11) / 4) ? ((Size.max - 11) / 4) : s.Length;
        if (startIndex + length > s.Length) { length = s.Length - startIndex; }
        Print(s.Substring(startIndex, length), StartPos.x + 6, StartPos.y + 2, 29);
    }
}

public class PlayerList(string roomName, string roomPassword, int startY)
{
    public int StartY { get; init; } = startY;
    public string RoomName { get; init; } = roomName;
    public string RoomPassword { get; init; } = roomPassword;
    public int Sel { get; private set; }
    private int lBound;
    private int rBound;

    public void Update(MouseState mouseState, MouseState prevMouseState)
    {
        var cursor = GameRendering.Current.GetCursorPosition(mouseState.X, mouseState.Y);
        float x = cursor.X;
        float y = cursor.Y;

        int menuWidth = 16;
        if (RoomName.Length > menuWidth) { menuWidth = Math.Min(RoomName.Length, 26); }
        foreach (var player in RoomHandler._playerDictionary.Values)
        {
            if (player.Name.Length + 10 > menuWidth) { menuWidth = Math.Min(player.Name.Length, 16) + 10; }
        }
        lBound = 63 - menuWidth * 2 - 6;
        rBound = 63 + menuWidth * 2 + 6;

        if (x > lBound * CellWidth && x < rBound * CellWidth && y > StartY * CellHeight && y < (StartY + 74) * CellHeight)
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
        //int menuWidth = 16;
        //if (RoomName.Length > menuWidth) { menuWidth = Math.Min(RoomName.Length, 26); }
        //foreach (var player in RoomHandler._playerDictionary.Values)
        //{
        //    if (player.Name.Length + 10 > menuWidth) { menuWidth = Math.Min(player.Name.Length, 16) + 10; }
        //}
        //int x = 63 - menuWidth * 2 - 9;
        GameRendering.Current.Draw("LobbyPlayerListContainer", lBound - 3, StartY, Color.White);
        GameRendering.Current.Draw("LobbyPlayerListContainer", rBound - 61, StartY, Color.White, flipX: true);
        Rectfill(63 - RoomName.Length * 2 - 1, StartY + 1, 63 + RoomName.Length * 2 + 1, StartY + 7, 13);
        Shared.Printc(RoomName, 64, StartY + 2, 7);
        Shared.Printc($"password-{RoomPassword}", 64, StartY + 12, 7);

        int i = 0;
        foreach (RoomUser player in RoomHandler._playerDictionary.Values)
        {
            if (i >= Sel && i < Sel + 7)
            {
                GameRendering.Current.Draw($"{player.Role}Icon", lBound + 5, StartY + (player.Role == Role.Player ? 21 : 20.75f) + (i - Sel) * 7, Color.White, 0.5, 0.5);
                Print(player.Name, lBound + 16, StartY + 21 + (i - Sel) * 7, 7);
                if (player.Ready)
                {
                    GameRendering.Current.Draw("Tick", lBound + 17 + player.Name.Length * 4, StartY + 21 + (i - Sel) * 7, Colors[6]);
                }
                if (player.Host)
                {
                    Print("[", rBound - 29, StartY + 21 + (i - Sel) * 7, 5);
                    Print("host", rBound - 26, StartY + 21 + (i - Sel) * 7, 5);
                    Print("]", rBound - 11, StartY + 21 + (i - Sel) * 7, 5);
                }
                i++;
            }
        }
        i = Math.Max(7, RoomHandler._playerDictionary.Count - 7);
        int scrollBarX = rBound - 6;
        int scrollBarY = StartY + 19;
        Rectfill(scrollBarX, scrollBarY, scrollBarX + 2, scrollBarY + 51, 13);
        Pset(F32.FromInt(scrollBarX) + 2, F32.FromInt(scrollBarY) + 51, 1);
        double range = 48.0 / Math.Max(7, RoomHandler._playerDictionary.Count);
        Rectfill(scrollBarX + 1, StartY + 20 + Sel * range, scrollBarX + 1, StartY + 20 + (Sel + 7) * range, 6);
    }
}

public class Item(string name, bool active, Func<Task>? onLeftClick = null, Func<Task>? onRightClick = null)
{
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public Func<Task>? OnLeftClick { get; set; } = onLeftClick;
    public Func<Task>? OnRightClick { get; set; } = onRightClick;
}

public class RoomSettings
{
    public (int x, int y) StartPos { get; init; }
    public string Title { get; init; }
    public List<Item> Items { get; init; }
    private int scrollIndex;
    private int? sel;
    private int rBound;

    public RoomSettings((int x, int y) startPos, string title, List<Item> items)
    {
        StartPos = startPos;
        Title = title;
        Items = items;
    }

    public void Update(MouseState mouseState, MouseState prevMouseState)
    {
        var cursor = GameRendering.Current.GetCursorPosition(mouseState.X, mouseState.Y);
        float x = cursor.X;
        float y = cursor.Y;

        int menuWidth = 8;
        if (Title.Length + 1 > menuWidth) { menuWidth = Title.Length + 1; }
        foreach (var item in Items)
        {
            menuWidth = Math.Max(item.Name.Length, menuWidth);
        }
        rBound = StartPos.x + 3 + (menuWidth - 8) * 4 + 40;

        if (x > StartPos.x * CellWidth && x < rBound * CellWidth && y > StartPos.y * CellHeight && y < (StartPos.y + 40) * CellHeight)
        {
            if (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue)
            {
                scrollIndex = Math.Max(0, scrollIndex - 1);
            }
            else if (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
            {
                scrollIndex = Math.Min(Math.Max(0, Items.Count - 4), scrollIndex + 1);
            }
        }

        sel = null;
        if (x > (StartPos.x + 2) * CellWidth && x < (rBound - 6) * CellWidth && y > (StartPos.y + 9) * CellHeight && y < (StartPos.y + 37) * CellHeight)
        {
            sel = (int)Math.Floor((y / CellHeight - StartPos.y - 9) / 7);
            var item = Items[(int)sel + scrollIndex];
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released && item.Active && item.OnLeftClick is not null)
            {
                _ = Task.Run(item.OnLeftClick);
            }
            else if (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released && item.Active && item.OnRightClick is not null)
            {
                _ = Task.Run(item.OnRightClick);
            }
        }
    }

    public void Draw()
    {
        GameRendering.Current.Draw("LobbySettingsContainer", StartPos.x, StartPos.y, Color.White);
        GameRendering.Current.Draw("LobbySettingsContainer", rBound - 40, StartPos.y, Color.White, flipX: true);

        int centerX = StartPos.x + (rBound - StartPos.x) / 2;
        Rectfill(centerX - Title.Length * 2 - 1, StartPos.y + 2, centerX + Title.Length * 2 - 1, StartPos.y + 7, 13);
        Shared.Printc(Title, centerX, StartPos.y + 2, 7);

        int i = 0;
        foreach (var item in Items)
        {
            if (i >= scrollIndex && i < scrollIndex + 4)
            {
                if (sel is not null && sel == i - scrollIndex)
                {
                    Rectfill(StartPos.x + 2, StartPos.y + 9 + (i - scrollIndex) * 7, rBound - 7, StartPos.y + 9 + (i - scrollIndex) * 7 + 6, 17);
                }
                Print(item.Name, StartPos.x + 4, StartPos.y + 10 + (i - scrollIndex) * 7, item.Active ? 7 : 0);
            }
            i++;
        }
        i = Math.Max(4, Items.Count - 4);
        int scrollBarX = rBound - 6;
        int scrollBarY = StartPos.y + 3;
        Rectfill(scrollBarX, scrollBarY, scrollBarX + 2, scrollBarY + 33, 13);
        Pset(F32.FromInt(scrollBarX) + 2, F32.FromInt(scrollBarY), 1);
        Pset(F32.FromInt(scrollBarX) + 2, F32.FromInt(scrollBarY) + 33, 1);
        double range = 29.0 / Math.Max(4, Items.Count);
        Rectfill(scrollBarX + 1, StartPos.y + 5 + scrollIndex * range, scrollBarX + 1, StartPos.y + 5 + (scrollIndex + 4) * range, 6);
    }
}

public class SeedTypeUI((int x, int y) startPos, bool isSurface, int type, bool unbansOn, bool isBanned = false, bool isAvailable = true)
{
    public (int x, int y) StartPos { get; init; } = startPos;
    public bool IsSurface { get; set; } = isSurface;
    public int Type { get; set; } = type;
    public bool UnbansOn { get; set; } = unbansOn;
    public bool IsBanned { get; set; } = isBanned;
    public bool IsAvailable { get; set; } = isAvailable;
    private bool isHovered = false;

    public void Update(MouseState mouseState, MouseState prevMouseState)
    {
        var cursor = GameRendering.Current.GetCursorPosition(mouseState.X, mouseState.Y);
        float x = cursor.X;
        float y = cursor.Y;

        isHovered = false;
        if (x > StartPos.x * CellWidth && x < (StartPos.x + 22) * CellWidth && y > StartPos.y * CellHeight && y < (StartPos.y + 22) * CellHeight)
        {
            isHovered = true;
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {

            }
            else if (mouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released)
            {

            }
        }
    }

    public void Draw()
    {
        if (Type > 0 && Type <= 5)
        {
            GameRendering.Current.Draw($"{(IsSurface ? "Surface" : "Cave")}{Type}Test", StartPos.x + 2, StartPos.y + 2, Color.White);
            GameRendering.Current.Draw("SeedSelector", StartPos.x, StartPos.y, Colors[isHovered && IsAvailable ? 14 : 2]);
            if (IsBanned)
                GameRendering.Current.Draw("SeedCross", StartPos.x + 2, StartPos.y + 2, Colors[isHovered && IsAvailable ? 14 : 2]);
            if (!IsAvailable || (!UnbansOn && IsBanned))
                GameRendering.Current.Draw("SeedGreyOut", StartPos.x, StartPos.y, Color.White);
        }
    }
}

public class SeedPickButton((int x, int y) startPos, string label, bool isActive = true)
{
    public (int x, int y) StartPos { get; set; } = startPos;
    public string Label { get; set; } = label;
    public bool IsActive { get; set; } = isActive;
    public bool IsHovered { get; internal set; } = false;

    public void Update(float x, float y)
    {
        if (IsActive && x > StartPos.x * CellWidth && x < (StartPos.x + (Label.Length * 4) + 5) * CellWidth && y > StartPos.y * CellHeight && y < (StartPos.y + 9) * CellHeight)
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
        GameRendering.Current.Draw($"SeedPick{(IsHovered ? "Highlight" : "Background")}Center", StartPos.x + 2, StartPos.y, Color.White, scaleX: Label.Length * 4 + 1);
        Print(Label, StartPos.x + 3, StartPos.y + 2, IsHovered ? 15 : 22);
        GameRendering.Current.Draw($"SeedPick{(IsHovered ? "Highlight" : "Background")}Edge", StartPos.x, StartPos.y, Color.White);
        GameRendering.Current.Draw($"SeedPick{(IsHovered ? "Highlight" : "Background")}Edge", StartPos.x + (Label.Length * 4) + 3, StartPos.y, Color.White, flipX: true);
    }
}