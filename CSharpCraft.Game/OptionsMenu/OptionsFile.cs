using System.Text.Json;
using CSharpCraft.Pico8;
using CSharpCraft.Pcraft;

namespace CSharpCraft.OptionsMenu;

public class OptionsFile : IAudioSettings, IDisplaySettings, IInputBindingProvider
{
    /// <summary>
    /// Global accessor for the current options. Set once in Main.cs at startup.
    /// </summary>
    public static OptionsFile Current { get; set; } = null!;

    // Keyboard bindings (serialized to settings.json)
    public InputBinding Kbm_Left { get; set; } = new InputBinding("Left", "NumPad4");
    public InputBinding Kbm_Right { get; set; } = new InputBinding("Right", "NumPad6");
    public InputBinding Kbm_Up { get; set; } = new InputBinding("Up", "NumPad8");
    public InputBinding Kbm_Down { get; set; } = new InputBinding("Down", "NumPad5");
    public InputBinding Kbm_Use { get; set; } = new InputBinding("X", "V");
    public InputBinding Kbm_Menu { get; set; } = new InputBinding("Z", "C");
    public InputBinding Kbm_Pause { get; set; } = new InputBinding("Escape", "Enter");

    // Controller bindings (serialized to settings.json)
    public InputBinding Con_Left { get; set; } = new InputBinding("DPadLeft", "LeftThumbstickLeft");
    public InputBinding Con_Right { get; set; } = new InputBinding("DPadRight", "LeftThumbstickRight");
    public InputBinding Con_Up { get; set; } = new InputBinding("DPadUp", "LeftThumbstickUp");
    public InputBinding Con_Down { get; set; } = new InputBinding("DPadDown", "LeftThumbstickDown");
    public InputBinding Con_Use { get; set; } = new InputBinding("A", "LeftShoulder");
    public InputBinding Con_Menu { get; set; } = new InputBinding("B", "RightShoulder");
    public InputBinding Con_Pause { get; set; } = new InputBinding("Start", "Back");

    // Audio/graphics settings (serialized to settings.json)
    public bool Gen_Sound_On { get; set; } = true;
    public int Gen_Music_Vol { get; set; } = 100;
    public int Gen_Sfx_Vol { get; set; } = 100;
    public int Pcraft_Soundtrack { get; set; } = 0;
    public int Pcraft_Sfx_Pack { get; set; } = 0;

    public bool Gen_Fullscreen { get; set; } = false;
    public int Gen_Window_Width { get; set; } = 512;
    public int Gen_Window_Height { get; set; } = 512;

    // === IAudioSettings explicit implementation ===
    // Delegates to the serialized properties — no reflection needed.

    bool IAudioSettings.SoundEnabled
    {
        get => Gen_Sound_On;
        set => Gen_Sound_On = value;
    }

    int IAudioSettings.MusicVolume
    {
        get => Gen_Music_Vol;
        set => Gen_Music_Vol = value;
    }

    int IAudioSettings.SfxVolume
    {
        get => Gen_Sfx_Vol;
        set => Gen_Sfx_Vol = value;
    }

    int IAudioSettings.CurrentSoundtrack
    {
        get => Pcraft_Soundtrack;
        set => Pcraft_Soundtrack = value;
    }

    int IAudioSettings.CurrentSfxPack
    {
        get => Pcraft_Sfx_Pack;
        set => Pcraft_Sfx_Pack = value;
    }

    void IAudioSettings.Save() => JsonWrite(this);

    // === IDisplaySettings explicit implementation ===

    bool IDisplaySettings.IsFullscreen
    {
        get => Gen_Fullscreen;
        set => Gen_Fullscreen = value;
    }

    int IDisplaySettings.WindowWidth
    {
        get => Gen_Window_Width;
        set => Gen_Window_Width = value;
    }

    int IDisplaySettings.WindowHeight
    {
        get => Gen_Window_Height;
        set => Gen_Window_Height = value;
    }

