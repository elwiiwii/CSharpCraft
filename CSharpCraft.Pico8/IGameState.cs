using FixMath;
using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8;

/// <summary>
/// Consolidated interface for accessing and managing game state.
/// Provides a single source of truth for all game data.
/// Phase 9: State Container Pattern
/// </summary>
public interface IGameState
{
    // View State
    (F32 x, F32 y) CameraOffset { get; set; }
    (int w, int h) Resolution { get; }
    (int Width, int Height) Cell { get; set; }

    // Game Data
    int[] MapData { get; }
    int[] FlagData { get; }
    Color[] Sprites { get; }
    Dictionary<string, List<SongInst>> Music { get; }
    Dictionary<string, Dictionary<int, string>> Sfx { get; }

    // Scene Reference
    IScene CurrentScene { get; }
    void SetCurrentScene(IScene scene);
}
