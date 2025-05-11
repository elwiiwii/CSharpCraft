using CSharpCraft.Pico8;

namespace CSharpCraft.Competitive;

public class Icon
{
    public (int x, int y) StartPos { get; init; }
    public (int x, int y) EndPos { get; init; }
    public (float x, float y) Offset { get; init; } = (-0.6f, -0.6f);
    public string Label { get; init; } = "";
    public string? ShadowTexture { get; init; }
    public string? IconTexture { get; init; }
    public IScene? Scene { get; init; }
}
