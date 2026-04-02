using System.Text.Json;
using CSharpCraft.Settings;
using FluentAssertions;
using PSharp8.Input;
using Xunit;

namespace CSharpCraft.Tests.Settings;

public sealed class GeneralSettingsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    // --------------------------------------------------------------------------
    #region Defaults
    // --------------------------------------------------------------------------

    [Fact]
    public void Default_MusicVolume_Is100()
    {
        new GeneralSettings().MusicVolume.Should().Be(100);
    }

    [Fact]
    public void Default_SfxVolume_Is100()
    {
        new GeneralSettings().SfxVolume.Should().Be(100);
    }

    [Fact]
    public void Default_Fullscreen_IsFalse()
    {
        new GeneralSettings().Fullscreen.Should().BeFalse();
    }

    [Fact]
    public void Default_WindowWidth_Is512()
    {
        new GeneralSettings().WindowWidth.Should().Be(512);
    }

    [Fact]
    public void Default_WindowHeight_Is512()
    {
        new GeneralSettings().WindowHeight.Should().Be(512);
    }

    [Fact]
    public void Default_InputBindings_IsNotNull()
    {
        new GeneralSettings().InputBindings.Should().NotBeNull();
    }

    [Fact]
    public void Default_BtnpConfig_IsNotNull()
    {
        new GeneralSettings().BtnpConfig.Should().NotBeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Json Serialization
    // --------------------------------------------------------------------------

    [Fact]
    public void JsonRoundTrip_PreservesScalarValues()
    {
        var original = new GeneralSettings
        {
            MusicVolume = 75,
            SfxVolume = 50,
            Fullscreen = true,
            WindowWidth = 1024,
            WindowHeight = 768,
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        restored.Should().NotBeNull();
        restored!.MusicVolume.Should().Be(75);
        restored.SfxVolume.Should().Be(50);
        restored.Fullscreen.Should().BeTrue();
        restored.WindowWidth.Should().Be(1024);
        restored.WindowHeight.Should().Be(768);
    }

    [Fact]
    public void JsonRoundTrip_PreservesKeyboardBinding()
    {
        var bindings = new InputBindings(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Left] = [new KeyboardSource(Microsoft.Xna.Framework.Input.Keys.A)],
        });
        var original = new GeneralSettings { InputBindings = bindings };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        restored.Should().NotBeNull();
        var leftBindings = restored!.InputBindings[PicoButton.Left];
        leftBindings.Should().HaveCount(1);
        leftBindings[0].Should().BeOfType<KeyboardSource>()
            .Which.Key.Should().Be(Microsoft.Xna.Framework.Input.Keys.A);
    }

    [Fact]
    public void JsonRoundTrip_PreservesGamePadBinding()
    {
        var bindings = new InputBindings(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Primary] = [new GamePadSource(Microsoft.Xna.Framework.Input.Buttons.A)],
        });
        var original = new GeneralSettings { InputBindings = bindings };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        var primaryBindings = restored!.InputBindings[PicoButton.Primary];
        primaryBindings.Should().HaveCount(1);
        primaryBindings[0].Should().BeOfType<GamePadSource>()
            .Which.Button.Should().Be(Microsoft.Xna.Framework.Input.Buttons.A);
    }

    [Fact]
    public void JsonRoundTrip_PreservesBtnpConfig()
    {
        var original = new GeneralSettings
        {
            BtnpConfig = new BtnpConfig(InitialRepeatMs: 200, SubsequentRepeatMs: 50),
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        restored!.BtnpConfig.InitialRepeatMs.Should().Be(200);
        restored.BtnpConfig.SubsequentRepeatMs.Should().Be(50);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
}
