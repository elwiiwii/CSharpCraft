namespace CSharpCraft.Pico8;

/// <summary>
/// Manages track selection for music and sound effects.
/// Encapsulates increment/decrement logic and current track accessors.
/// </summary>
public class TrackManager(
    Func<Dictionary<string, List<SongInst>>?> getMusicDict,
    Func<Dictionary<string, Dictionary<int, string>>?> getSfxDict,
    IAudioSettings settings) : ITrackManager
{
    public int MusicCount => getMusicDict()?.Count ?? 0;
    public int SfxCount => getSfxDict()?.Count ?? 0;

    public string GetCurrentSoundtrackName()
    {
        var musicDict = getMusicDict();
        if (musicDict == null || settings.CurrentSoundtrack < 0 || settings.CurrentSoundtrack >= musicDict.Count)
            return "music";
        return musicDict.ElementAt(settings.CurrentSoundtrack).Key;
    }

    public string GetCurrentSfxPackName()
    {
        var sfxDict = getSfxDict();
        if (sfxDict == null || settings.CurrentSfxPack < 0 || settings.CurrentSfxPack >= sfxDict.Count)
            return "sfx";
        return sfxDict.ElementAt(settings.CurrentSfxPack).Key;
    }

    public void DecrementSoundtrack()
    {
        int count = MusicCount;
        if (count <= 1) return;
        int newTrack = settings.CurrentSoundtrack - 1;
        settings.CurrentSoundtrack = Pico8MathUtils.Loop(newTrack, count);
    }

    public void IncrementSoundtrack()
    {
        int count = MusicCount;
        if (count <= 1) return;
        int newTrack = settings.CurrentSoundtrack + 1;
        settings.CurrentSoundtrack = Pico8MathUtils.Loop(newTrack, count);
    }

    public void DecrementSfxPack()
    {
        int count = SfxCount;
        if (count <= 1) return;
        int newPack = settings.CurrentSfxPack - 1;
        settings.CurrentSfxPack = Pico8MathUtils.Loop(newPack, count);
    }

    public void IncrementSfxPack()
    {
        int count = SfxCount;
        if (count <= 1) return;
        int newPack = settings.CurrentSfxPack + 1;
        settings.CurrentSfxPack = Pico8MathUtils.Loop(newPack, count);
    }
}
