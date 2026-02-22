using CSharpCraft.Pico8;
using CSharpCraft.OptionsMenu;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.OptionsMenu;

/// <summary>
/// Tests that OptionsFile correctly implements IAudioGraphicsSettings and IInputBindingProvider.
/// Verifies the property mappings between game-layer names and engine-layer interfaces.
/// </summary>
public class OptionsFileInterfaceTests
{
    // === IAudioGraphicsSettings mapping tests ===

    [Fact]
    public void SoundEnabled_MapsTo_Gen_Sound_On()
    {
        var file = new OptionsFile { Gen_Sound_On = false };
        IAudioGraphicsSettings settings = file;

        settings.SoundEnabled.Should().BeFalse();

        settings.SoundEnabled = true;
        file.Gen_Sound_On.Should().BeTrue();
    }

    [Fact]
    public void MusicVolume_MapsTo_Gen_Music_Vol()
    {
        var file = new OptionsFile { Gen_Music_Vol = 75 };
        IAudioGraphicsSettings settings = file;

        settings.MusicVolume.Should().Be(75);

        settings.MusicVolume = 50;
        file.Gen_Music_Vol.Should().Be(50);
    }

    [Fact]
    public void SfxVolume_MapsTo_Gen_Sfx_Vol()
    {
        var file = new OptionsFile { Gen_Sfx_Vol = 80 };
        IAudioGraphicsSettings settings = file;

        settings.SfxVolume.Should().Be(80);

        settings.SfxVolume = 30;
        file.Gen_Sfx_Vol.Should().Be(30);
    }

    [Fact]
    public void CurrentSoundtrack_MapsTo_Pcraft_Soundtrack()
    {
        var file = new OptionsFile { Pcraft_Soundtrack = 2 };
        IAudioGraphicsSettings settings = file;

        settings.CurrentSoundtrack.Should().Be(2);

        settings.CurrentSoundtrack = 1;
        file.Pcraft_Soundtrack.Should().Be(1);
    }

    [Fact]
    public void CurrentSfxPack_MapsTo_Pcraft_Sfx_Pack()
    {
        var file = new OptionsFile { Pcraft_Sfx_Pack = 3 };
        IAudioGraphicsSettings settings = file;

        settings.CurrentSfxPack.Should().Be(3);

        settings.CurrentSfxPack = 0;
        file.Pcraft_Sfx_Pack.Should().Be(0);
    }

    [Fact]
    public void IsFullscreen_MapsTo_Gen_Fullscreen()
    {
        var file = new OptionsFile { Gen_Fullscreen = true };
        IAudioGraphicsSettings settings = file;

        settings.IsFullscreen.Should().BeTrue();

        settings.IsFullscreen = false;
        file.Gen_Fullscreen.Should().BeFalse();
    }

    [Fact]
    public void WindowWidth_MapsTo_Gen_Window_Width()
    {
        var file = new OptionsFile { Gen_Window_Width = 1024 };
        IAudioGraphicsSettings settings = file;

        settings.WindowWidth.Should().Be(1024);

        settings.WindowWidth = 800;
        file.Gen_Window_Width.Should().Be(800);
    }

    [Fact]
    public void WindowHeight_MapsTo_Gen_Window_Height()
    {
        var file = new OptionsFile { Gen_Window_Height = 768 };
        IAudioGraphicsSettings settings = file;

        settings.WindowHeight.Should().Be(768);

        settings.WindowHeight = 600;
        file.Gen_Window_Height.Should().Be(600);
    }

    // === IInputBindingProvider mapping tests ===

    [Fact]
    public void KeyboardLeft_MapsTo_Kbm_Left()
    {
        var file = new OptionsFile { Kbm_Left = new InputBinding("A", "Q") };
        IInputBindingProvider bindings = file;

        bindings.KeyboardLeft.Bind1.Should().Be("A");
        bindings.KeyboardLeft.Bind2.Should().Be("Q");
    }

    [Fact]
    public void KeyboardRight_MapsTo_Kbm_Right()
    {
        var file = new OptionsFile { Kbm_Right = new InputBinding("D", "E") };
        IInputBindingProvider bindings = file;

        bindings.KeyboardRight.Bind1.Should().Be("D");
        bindings.KeyboardRight.Bind2.Should().Be("E");
    }

    [Fact]
    public void KeyboardUp_MapsTo_Kbm_Up()
    {
        var file = new OptionsFile { Kbm_Up = new InputBinding("W", "Z") };
        IInputBindingProvider bindings = file;

        bindings.KeyboardUp.Bind1.Should().Be("W");
        bindings.KeyboardUp.Bind2.Should().Be("Z");
    }

    [Fact]
    public void KeyboardDown_MapsTo_Kbm_Down()
    {
        var file = new OptionsFile { Kbm_Down = new InputBinding("S", "X") };
        IInputBindingProvider bindings = file;

        bindings.KeyboardDown.Bind1.Should().Be("S");
        bindings.KeyboardDown.Bind2.Should().Be("X");
    }

    [Fact]
    public void KeyboardUse_MapsTo_Kbm_Use()
    {
        var file = new OptionsFile { Kbm_Use = new InputBinding("Space", "E") };
        IInputBindingProvider bindings = file;

        bindings.KeyboardUse.Bind1.Should().Be("Space");
        bindings.KeyboardUse.Bind2.Should().Be("E");
    }

    [Fact]
    public void KeyboardMenu_MapsTo_Kbm_Menu()
    {
        var file = new OptionsFile { Kbm_Menu = new InputBinding("Tab", "M") };
        IInputBindingProvider bindings = file;

        bindings.KeyboardMenu.Bind1.Should().Be("Tab");
        bindings.KeyboardMenu.Bind2.Should().Be("M");
    }

