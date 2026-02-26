using System;

namespace CSharpCraft.Pico8
{
    /// <summary>
    /// AudioOrchestrator - Audio coordination and state management
    /// 
    /// Responsibility: Coordinate audio operations and track audio state
    /// - Delegates sound playback to IAudioAPI
    /// - Tracks last music call for state queries
    /// - Manages audio-specific coordination (music, sfx)
    /// 
    /// SRP 5/5: This class has ONE reason to change: audio coordination logic
    /// 
    /// Note: GameOrchestrator composes this for audio operations.
    /// Pico8 static class delegates through GameOrchestrator.Audio to here.
    /// </summary>
    /// <remarks>
    /// Initialize AudioOrchestrator with required IAudioAPI
    /// </remarks>
    public class AudioOrchestrator : IDisposable
    {
        private readonly IAudioAPI _audioAPI;

        public AudioOrchestrator(IAudioAPI audioAPI)
        {
            _audioAPI = audioAPI ?? throw new ArgumentNullException(nameof(audioAPI));
        }

        /// <summary>
        /// Exposes the underlying IAudioAPI for testing and advanced access
        /// </summary>
        public IAudioAPI API => _audioAPI;

        /// <summary>
        /// Last music track ID that was played, or null if no music has been played
        /// </summary>
        public int? LastMusicCall { get; private set; } = null;

        #region AUDIO OPERATIONS

        /// <summary>
        /// Play a sound effect (delegates to IAudioAPI)
        /// </summary>
        public void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0)
        {
            _audioAPI.Sfx(n, channel, offset, length);
        }

        /// <summary>
        /// Play background music (delegates to IAudioAPI, tracks state)
        /// </summary>
        public void Music(int n, double fadems = 0)
        {
            LastMusicCall = n;
            _audioAPI.Music(n, fadems);
        }

        /// <summary>
        /// Mute all audio (delegates to IAudioAPI)
        /// </summary>
        public void Mute()
        {
            _audioAPI.Mute();
        }

        #endregion

        #region LIFECYCLE MANAGEMENT

        /// <summary>
        /// Resume or pause all audio based on play state.
        /// Called each frame by GameOrchestrator based on pause state.
        /// </summary>
        public void PlaySound(bool play)
        {
            if (play) { _audioAPI.Resume(); }
            else { _audioAPI.Pause(); }
        }

        /// <summary>
        /// Stop all audio playback (music and sound effects).
        /// </summary>
        public void SoundDispose()
        {
            _audioAPI.StopAll();
        }

        /// <summary>
        /// Advance audio state each frame (music transitions, fades).
        /// </summary>
        public void Update()
        {
            _audioAPI.Update();
        }

        /// <summary>
        /// Dispose all audio resources and stop playback.
        /// </summary>
        public void Dispose()
        {
            _audioAPI.StopAll();
        }

        #endregion
    }
}
