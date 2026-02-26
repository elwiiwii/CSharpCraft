namespace CSharpCraft.Pico8.Audio;

/// <summary>
/// Manages track selection for music and sound effects.
/// Encapsulates increment/decrement logic and current track accessors.
/// </summary>
public class TrackManager : ITrackManager
{
    private readonly Func<Dictionary<string, List<SongInst>>?> _getMusicDict;
    private readonly Func<Dictionary<string, Dictionary<int, string>>?> _getSfxDict;
    private readonly IAudioSettings _settings;

    public TrackManager(
        Func<Dictionary<string, List<SongInst>>?> getMusicDict,
        Func<Dictionary<string, Dictionary<int, string>>?> getSfxDict,
        IAudioSettings settings)
    {
        _getMusicDict = getMusicDict ?? throw new ArgumentNullException(nameof(getMusicDict));
        _getSfxDict = getSfxDict ?? throw new ArgumentNullException(nameof(getSfxDict));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public int MusicCount => _getMusicDict()?.Count ?? 0;
    public int SfxCount => _getSfxDict()?.Count ?? 0;

    public string GetCurrentSoundtrackName()
    {
        var musicDict = _getMusicDict();
        if (musicDict == null || _settings.CurrentSoundtrack < 0 || _settings.CurrentSoundtrack >= musicDict.Count)
            return "music";
        return musicDict.ElementAt(_settings.CurrentSoundtrack).Key;
    }

    public string GetCurrentSfxPackName()
    {
        var sfxDict = _getSfxDict();
        if (sfxDict == null || _settings.CurrentSfxPack < 0 || _settings.CurrentSfxPack >= sfxDict.Count)
            return "sfx";
        return sfxDict.ElementAt(_settings.CurrentSfxPack).Key;
    }

    public void DecrementSoundtrack()
    {
        int count = MusicCount;
        if (count <= 1) return;
        int newTrack = _settings.CurrentSoundtrack - 1;
        _settings.CurrentSoundtrack = Pico8MathUtils.Loop(newTrack, count);
    }

    public void IncrementSoundtrack()
    {
        int count = MusicCount;
        if (count <= 1) return;
        int newTrack = _settings.CurrentSoundtrack + 1;
        _settings.CurrentSoundtrack = Pico8MathUtils.Loop(newTrack, count);
    }

    public void DecrementSfxPack()
    {
        int count = SfxCount;
        if (count <= 1) return;
        int newPack = _settings.CurrentSfxPack - 1;
        _settings.CurrentSfxPack = Pico8MathUtils.Loop(newPack, count);
    }

    public void IncrementSfxPack()
    {
        int count = SfxCount;
        if (count <= 1) return;
        int newPack = _settings.CurrentSfxPack + 1;
        _settings.CurrentSfxPack = Pico8MathUtils.Loop(newPack, count);
    }
}
