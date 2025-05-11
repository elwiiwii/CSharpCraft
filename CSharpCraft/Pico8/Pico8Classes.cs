using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace CSharpCraft.Pico8;

public class P8Btns
{
    public bool[] Prev { get; internal set; } = new bool[7];
    public bool[] Lockout { get; internal set; } = new bool[7];
    public int[] HeldCount { get; internal set; } = new int[6];
    public void Reset(Pico8Functions p8)
    {
        for (int i = 0; i < 6; i++)
        {
            Lockout[i] = true;
            HeldCount[i] = 0;
        }
        Lockout[6] = true;
    }
    public void Update(Pico8Functions p8)
    {
        for (int i = 0; i < 6; i++)
        {
            Prev[i] = p8.Btn(i);
            if (Prev[i]) { HeldCount[i]++; }
            else { HeldCount[i] = 0; }
        }
    }
    public void UpPause(Pico8Functions p8)
    {
        Prev[6] = p8.Btn(6);
    }
    public void UpLockout(Pico8Functions p8, bool paused)
    {
        for (int i = 0; i < 7; i++)
        {
            if (paused)
            {
                if (Pico8Utils.Ptn(p8, i)) { Lockout[i] = true; } else { Lockout[i] = false; }
            }
            else
            {
                if (!Pico8Utils.Ptn(p8, i)) { Lockout[i] = false; }
            }
        }
    }
}

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