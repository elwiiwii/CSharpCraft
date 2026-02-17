namespace CSharpCraft.Pico8;

/// <summary>
/// Default implementation of audio/graphics settings using reflection to access OptionsFile.
/// Allows Pico8 to work with OptionsFile without direct dependency.
/// </summary>
public class ReflectionAudioGraphicsSettings : IAudioGraphicsSettings
{
    private readonly object? _optionsData;

    public ReflectionAudioGraphicsSettings(object? optionsData)
    {
        _optionsData = optionsData;
    }

    public bool SoundEnabled
    {
        get => GetPropertyValue<bool>("Gen_Sound_On") ?? true;
        set => SetPropertyValue("Gen_Sound_On", value);
    }

    public int MusicVolume
    {
        get => GetPropertyValue<int>("Gen_Music_Vol") ?? 100;
        set => SetPropertyValue("Gen_Music_Vol", value);
    }

    public int SfxVolume
    {
        get => GetPropertyValue<int>("Gen_Sfx_Vol") ?? 100;
        set => SetPropertyValue("Gen_Sfx_Vol", value);
    }

    public int CurrentSoundtrack
    {
        get => GetPropertyValue<int>("Pcraft_Soundtrack") ?? 0;
        set => SetPropertyValue("Pcraft_Soundtrack", value);
    }

    public int CurrentSfxPack
    {
        get => GetPropertyValue<int>("Pcraft_Sfx_Pack") ?? 0;
        set => SetPropertyValue("Pcraft_Sfx_Pack", value);
    }

    public bool IsFullscreen
    {
        get => GetPropertyValue<bool>("Gen_Fullscreen") ?? false;
        set => SetPropertyValue("Gen_Fullscreen", value);
    }

    public int WindowWidth
    {
        get => GetPropertyValue<int>("Gen_Window_Width") ?? 512;
        set => SetPropertyValue("Gen_Window_Width", value);
    }

    public int WindowHeight
    {
        get => GetPropertyValue<int>("Gen_Window_Height") ?? 512;
        set => SetPropertyValue("Gen_Window_Height", value);
    }

    private T? GetPropertyValue<T>(string propertyName) where T : struct
    {
        if (_optionsData == null) return null;
        try
        {
            var prop = _optionsData.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                var value = prop.GetValue(_optionsData);
                if (value is T tValue) return tValue;
            }
        }
        catch { }
        return null;
    }

    private void SetPropertyValue<T>(string propertyName, T value) where T : struct
    {
        if (_optionsData == null) return;
        try
        {
            var prop = _optionsData.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                prop.SetValue(_optionsData, value);
                // Persist to file if method exists
                var writeMethod = _optionsData.GetType().GetMethod("JsonWrite");
                if (writeMethod != null)
                {
                    writeMethod.Invoke(null, new[] { _optionsData });
                }
            }
        }
        catch { }
    }
}
