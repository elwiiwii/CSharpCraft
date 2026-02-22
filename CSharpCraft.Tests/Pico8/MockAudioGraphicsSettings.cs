using CSharpCraft.Pico8;

namespace CSharpCraft.Tests;

/// <summary>
/// Mock implementation of IAudioGraphicsSettings for testing purposes.
/// Allows manual control of all properties without side effects.
/// </summary>
internal class MockAudioGraphicsSettings : IAudioGraphicsSettings
{
    public bool SoundEnabled { get; set; } = true;
    public int MusicVolume { get; set; } = 100;
    public int SfxVolume { get; set; } = 100;
    public int CurrentSoundtrack { get; set; } = 0;
    public int CurrentSfxPack { get; set; } = 0;
    public bool IsFullscreen { get; set; } = false;
    public int WindowWidth { get; set; } = 1024;
    public int WindowHeight { get; set; } = 576;
}
