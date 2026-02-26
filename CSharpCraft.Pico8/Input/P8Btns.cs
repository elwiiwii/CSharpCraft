namespace CSharpCraft.Pico8.Input;

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