    [Fact]
    public void KeyboardPause_MapsTo_Kbm_Pause()
    {
        var file = new OptionsFile { Kbm_Pause = new InputBinding("P", "Escape") };
        IInputBindingProvider bindings = file;

        bindings.KeyboardPause.Bind1.Should().Be("P");
        bindings.KeyboardPause.Bind2.Should().Be("Escape");
    }

    [Fact]
    public void ControllerLeft_MapsTo_Con_Left()
    {
        var file = new OptionsFile { Con_Left = new InputBinding("DPadLeft", "LeftStick") };
        IInputBindingProvider bindings = file;

        bindings.ControllerLeft.Bind1.Should().Be("DPadLeft");
        bindings.ControllerLeft.Bind2.Should().Be("LeftStick");
    }

    [Fact]
    public void ControllerRight_MapsTo_Con_Right()
    {
        var file = new OptionsFile { Con_Right = new InputBinding("DPadRight", "RightStick") };
        IInputBindingProvider bindings = file;

        bindings.ControllerRight.Bind1.Should().Be("DPadRight");
        bindings.ControllerRight.Bind2.Should().Be("RightStick");
    }

    [Fact]
    public void ControllerUp_MapsTo_Con_Up()
    {
        var file = new OptionsFile { Con_Up = new InputBinding("DPadUp", "LeftThumbUp") };
        IInputBindingProvider bindings = file;

        bindings.ControllerUp.Bind1.Should().Be("DPadUp");
        bindings.ControllerUp.Bind2.Should().Be("LeftThumbUp");
    }

    [Fact]
    public void ControllerDown_MapsTo_Con_Down()
    {
        var file = new OptionsFile { Con_Down = new InputBinding("DPadDown", "LeftThumbDown") };
        IInputBindingProvider bindings = file;

        bindings.ControllerDown.Bind1.Should().Be("DPadDown");
        bindings.ControllerDown.Bind2.Should().Be("LeftThumbDown");
    }

    [Fact]
    public void ControllerUse_MapsTo_Con_Use()
    {
        var file = new OptionsFile { Con_Use = new InputBinding("A", "X") };
        IInputBindingProvider bindings = file;

        bindings.ControllerUse.Bind1.Should().Be("A");
        bindings.ControllerUse.Bind2.Should().Be("X");
    }

    [Fact]
    public void ControllerMenu_MapsTo_Con_Menu()
    {
        var file = new OptionsFile { Con_Menu = new InputBinding("B", "Y") };
        IInputBindingProvider bindings = file;

        bindings.ControllerMenu.Bind1.Should().Be("B");
        bindings.ControllerMenu.Bind2.Should().Be("Y");
    }

    [Fact]
    public void ControllerPause_MapsTo_Con_Pause()
    {
        var file = new OptionsFile { Con_Pause = new InputBinding("Start", "Select") };
        IInputBindingProvider bindings = file;

        bindings.ControllerPause.Bind1.Should().Be("Start");
        bindings.ControllerPause.Bind2.Should().Be("Select");
    }

    // === Cross-concern: same instance serves both interfaces ===

    [Fact]
    public void SingleInstance_ServesBothInterfaces()
    {
        var file = new OptionsFile();

        // Can be used as both interfaces simultaneously
        IAudioGraphicsSettings settings = file;
        IInputBindingProvider bindings = file;

        settings.SoundEnabled.Should().BeTrue(); // default
        bindings.KeyboardLeft.Bind1.Should().Be("Left"); // default
    }

    // === Default values match ===

    [Fact]
    public void DefaultValues_MatchOriginalDefaults()
    {
        var file = new OptionsFile();
        IAudioGraphicsSettings settings = file;

        settings.SoundEnabled.Should().BeTrue();
        settings.MusicVolume.Should().Be(100);
        settings.SfxVolume.Should().Be(100);
        settings.CurrentSoundtrack.Should().Be(0);
        settings.CurrentSfxPack.Should().Be(0);
        settings.IsFullscreen.Should().BeFalse();
        settings.WindowWidth.Should().Be(512);
        settings.WindowHeight.Should().Be(512);
    }

    [Fact]
    public void DefaultBindings_MatchOriginalDefaults()
    {
        var file = new OptionsFile();
        IInputBindingProvider bindings = file;

        bindings.KeyboardLeft.Should().Be(new InputBinding("Left", "NumPad4"));
        bindings.KeyboardRight.Should().Be(new InputBinding("Right", "NumPad6"));
        bindings.KeyboardUp.Should().Be(new InputBinding("Up", "NumPad8"));
        bindings.KeyboardDown.Should().Be(new InputBinding("Down", "NumPad5"));
        bindings.KeyboardUse.Should().Be(new InputBinding("X", "V"));
        bindings.KeyboardMenu.Should().Be(new InputBinding("Z", "C"));
        bindings.KeyboardPause.Should().Be(new InputBinding("Escape", "Enter"));

        bindings.ControllerLeft.Should().Be(new InputBinding("DPadLeft", "LeftThumbstickLeft"));
        bindings.ControllerRight.Should().Be(new InputBinding("DPadRight", "LeftThumbstickRight"));
        bindings.ControllerUp.Should().Be(new InputBinding("DPadUp", "LeftThumbstickUp"));
        bindings.ControllerDown.Should().Be(new InputBinding("DPadDown", "LeftThumbstickDown"));
        bindings.ControllerUse.Should().Be(new InputBinding("A", "LeftShoulder"));
        bindings.ControllerMenu.Should().Be(new InputBinding("B", "RightShoulder"));
        bindings.ControllerPause.Should().Be(new InputBinding("Start", "Back"));
    }
}
