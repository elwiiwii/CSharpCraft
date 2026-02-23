using Microsoft.Xna.Framework.Audio;

namespace CSharpCraft.Pico8;

/// <summary>
/// Manages music state machine, track progression, and fade transitions.
/// Extracts 5 scattered state fields and 40+ lines from Update() into a cohesive system.
/// </summary>
public class MusicManager(
    Func<Dictionary<string, List<SongInst>>> getMusicDict,
    Func<Dictionary<string, SoundEffect>> getMusicSoundDict,
    Func<IAudioGraphicsSettings> getSettings,
    Action soundDispose)
{
    private List<List<MusicInst>> _channelMusic = [];
    private int _curTrack = 0;
    private (List<SoundEffectInstance> fromSong, List<SoundEffectInstance> toSong) _musicTransition = new();
    private int? _lastMusicCall;

    public List<List<MusicInst>> ChannelMusic => _channelMusic;
    public int CurrentTrack => _curTrack;
    public int? LastMusicCall => _lastMusicCall;
    public (List<SoundEffectInstance> fromSong, List<SoundEffectInstance> toSong) MusicTransition => _musicTransition;

    /// <summary>
    /// Play music track n with optional fade-in.
    /// Maintains Pico-8 Music() semantics and handles track transitions.
    /// </summary>
    public void PlayMusic(int n, double fadems = 0)
    {
        _lastMusicCall = n;

        var musicDict = getMusicDict();
        var musicSoundDict = getMusicSoundDict();
        var settings = getSettings();

        SongInst curSong = musicDict.ElementAt(settings.CurrentSoundtrack).Value[n];

        if (_channelMusic.Count > 0 && _channelMusic[0][0].Group == curSong.Group)
        {
            // Transition between songs in same group
            foreach (List<MusicInst> item in _channelMusic)
            {
                if (item[0].Name != curSong.Tracks[0].name)
                {
                    foreach (MusicInst sfxInst in item)
                    {
                        _musicTransition.fromSong = [];
                        _musicTransition.fromSong.Add(sfxInst.Track);
                    }
                }
                else
                {
                    foreach (MusicInst sfxInst in item)
                    {
                        _musicTransition.toSong = [];
                        _musicTransition.toSong.Add(sfxInst.Track);
                    }
                }
            }
        }
        else
        {
            // Switch to different group - full reset
            soundDispose();

            foreach (SongInst song in musicDict.ElementAt(settings.CurrentSoundtrack).Value)
            {
                if (song.Group == curSong.Group)
                {
                    List<MusicInst> listOfTracks = [];
                    foreach ((string name, bool loop) in song.Tracks)
                    {
                        listOfTracks.Add(new(name, musicSoundDict[name].CreateInstance(), loop, song.Group));
                    }
                    _channelMusic.Add(listOfTracks);
                }
            }

            foreach (List<MusicInst> item in _channelMusic)
            {
                item[0].Track.IsLooped = item[0].Loop;
                item[0].Track.Play();
                item[0].Track.Volume = item[0].Name == curSong.Tracks[0].name ? settings.SoundEnabled ? settings.MusicVolume / 100.0f : 0 : 0;
                _curTrack = 0;
            }
        }
    }

    /// <summary>
    /// Update music state: track progression, volume fading for transitions.
    /// Called from Pico8Functions.Update() to manage ongoing music playback.
    /// </summary>
    public void Update()
    {
        var settings = getSettings();
        float fadeStep = settings.MusicVolume / 1600.0f;

        foreach (List<MusicInst> song in _channelMusic)
        {
            if (song[_curTrack].Track.State == SoundState.Stopped)
            {
                if (song.Count > _curTrack + 1)
                {
                    _curTrack += 1;
                    song[_curTrack].Track.IsLooped = song[_curTrack].Loop;
                    song[_curTrack].Track.Play();
                    if (settings.SoundEnabled) { song[_curTrack].Track.Volume = settings.MusicVolume / 100.0f; }
                }
            }

            if (_musicTransition.fromSong is null || _musicTransition.toSong is null)
            {
                _musicTransition = new();
                var songName = getMusicDict().ElementAt(settings.CurrentSoundtrack).Value[_lastMusicCall ?? 0].Tracks[_curTrack].name;
                if (settings.SoundEnabled && _lastMusicCall is not null && song[_curTrack].Name == songName)
                {
                    song[_curTrack].Track.Volume = settings.MusicVolume / 100.0f;
                }
            }
        }

        // Handle fade transition between songs
        if (settings.SoundEnabled && _musicTransition.fromSong is not null && _musicTransition.toSong is not null 
            && _musicTransition.fromSong[_curTrack].State == SoundState.Playing 
            && _musicTransition.toSong[_curTrack].State == SoundState.Playing)
        {
            if (_musicTransition.fromSong[_curTrack].Volume > 0.0f)
            {
                _musicTransition.fromSong[_curTrack].Volume -= fadeStep;
            }
            if (_musicTransition.toSong[_curTrack].Volume < settings.MusicVolume / 100.0f)
            {
                _musicTransition.toSong[_curTrack].Volume += fadeStep;
            }
            if (_musicTransition.fromSong[_curTrack].Volume <= 0.0f && _musicTransition.toSong[_curTrack].Volume >= settings.MusicVolume / 100.0f)
            {
                _musicTransition.fromSong[_curTrack].Volume = 0.0f;
                _musicTransition.toSong[_curTrack].Volume = settings.MusicVolume / 100.0f;
                _musicTransition = new();
            }
        }
    }

    /// <summary>
    /// Pause all music tracks.
    /// </summary>
    public void Pause()
    {
        foreach (List<MusicInst> song in _channelMusic)
        {
            if (song[_curTrack].Track.State == SoundState.Playing)
                song[_curTrack].Track.Pause();
        }
    }

    /// <summary>
    /// Resume all music tracks.
    /// </summary>
    public void Resume()
    {
        foreach (List<MusicInst> song in _channelMusic)
        {
            if (song[_curTrack].Track.State == SoundState.Paused)
                song[_curTrack].Track.Play();
        }
    }

    /// <summary>
    /// Mute all music tracks by setting volume to 0.
    /// </summary>
    public void Mute(float volume = 0.0f)
    {
        foreach (List<MusicInst> song in _channelMusic)
        {
            foreach (MusicInst track in song)
            {
                track.Track.Volume = volume;
            }
        }
    }

    /// <summary>
    /// Stop and dispose all music tracks.
    /// </summary>
    public void StopAll()
    {
        foreach (List<MusicInst> song in _channelMusic)
        {
            foreach (MusicInst track in song)
            {
                track.Track.Stop();
                track.Track.Dispose();
            }
        }
        _channelMusic.Clear();
        _curTrack = 0;
        _musicTransition = new();
    }
}
