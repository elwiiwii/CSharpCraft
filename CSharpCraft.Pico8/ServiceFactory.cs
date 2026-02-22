using FixMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// Default implementation of IServiceFactory.
/// Creates all Pico-8 services with standard dependency resolution.
/// </summary>
public class ServiceFactory : IServiceFactory
{
    public AudioChannels CreateAudioChannels()
    {
        return new AudioChannels();
    }

    public SpriteCache CreateSpriteCache()
    {
        return new SpriteCache();
    }

    public PaletteManager CreatePaletteManager(List<Color> colors)
    {
        return new PaletteManager(colors);
    }

    public MusicManager CreateMusicManager(
        Func<Dictionary<string, List<SongInst>>> getMusicDict,
        Func<Dictionary<string, SoundEffect>> getMusicDictionary,
        Func<IAudioGraphicsSettings> getSettings,
        Action soundDispose)
    {
        return new MusicManager(
            getMusicDict,
            getMusicDictionary,
            getSettings,
            soundDispose);
    }

    public ITrackManager CreateTrackManager(
        Func<Dictionary<string, List<SongInst>>> getMusicDict,
        Func<Dictionary<string, Dictionary<int, string>>> getSfxDict,
        IAudioGraphicsSettings settings)
    {
        return new TrackManager(
            getMusicDict,
            getSfxDict,
            settings);
    }

    public IMapManager CreateMapManager(
        int[] mapData,
        int[] flagData,
        (int x, int y) mapDimensions)
    {
        return new MapManager(
            mapData,
            flagData,
            mapDimensions);
    }

    public IGraphicsAPI CreateGraphicsAPI(
        SpriteBatch batch,
        Texture2D pixel,
        List<Color> colors,
        F32 cameraOffsetX,
        F32 cameraOffsetY,
        (int Width, int Height) cell)
    {
        return new GraphicsAPI(
            batch,
            pixel,
            colors,
            cameraOffsetX,
            cameraOffsetY,
            cell);
    }

    public IAudioAPI CreateAudioAPI(
        AudioChannels? audioChannels,
        MusicManager? musicManager,
        Dictionary<string, SoundEffect> soundEffectDictionary,
        Func<Dictionary<string, Dictionary<int, string>>> getSfxDict,
        Func<int> getCurrentSfxPack,
        Func<bool> isSoundEnabled,
        Func<int> getSfxVolume)
    {
        return new AudioAPI(
            audioChannels,
            musicManager,
            soundEffectDictionary,
            getSfxDict,
            getCurrentSfxPack,
            isSoundEnabled,
            getSfxVolume);
    }

    public PauseMenuState CreatePauseMenuState(GameOrchestrator orchestrator)
    {
        return new PauseMenuState(orchestrator);
    }

    public IInputStateManager CreateInputStateManager()
    {
        return new InputStateManager();
    }

    public IGameState CreateGameState(
        IScene initialScene,
        int[] mapData,
        int[] flagData,
        Microsoft.Xna.Framework.Color[] sprites,
        Dictionary<string, List<SongInst>> music,
        Dictionary<string, Dictionary<int, string>> sfx)
    {
        return new GameStateContainer(initialScene, mapData, flagData, sprites, music, sfx);
    }

    public IOutputFacade CreateOutputFacade(IGraphicsAPI? graphicsAPI, IAudioAPI? audioAPI)
    {
        return new OutputFacade(graphicsAPI, audioAPI);
    }
}