    void IDisplaySettings.Save() => JsonWrite(this);

    // === IInputBindingProvider explicit implementation ===
    // Delegates to the serialized properties — engine reads bindings directly.

    InputBinding IInputBindingProvider.KeyboardLeft => Kbm_Left;
    InputBinding IInputBindingProvider.KeyboardRight => Kbm_Right;
    InputBinding IInputBindingProvider.KeyboardUp => Kbm_Up;
    InputBinding IInputBindingProvider.KeyboardDown => Kbm_Down;
    InputBinding IInputBindingProvider.KeyboardUse => Kbm_Use;
    InputBinding IInputBindingProvider.KeyboardMenu => Kbm_Menu;
    InputBinding IInputBindingProvider.KeyboardPause => Kbm_Pause;

    InputBinding IInputBindingProvider.ControllerLeft => Con_Left;
    InputBinding IInputBindingProvider.ControllerRight => Con_Right;
    InputBinding IInputBindingProvider.ControllerUp => Con_Up;
    InputBinding IInputBindingProvider.ControllerDown => Con_Down;
    InputBinding IInputBindingProvider.ControllerUse => Con_Use;
    InputBinding IInputBindingProvider.ControllerMenu => Con_Menu;
    InputBinding IInputBindingProvider.ControllerPause => Con_Pause;

    // === Serialization ===

    private static readonly string optionsFileName = "settings.json";
    private static readonly JsonSerializerOptions jsonOptions = new() { IncludeFields = true, WriteIndented = true };

    public static OptionsFile JsonWrite(OptionsFile file)
    {
        string jsonString = JsonSerializer.Serialize(file, jsonOptions);
        File.WriteAllText(optionsFileName, jsonString);
        return file;
    }

    private static OptionsFile CreateNewOptionsFile()
    {
        OptionsFile optionsFile = new();
        JsonWrite(optionsFile);
        return optionsFile;
    }

    // === Validation (replaces old reflection-based FixFile) ===

    /// <summary>
    /// Validates all settings, resetting invalid values to defaults.
    /// Public for testability — called internally by Initialize().
    /// </summary>
    public static void Validate(OptionsFile file)
    {
        var defaults = new OptionsFile();
        bool changed = false;

        // Validate keyboard bindings
        changed |= ValidateKeyBinding(file, defaults, f => f.Kbm_Left, (f, v) => f.Kbm_Left = v);
        changed |= ValidateKeyBinding(file, defaults, f => f.Kbm_Right, (f, v) => f.Kbm_Right = v);
        changed |= ValidateKeyBinding(file, defaults, f => f.Kbm_Up, (f, v) => f.Kbm_Up = v);
        changed |= ValidateKeyBinding(file, defaults, f => f.Kbm_Down, (f, v) => f.Kbm_Down = v);
        changed |= ValidateKeyBinding(file, defaults, f => f.Kbm_Use, (f, v) => f.Kbm_Use = v);
        changed |= ValidateKeyBinding(file, defaults, f => f.Kbm_Menu, (f, v) => f.Kbm_Menu = v);
        changed |= ValidateKeyBinding(file, defaults, f => f.Kbm_Pause, (f, v) => f.Kbm_Pause = v);

        // Validate controller bindings
        changed |= ValidateButtonBinding(file, defaults, f => f.Con_Left, (f, v) => f.Con_Left = v);
        changed |= ValidateButtonBinding(file, defaults, f => f.Con_Right, (f, v) => f.Con_Right = v);
        changed |= ValidateButtonBinding(file, defaults, f => f.Con_Up, (f, v) => f.Con_Up = v);
        changed |= ValidateButtonBinding(file, defaults, f => f.Con_Down, (f, v) => f.Con_Down = v);
        changed |= ValidateButtonBinding(file, defaults, f => f.Con_Use, (f, v) => f.Con_Use = v);
        changed |= ValidateButtonBinding(file, defaults, f => f.Con_Menu, (f, v) => f.Con_Menu = v);
        changed |= ValidateButtonBinding(file, defaults, f => f.Con_Pause, (f, v) => f.Con_Pause = v);

        // Validate volumes (0-100)
        if (file.Gen_Music_Vol < 0 || file.Gen_Music_Vol > 100)
        {
            file.Gen_Music_Vol = defaults.Gen_Music_Vol;
            changed = true;
        }
        if (file.Gen_Sfx_Vol < 0 || file.Gen_Sfx_Vol > 100)
        {
            file.Gen_Sfx_Vol = defaults.Gen_Sfx_Vol;
            changed = true;
        }

        // Validate soundtrack/sfx pack indices
        if (file.Pcraft_Soundtrack < 0 || file.Pcraft_Soundtrack > new PcraftSingleplayer().Music.Count - 1)
        {
            file.Pcraft_Soundtrack = defaults.Pcraft_Soundtrack;
            changed = true;
        }
        if (file.Pcraft_Sfx_Pack < 0 || file.Pcraft_Sfx_Pack > new PcraftSingleplayer().Sfx.Count - 1)
        {
            file.Pcraft_Sfx_Pack = defaults.Pcraft_Sfx_Pack;
            changed = true;
        }

        // Validate window dimensions (1-16384)
        if (file.Gen_Window_Width < 1 || file.Gen_Window_Width > 16384)
        {
            file.Gen_Window_Width = defaults.Gen_Window_Width;
            changed = true;
        }
        if (file.Gen_Window_Height < 1 || file.Gen_Window_Height > 16384)
        {
            file.Gen_Window_Height = defaults.Gen_Window_Height;
            changed = true;
        }

        if (changed)
        {
            JsonWrite(file);
        }
    }

