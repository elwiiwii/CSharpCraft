using FixMath;

namespace CSharpCraft.Pico8;

/// <summary>
/// Implementation of output facade for graphics and audio.
/// Delegates to underlying graphics and audio APIs.
/// Phase 9: Output Facade Pattern
/// </summary>
public class OutputFacade(IGraphicsAPI? graphicsAPI, IAudioAPI? audioAPI) : IOutputFacade
{

    // Graphics Operations

    public void SetPixel(double x, double y, int color)
    {
        graphicsAPI?.Pset(F32.FromDouble(x), F32.FromDouble(y), color);
    }

    public void DrawRect(double x, double y, double w, double h, int color)
    {
        graphicsAPI?.Rect(x, y, x + w, y + h, (double)color);
    }

    public void FillRect(double x, double y, double w, double h, int color)
    {
        graphicsAPI?.Rectfill(x, y, x + w, y + h, (double)color);
    }

    public void DrawCircle(double x, double y, double r, int color)
    {
        graphicsAPI?.Circ(F32.FromDouble(x), F32.FromDouble(y), r, color);
    }

    public void FillCircle(double x, double y, double r, int color)
    {
        graphicsAPI?.Circfill(F32.FromDouble(x), F32.FromDouble(y), r, color);
    }

    public void ClearScreen(int color = 0)
    {
        graphicsAPI?.Cls(color);
    }

    // Audio Operations

    public void PlaySound(int sfxIndex, int channel = 0, int offset = 0, int length = 32)
    {
        audioAPI?.Sfx(sfxIndex, channel, offset, length);
    }

    public void PlayMusic(int musicIndex, int fadems = 0)
    {
        audioAPI?.Music(musicIndex, fadems);
    }

    public void StopMusic()
    {
        audioAPI?.Music(-1);
    }

    public void MuteAudio()
    {
        audioAPI?.Mute();
    }
}
