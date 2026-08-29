using PSharp8.Audio;
using PSharp8.Scene;

namespace CSharpCraft;

internal sealed class EmptyScene : IScene
{
    public string? Name => null;
    public void Init(ISceneSetup setup) { }
    public void Update() { }
    public void Draw() { }
    public string? SpritesPath => null;
    public string? MapPath => null;
    public string? FlagData => null;
    public IReadOnlyList<Soundtrack> Music => [];
    public IReadOnlyList<SfxPack> Sfx => [];
}
