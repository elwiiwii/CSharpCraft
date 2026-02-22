using CSharpCraft.OptionsMenu;
using CSharpCraft.Pico8;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.OptionsMenu;

/// <summary>
/// Tests for OptionsFile.Current static accessor and FixFile validation logic.
/// </summary>
public class OptionsFileValidationTests
{
    [Fact]
    public void Current_CanBeSetAndRetrieved()
    {
        var file = new OptionsFile();
        OptionsFile.Current = file;

        OptionsFile.Current.Should().BeSameAs(file);

        // Cleanup
        OptionsFile.Current = null!;
    }

    [Fact]
    public void FixFile_ClampsVolumeAbove100()
    {
        var file = new OptionsFile { Gen_Music_Vol = 150, Gen_Sfx_Vol = 200 };

        // FixFile is called internally by Validate
        OptionsFile.Validate(file);

        file.Gen_Music_Vol.Should().Be(100);
        file.Gen_Sfx_Vol.Should().Be(100);
    }

    [Fact]
    public void FixFile_ClampsVolumeBelowZero()
    {
        var file = new OptionsFile { Gen_Music_Vol = -10, Gen_Sfx_Vol = -5 };

        OptionsFile.Validate(file);

        file.Gen_Music_Vol.Should().Be(100); // reset to default
        file.Gen_Sfx_Vol.Should().Be(100);   // reset to default
    }

    [Fact]
    public void FixFile_ClampsWindowSizeBelowMinimum()
    {
        var file = new OptionsFile { Gen_Window_Width = 0, Gen_Window_Height = -1 };

        OptionsFile.Validate(file);

        file.Gen_Window_Width.Should().Be(512); // reset to default
        file.Gen_Window_Height.Should().Be(512); // reset to default
    }

    [Fact]
    public void FixFile_ClampsWindowSizeAboveMaximum()
    {
        var file = new OptionsFile { Gen_Window_Width = 99999, Gen_Window_Height = 99999 };

        OptionsFile.Validate(file);

        file.Gen_Window_Width.Should().Be(512);  // reset to default
        file.Gen_Window_Height.Should().Be(512); // reset to default
    }

    [Fact]
    public void FixFile_ValidValues_ArePreserved()
    {
        var file = new OptionsFile
        {
            Gen_Music_Vol = 50,
            Gen_Sfx_Vol = 75,
            Gen_Window_Width = 1024,
            Gen_Window_Height = 768,
            Gen_Sound_On = false,
            Gen_Fullscreen = true,
        };

        OptionsFile.Validate(file);

        file.Gen_Music_Vol.Should().Be(50);
        file.Gen_Sfx_Vol.Should().Be(75);
        file.Gen_Window_Width.Should().Be(1024);
        file.Gen_Window_Height.Should().Be(768);
        file.Gen_Sound_On.Should().BeFalse();
        file.Gen_Fullscreen.Should().BeTrue();
    }

    [Fact]
    public void FixFile_InvalidKeyboardBinding_ResetsToDefault()
    {
        var file = new OptionsFile
        {
            Kbm_Left = new InputBinding("INVALID_KEY", "NumPad4")
        };

        OptionsFile.Validate(file);

        // Invalid Bind1 should be reset to default
        file.Kbm_Left.Bind1.Should().Be("Left");
        // Valid Bind2 should be preserved
        file.Kbm_Left.Bind2.Should().Be("NumPad4");
    }

    [Fact]
    public void FixFile_NullKeyboardBinding_ResetsToDefault()
    {
        var file = new OptionsFile
        {
            Kbm_Left = new InputBinding(null!, "NumPad4")
        };

        OptionsFile.Validate(file);

        file.Kbm_Left.Bind1.Should().Be("Left");
        file.Kbm_Left.Bind2.Should().Be("NumPad4");
    }

    [Fact]
    public void FixFile_InvalidControllerBinding_ResetsToDefault()
    {
        var file = new OptionsFile
        {
            Con_Left = new InputBinding("INVALID_BUTTON", "LeftThumbstickLeft")
        };

        OptionsFile.Validate(file);

        file.Con_Left.Bind1.Should().Be("DPadLeft");
        file.Con_Left.Bind2.Should().Be("LeftThumbstickLeft");
    }

    [Fact]
    public void FixFile_NegativeSoundtrack_ResetsToDefault()
    {
        var file = new OptionsFile { Pcraft_Soundtrack = -1 };

        OptionsFile.Validate(file);

        file.Pcraft_Soundtrack.Should().Be(0);
    }

    [Fact]
    public void FixFile_NegativeSfxPack_ResetsToDefault()
    {
        var file = new OptionsFile { Pcraft_Sfx_Pack = -1 };

        OptionsFile.Validate(file);

        file.Pcraft_Sfx_Pack.Should().Be(0);
    }
}
