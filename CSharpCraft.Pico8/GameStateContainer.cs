using FixMath;
using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8;

/// <summary>
/// Consolidated container for all game state data.
/// Provides single source of truth for game data access.
/// Phase 9: State Container Pattern
/// </summary>
public class GameStateContainer(
    IScene initialScene,
    int[] mapData,
    int[] flagData,
    Color[] sprites,
    Dictionary<string, List<SongInst>> music,
    Dictionary<string, Dictionary<int, string>> sfx) : IGameState
{
    private IScene _currentScene = initialScene ?? throw new ArgumentNullException(nameof(initialScene));
    
    // View state
    private (F32 x, F32 y) _cameraOffset = (F32.Zero, F32.Zero);
    private (int w, int h) _resolution = (128, 128);
    private (int Width, int Height) _cell = (1, 1);

    // Game data
    private readonly int[] _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
    private readonly int[] _flagData = flagData ?? throw new ArgumentNullException(nameof(flagData));
    private readonly Color[] _sprites = sprites ?? throw new ArgumentNullException(nameof(sprites));
    private readonly Dictionary<string, List<SongInst>> _music = music ?? throw new ArgumentNullException(nameof(music));
    private readonly Dictionary<string, Dictionary<int, string>> _sfx = sfx ?? throw new ArgumentNullException(nameof(sfx));

    /// <summary>
    /// Gets or sets the camera offset for viewport positioning.
    /// </summary>
    public (F32 x, F32 y) CameraOffset
    {
        get => _cameraOffset;
        set => _cameraOffset = value;
    }

    /// <summary>
    /// Gets the game resolution (128x128 for Pico-8).
    /// </summary>
    public (int w, int h) Resolution => _resolution;

    /// <summary>
    /// Gets or sets the cell dimensions (viewport cell size in pixels).
    /// </summary>
    public (int Width, int Height) Cell
    {
        get => _cell;
        set => _cell = value;
    }

    /// <summary>
    /// Gets the map tile data array.
    /// </summary>
    public int[] MapData => _mapData;

    /// <summary>
    /// Gets the map flag data array.
    /// </summary>
    public int[] FlagData => _flagData;

    /// <summary>
    /// Gets the sprite graphics data.
    /// </summary>
    public Color[] Sprites => _sprites;

    /// <summary>
    /// Gets the music tracks dictionary.
    /// </summary>
    public Dictionary<string, List<SongInst>> Music => _music;

    /// <summary>
    /// Gets the SFX samples dictionary.
    /// </summary>
    public Dictionary<string, Dictionary<int, string>> Sfx => _sfx;

    /// <summary>
    /// Gets the currently active scene.
    /// </summary>
    public IScene CurrentScene => _currentScene;

    /// <summary>
    /// Updates the current scene reference.
    /// Called when transitioning to a new scene.
    /// </summary>
    public void SetCurrentScene(IScene scene)
    {
        _currentScene = scene ?? throw new ArgumentNullException(nameof(scene));
    }
}
