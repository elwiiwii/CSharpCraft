using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace CSharpCraft.Pico8;

/// <summary>
/// Button state tracker for PICO-8 input system.
/// Tracks previous frame state, lockout, and hold duration for each button.
/// Uses the Pico8 static API for button state queries.
/// </summary>
public class P8Btns
{
    public bool[] Prev { get; internal set; } = new bool[7];
    public bool[] Lockout { get; internal set; } = new bool[7];
    public int[] HeldCount { get; internal set; } = new int[6];

    public void Reset()
    {
        for (int i = 0; i < 6; i++)
        {
            Lockout[i] = true;
            HeldCount[i] = 0;
        }
        Lockout[6] = true;
    }

    public void Update()
    {
        for (int i = 0; i < 6; i++)
        {
            Prev[i] = Pico8.Btn(i);
            if (Prev[i]) { HeldCount[i]++; }
            else { HeldCount[i] = 0; }
        }
    }

    public void UpPause()
    {
        Prev[6] = Pico8.Btn(6);
    }

    public void UpLockout(bool paused, IInputBindingProvider bindings)
    {
        for (int i = 0; i < 7; i++)
        {
            if (paused)
            {
                if (Pico8Utils.Ptn(i, bindings)) { Lockout[i] = true; } else { Lockout[i] = false; }
            }
            else
            {
                if (!Pico8Utils.Ptn(i, bindings)) { Lockout[i] = false; }
            }
        }
    }
}

/// <summary>
/// Button state passed to menu item callbacks.
/// Replaces static Btnp() calls, breaking the circular PauseMenuBuilder → Pico8 dependency.
/// </summary>
public record MenuInput(bool Left, bool Right, bool ActionA, bool ActionB);

/// <summary>
/// A menu item with a display name and callback action.
/// Used by the pause menu system.
/// Callbacks receive MenuInput so they don't need to query input state globally.
/// </summary>
public class MenuItem
{
    public Func<string> GetName { get; }
    public Action<MenuInput> Function { get; }
    public MenuItem Clone() => new(this.GetName, this.Function);

    public MenuItem(Func<string> getName, Action<MenuInput> function)
    {
        GetName = getName;
        Function = function;
    }
}

/// <summary>
/// Represents an active music instance (a playing SoundEffectInstance).
/// </summary>
public record MusicInst(string Name, SoundEffectInstance Track, bool Loop, int Group);

/// <summary>
/// Represents a song definition: a list of named tracks with loop settings.
/// </summary>
public record SongInst(List<(string name, bool loop)> Tracks, int Group);

/// <summary>
/// Palette color remapping entry (source color, target color, transparency flag).
/// </summary>
public record PalCol(Color C0, Color C1, bool Trans)
{
    public Color C0 { get; set; } = C0;
    public Color C1 { get; set; } = C1;
    public bool Trans { get; set; } = Trans;
}