    private static bool ValidateKeyBinding(
        OptionsFile file, OptionsFile defaults,
        Func<OptionsFile, InputBinding> get, Action<OptionsFile, InputBinding> set)
    {
        var binding = get(file);
        var @default = get(defaults);
        bool changed = false;

        string bind1 = binding.Bind1;
        string bind2 = binding.Bind2;

        if (bind1 is null || !KeyNames.keyNames.ContainsKey(bind1))
        {
            bind1 = @default.Bind1;
            changed = true;
        }
        if (bind2 is null || !KeyNames.keyNames.ContainsKey(bind2))
        {
            bind2 = @default.Bind2;
            changed = true;
        }

        if (changed) { set(file, new InputBinding(bind1, bind2)); }
        return changed;
    }

    private static bool ValidateButtonBinding(
        OptionsFile file, OptionsFile defaults,
        Func<OptionsFile, InputBinding> get, Action<OptionsFile, InputBinding> set)
    {
        var binding = get(file);
        var @default = get(defaults);
        bool changed = false;

        string bind1 = binding.Bind1;
        string bind2 = binding.Bind2;

        if (bind1 is null || !ButtonNames.buttonNames.ContainsKey(bind1))
        {
            bind1 = @default.Bind1;
            changed = true;
        }
        if (bind2 is null || !ButtonNames.buttonNames.ContainsKey(bind2))
        {
            bind2 = @default.Bind2;
            changed = true;
        }

        if (changed) { set(file, new InputBinding(bind1, bind2)); }
        return changed;
    }

    // === Initialization ===

    public static (OptionsFile file, bool error) Initialize()
    {
        if (File.Exists(optionsFileName))
        {
            try
            {
                string jsonString = File.ReadAllText(optionsFileName);
                OptionsFile? result = JsonSerializer.Deserialize<OptionsFile>(jsonString);

                Validate(result!);

                return (result!, false);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Corrupted JSON: {ex.Message}. Recreating file.");
                File.Delete(optionsFileName);
                return (CreateNewOptionsFile(), true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Console.WriteLine($"File error: {ex.Message}. Using defaults.");
                return (new OptionsFile(), true);
            }
        }
        else
        {
            return (CreateNewOptionsFile(), true);
        }
    }
}
