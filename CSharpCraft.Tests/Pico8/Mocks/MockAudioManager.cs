using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8.Mocks;

/// <summary>
/// Mock audio manager that tracks playback for testing
/// </summary>
public class MockAudioManager : IAudioManager
{
    public List<(string operation, int id)> PlaybackCalls { get; } = new();

    public void PlayMusic(int trackId, double fade = 0)
    {
        PlaybackCalls.Add(("PlayMusic", trackId));
    }

    public void PlaySfx(int sfxId, int channel = -1, int offset = 0)
    {
        PlaybackCalls.Add(("PlaySfx", sfxId));
    }

    public void Mute()
    {
        PlaybackCalls.Add(("Mute", -1));
    }

    public int? GetCurrentTrack() => null;
    public void SetMusicLibrary(Dictionary<string, List<SongInst>> music) { }
    public void SetSfxLibrary(Dictionary<string, Dictionary<int, string>> sfx) { }
}
