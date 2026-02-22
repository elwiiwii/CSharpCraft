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

    public void UpLockout(bool paused)
    {
        for (int i = 0; i < 7; i++)
        {
            if (paused)
            {
                if (Pico8Utils.Ptn(i)) { Lockout[i] = true; } else { Lockout[i] = false; }
            }
            else
            {
                if (!Pico8Utils.Ptn(i)) { Lockout[i] = false; }
            }
        }
    }
}

/// <summary>
/// A menu item with a display name and callback action.
/// Used by the pause menu system.
/// </summary>
public class MenuItem
{
    public Func<string> GetName { get; }
    public Action Function { get; }
    public MenuItem Clone() => new(this.GetName, this.Function);

    public MenuItem(Func<string> getName, Action function)
    {
        GetName = getName;
        Function = function;
    }
}

/// <summary>
/// Represents an active music instance (a playing SoundEffectInstance).
/// </summary>
public class MusicInst
{
    public string Name { get; }
    public SoundEffectInstance Track { get; }
    public bool Loop { get; }
    public int Group { get; }
    public MusicInst(string name, SoundEffectInstance track, bool loop, int group)
    {
        Name = name;
        Track = track;
        Loop = loop;
        Group = group;
    }
}

/// <summary>
/// Represents a song definition: a list of named tracks with loop settings.
/// </summary>
public class SongInst
{
    public List<(string name, bool loop)> Tracks { get; }
    public int Group { get; }
    public SongInst(List<(string name, bool loop)> tracks, int group)
    {
        Tracks = tracks;
        Group = group;
    }
}

/// <summary>
/// Palette color remapping entry (source color, target color, transparency flag).
/// </summary>
public class PalCol
{
    public Color C0 { get; set; }
    public Color C1 { get; set; }
    public bool Trans { get; set; } = false;
    public PalCol(Color c0, Color c1, bool trans)
    {
        C0 = c0;
        C1 = c1;
        Trans = trans;
    }
}
