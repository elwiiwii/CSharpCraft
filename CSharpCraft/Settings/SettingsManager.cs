using Microsoft.Xna.Framework;
using PSharp8.Settings;

namespace CSharpCraft.Settings;

public class SettingsManager : IDisposable
{
    private readonly GameOrchestrator _orchestrator;
    private readonly GraphicsDeviceManager _gdm;
    private readonly HotReloadableSettings<GeneralSettings> _general;

    public SettingsManager(
        string configDirectory,
        GameOrchestrator orchestrator,
        GraphicsDeviceManager graphicsDeviceManager)
    {
        ArgumentNullException.ThrowIfNull(configDirectory);
        ArgumentNullException.ThrowIfNull(orchestrator);
        ArgumentNullException.ThrowIfNull(graphicsDeviceManager);

        _orchestrator = orchestrator;
        _gdm = graphicsDeviceManager;

        _ = Directory.CreateDirectory(configDirectory);
        _general = new HotReloadableSettings<GeneralSettings>(
            Path.Combine(configDirectory, "general.json"));

        ApplyAll(_general.Current);
    }

    public GeneralSettings GeneralSettings => _general.Current;

    /// <summary>
    /// Flush any pending hot-reload changes and apply them.
    /// Call once per game tick before input processing.
    /// </summary>
    public void Update()
    {
        GeneralSettings previousGeneral = _general.Current;
        _general.FlushPending();

        if (!ReferenceEquals(_general.Current, previousGeneral))
            ApplyAll(_general.Current);
    }

    public void Dispose()
    {
        _general.Dispose();
    }

    private void ApplyAll(GeneralSettings settings)
    {
        _orchestrator.ApplyInputSettings(settings.InputBindings, settings.BtnpConfig);
        _orchestrator.ApplyAudioSettings(settings.MusicVolume, settings.SfxVolume);
        ApplyWindowSettings(settings);
    }

    private void ApplyWindowSettings(GeneralSettings settings)
    {
        _gdm.PreferredBackBufferWidth = settings.WindowWidth;
        _gdm.PreferredBackBufferHeight = settings.WindowHeight;
        _gdm.IsFullScreen = settings.Fullscreen;
        _gdm.ApplyChanges();
    }
}
