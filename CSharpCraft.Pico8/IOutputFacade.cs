using FixMath;

namespace CSharpCraft.Pico8;

/// <summary>
/// Facade for graphics and audio output operations.
/// Provides simplified interface for rendering and sound coordination.
/// Phase 9: Output Facade Pattern
/// </summary>
public interface IOutputFacade
{
    // Graphics Operations
    
    /// <summary>
    /// Sets a pixel at the specified coordinates.
    /// </summary>
    void SetPixel(double x, double y, int color);

    /// <summary>
    /// Draws an outlined rectangle.
    /// </summary>
    void DrawRect(double x, double y, double w, double h, int color);

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    void FillRect(double x, double y, double w, double h, int color);

    /// <summary>
    /// Draws an outlined circle.
    /// </summary>
    void DrawCircle(double x, double y, double r, int color);

    /// <summary>
    /// Draws a filled circle.
    /// </summary>
    void FillCircle(double x, double y, double r, int color);

    /// <summary>
    /// Clears the screen with the specified color.
    /// </summary>
    void ClearScreen(int color = 0);

    // Audio Operations

    /// <summary>
    /// Plays a sound effect on the specified channel.
    /// </summary>
    void PlaySound(int sfxIndex, int channel = 0, int offset = 0, int length = 32);

    /// <summary>
    /// Plays music with optional fade-in.
    /// </summary>
    void PlayMusic(int musicIndex, int fadems = 0);

    /// <summary>
    /// Stops the currently playing music.
    /// </summary>
    void StopMusic();

    /// <summary>
    /// Mutes all audio output.
    /// </summary>
    void MuteAudio();
}
