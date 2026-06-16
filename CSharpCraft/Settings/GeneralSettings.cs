using PSharp8.Input;

namespace CSharpCraft.Settings;

public class GeneralSettings
{
    public int MusicVolume { get; set; } = 100;
    public int SfxVolume { get; set; } = 100;
    public bool Fullscreen { get; set; } = false;
    public int WindowWidth { get; set; } = 512;
    public int WindowHeight { get; set; } = 512;
    public InputBindings InputBindings { get; set; } = InputBindings.Default;
    public BtnpConfig BtnpConfig { get; set; } = new();
}
