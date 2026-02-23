using FixMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// Factory for creating Pico-8 service instances with proper dependency injection.
/// Abstracts away service construction logic and allows for mock implementations in tests.
/// </summary>
public interface IServiceFactory
{
    /// <summary>
    /// Creates an audio channels service for multi-channel sound management.
    /// </summary>
    AudioChannels CreateAudioChannels();

    /// <summary>
    /// Creates a sprite cache service for sprite management.
    /// </summary>
    SpriteCache CreateSpriteCache();

    /// <summary>
    /// Creates a palette manager service for color remapping.
    /// </summary>
    PaletteManager CreatePaletteManager(List<Color> colors);

    /// <summary>
    /// Creates a music manager service.
    /// </summary>
    MusicManager CreateMusicManager(
        Func<Dictionary<string, List<SongInst>>> getMusicDict,
        Func<Dictionary<string, SoundEffect>> getMusicDictionary,
        Func<IAudioGraphicsSettings> getSettings,
        Action soundDispose);

    /// <summary>
    /// Creates a track manager service for music/SFX selection.
    /// </summary>
    ITrackManager CreateTrackManager(
        Func<Dictionary<string, List<SongInst>>> getMusicDict,
        Func<Dictionary<string, Dictionary<int, string>>> getSfxDict,
        IAudioGraphicsSettings settings);

    /// <summary>
    /// Creates a map manager service for tile and flag access.
    /// </summary>
    IMapManager CreateMapManager(
        int[] mapData,
        int[] flagData,
        (int x, int y) mapDimensions);

    /// <summary>
    /// Creates a graphics API service for drawing primitives.
    /// </summary>
    IGraphicsAPI CreateGraphicsAPI(
        SpriteBatch batch,
        Texture2D pixel,
        List<Color> colors,
        F32 cameraOffsetX,
        F32 cameraOffsetY,
        (int Width, int Height) cell);

    /// <summary>
    /// Creates an audio API service for sound/music playback.
    /// </summary>
    IAudioAPI CreateAudioAPI(
        AudioChannels? audioChannels,
        MusicManager? musicManager,
        Dictionary<string, SoundEffect> soundEffectDictionary,
        Func<Dictionary<string, Dictionary<int, string>>> getSfxDict,
        Func<int> getCurrentSfxPack,
        Func<bool> isSoundEnabled,
        Func<int> getSfxVolume);

    /// <summary>
    /// Creates a pause menu state manager for pause menu rendering and input.
    /// </summary>
    PauseMenuState CreatePauseMenuState(IPauseMenuContext context);

    /// <summary>
    /// Creates an input state manager for button handling and input queries.
    /// Phase 8: Input Handling Extraction
    /// </summary>
    IInputStateManager CreateInputStateManager();

    /// <summary>
    /// Creates an output facade for graphics and audio coordination.
    /// Phase 9: Output Facade Pattern
    /// </summary>
    IOutputFacade CreateOutputFacade(IGraphicsAPI? graphicsAPI, IAudioAPI? audioAPI);
}
