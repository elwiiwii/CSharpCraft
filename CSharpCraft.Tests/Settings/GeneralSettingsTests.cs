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
        _ = new GeneralSettings().MusicVolume.Should().Be(100);
    }

    [Fact]
    public void Default_SfxVolume_Is100()
    {
        _ = new GeneralSettings().SfxVolume.Should().Be(100);
    }

    [Fact]
    public void Default_Fullscreen_IsFalse()
    {
        _ = new GeneralSettings().Fullscreen.Should().BeFalse();
    }

    [Fact]
    public void Default_WindowWidth_Is512()
    {
        _ = new GeneralSettings().WindowWidth.Should().Be(512);
    }

    [Fact]
    public void Default_WindowHeight_Is512()
    {
        _ = new GeneralSettings().WindowHeight.Should().Be(512);
    }

    [Fact]
    public void Default_InputBindings_IsNotNull()
    {
        _ = new GeneralSettings().InputBindings.Should().NotBeNull();
    }

    [Fact]
    public void Default_BtnpConfig_IsNotNull()
    {
        _ = new GeneralSettings().BtnpConfig.Should().NotBeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Json Serialization
    // --------------------------------------------------------------------------

    [Fact]
    public void JsonRoundTrip_PreservesScalarValues()
    {
        GeneralSettings original = new()
        {
            MusicVolume = 75,
            SfxVolume = 50,
            Fullscreen = true,
            WindowWidth = 1024,
            WindowHeight = 768,
        };

        string json = JsonSerializer.Serialize(original, JsonOptions);
        GeneralSettings? restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        _ = restored.Should().NotBeNull();
        _ = restored!.MusicVolume.Should().Be(75);
        _ = restored.SfxVolume.Should().Be(50);
        _ = restored.Fullscreen.Should().BeTrue();
        _ = restored.WindowWidth.Should().Be(1024);
        _ = restored.WindowHeight.Should().Be(768);
    }

    [Fact]
    public void JsonRoundTrip_PreservesKeyboardBinding()
    {
        InputBindings bindings = new(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Left] = [new KeyboardSource(Microsoft.Xna.Framework.Input.Keys.A)],
        });
        GeneralSettings original = new() { InputBindings = bindings };

        string json = JsonSerializer.Serialize(original, JsonOptions);
        GeneralSettings? restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        _ = restored.Should().NotBeNull();
        IReadOnlyList<InputSource> leftBindings = restored!.InputBindings[PicoButton.Left];
        _ = leftBindings.Should().HaveCount(1);
        _ = leftBindings[0].Should().BeOfType<KeyboardSource>()
            .Which.Key.Should().Be(Microsoft.Xna.Framework.Input.Keys.A);
    }

    [Fact]
    public void JsonRoundTrip_PreservesGamePadBinding()
    {
        InputBindings bindings = new(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Primary] = [new GamePadSource(Microsoft.Xna.Framework.Input.Buttons.A)],
        });
        GeneralSettings original = new() { InputBindings = bindings };

        string json = JsonSerializer.Serialize(original, JsonOptions);
        GeneralSettings? restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        IReadOnlyList<InputSource> primaryBindings = restored!.InputBindings[PicoButton.Primary];
        _ = primaryBindings.Should().HaveCount(1);
        _ = primaryBindings[0].Should().BeOfType<GamePadSource>()
            .Which.Button.Should().Be(Microsoft.Xna.Framework.Input.Buttons.A);
    }

    [Fact]
    public void JsonRoundTrip_PreservesBtnpConfig()
    {
        GeneralSettings original = new()
        {
            BtnpConfig = new BtnpConfig(InitialRepeatMs: 200, SubsequentRepeatMs: 50),
        };

        string json = JsonSerializer.Serialize(original, JsonOptions);
        GeneralSettings? restored = JsonSerializer.Deserialize<GeneralSettings>(json, JsonOptions);

        _ = restored!.BtnpConfig.InitialRepeatMs.Should().Be(200);
        _ = restored.BtnpConfig.SubsequentRepeatMs.Should().Be(50);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
}
