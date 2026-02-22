using Xunit;
using FluentAssertions;
using CSharpCraft.Pico8;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Tests.Pico8;

public class PaletteManagerTests
{
    // Standard PICO-8 palette subset for testing
    private static readonly Color C0 = new(0, 0, 0);        // black
    private static readonly Color C1 = new(29, 43, 83);     // dark blue
    private static readonly Color C2 = new(126, 37, 83);    // dark purple
    private static readonly Color C3 = new(0, 135, 81);     // dark green
    private static readonly Color C4 = new(171, 82, 54);    // brown
    private static readonly Color C5 = new(95, 87, 79);     // dark grey
    private static readonly Color C6 = new(194, 195, 199);  // light grey
    private static readonly Color C7 = new(255, 241, 232);  // white
    private static readonly Color C8 = new(255, 0, 77);     // red
    private static readonly Color C9 = new(255, 163, 0);    // orange
    private static readonly Color C10 = new(255, 236, 39);  // yellow
    private static readonly Color C11 = new(0, 228, 54);    // green
    private static readonly Color C12 = new(41, 173, 255);  // blue
    private static readonly Color C13 = new(131, 118, 156); // indigo
    private static readonly Color C14 = new(255, 119, 168); // pink
    private static readonly Color C15 = new(255, 204, 170); // peach

    private static List<Color> CreatePalette() =>
    [
        C0, C1, C2, C3, C4, C5, C6, C7,
        C8, C9, C10, C11, C12, C13, C14, C15
    ];

    private PaletteManager CreateManager() => new(CreatePalette());

    // ── Constructor ──

    [Fact]
    public void Constructor_InitializesPaletteMap()
    {
        var manager = CreateManager();
        manager.PaletteMap.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_SetsBlackAsTransparent()
    {
        var manager = CreateManager();
        var remapping = manager.TryGetRemapping(C0);
        remapping.Should().NotBeNull();
        remapping!.Trans.Should().BeTrue();
    }

    [Fact]
    public void Constructor_BlackRemapsToItself()
    {
        var manager = CreateManager();
        var remapping = manager.TryGetRemapping(C0);
        remapping.Should().NotBeNull();
        remapping!.C0.Should().Be(C0);
        remapping.C1.Should().Be(C0);
    }

    [Fact]
    public void Constructor_StoresStaticPalette()
    {
        var palette = CreatePalette();
        var manager = new PaletteManager(palette);
        manager.StaticPalette.Should().BeSameAs(palette);
    }

    [Fact]
    public void Constructor_OnlyBlackHasRemapping()
    {
        var manager = CreateManager();
        manager.PaletteMap.Should().HaveCount(1);
    }

    // ── TryGetRemapping ──

    [Fact]
    public void TryGetRemapping_ReturnsNull_ForUnmappedColor()
    {
        var manager = CreateManager();
        manager.TryGetRemapping(C7).Should().BeNull();
    }

    [Fact]
    public void TryGetRemapping_ReturnsRemapping_ForMappedColor()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        var result = manager.TryGetRemapping(C1);
        result.Should().NotBeNull();
    }

    // ── SetPalette (int, int) ──

    [Fact]
    public void SetPalette_IntOverload_RemapsCorrectly()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        var remapping = manager.TryGetRemapping(C1);
        remapping.Should().NotBeNull();
        remapping!.C0.Should().Be(C1);
        remapping.C1.Should().Be(C7);
    }

    [Fact]
    public void SetPalette_IntOverload_SetsTransparentFalse()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        var remapping = manager.TryGetRemapping(C1);
        remapping!.Trans.Should().BeFalse();
    }

    [Fact]
    public void SetPalette_IntOverload_OverwritesPreviousRemapping()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        manager.SetPalette(1, 3);
        var remapping = manager.TryGetRemapping(C1);
        remapping!.C1.Should().Be(C3);
    }

    [Fact]
    public void SetPalette_IntOverload_ThrowsOnNegativeC0()
    {
        var manager = CreateManager();
        var act = () => manager.SetPalette(-1, 5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPalette_IntOverload_ThrowsOnC0TooLarge()
    {
        var manager = CreateManager();
        var act = () => manager.SetPalette(16, 5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPalette_IntOverload_ThrowsOnNegativeC1()
    {
        var manager = CreateManager();
        var act = () => manager.SetPalette(5, -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPalette_IntOverload_ThrowsOnC1TooLarge()
    {
        var manager = CreateManager();
        var act = () => manager.SetPalette(5, 16);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetPalette_IntOverload_CanRemapColorToItself()
    {
        var manager = CreateManager();
        manager.SetPalette(5, 5);
        var remapping = manager.TryGetRemapping(C5);
        remapping.Should().NotBeNull();
        remapping!.C0.Should().Be(C5);
        remapping.C1.Should().Be(C5);
    }

    [Fact]
    public void SetPalette_IntOverload_DoesNotAffectBlackTransparency()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        // Black should still be there
        manager.TryGetRemapping(C0).Should().NotBeNull();
        manager.TryGetRemapping(C0)!.Trans.Should().BeTrue();
    }

    // ── SetPalette (Color, Color) ──

    [Fact]
    public void SetPalette_ColorOverload_RemapsCorrectly()
    {
        var manager = CreateManager();
        manager.SetPalette(C8, C12);
        var remapping = manager.TryGetRemapping(C8);
        remapping.Should().NotBeNull();
        remapping!.C0.Should().Be(C8);
        remapping.C1.Should().Be(C12);
    }

    [Fact]
    public void SetPalette_ColorOverload_SetsTransparentFalse()
    {
        var manager = CreateManager();
        manager.SetPalette(C8, C12);
        manager.TryGetRemapping(C8)!.Trans.Should().BeFalse();
    }

    [Fact]
    public void SetPalette_ColorOverload_OverwritesPreviousRemapping()
    {
        var manager = CreateManager();
        manager.SetPalette(C8, C12);
        manager.SetPalette(C8, C3);
        manager.TryGetRemapping(C8)!.C1.Should().Be(C3);
    }

    [Fact]
    public void SetPalette_ColorOverload_CanRemapBlack()
    {
        var manager = CreateManager();
        // Overwrite the default black → black mapping
        manager.SetPalette(C0, C7);
        var remapping = manager.TryGetRemapping(C0);
        remapping!.C1.Should().Be(C7);
        remapping.Trans.Should().BeFalse();
    }

    // ── ResetPalette ──

    [Fact]
    public void ResetPalette_ClearsAllRemappings()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        manager.SetPalette(3, 9);
        manager.SetPalette(5, 11);
        manager.ResetPalette();
        // Only black should remain
        manager.PaletteMap.Should().HaveCount(1);
    }

    [Fact]
    public void ResetPalette_RestoresBlackTransparency()
    {
        var manager = CreateManager();
        manager.SetPalette(C0, C7); // Override black
        manager.ResetPalette();
        var black = manager.TryGetRemapping(C0);
        black.Should().NotBeNull();
        black!.Trans.Should().BeTrue();
        black.C0.Should().Be(C0);
        black.C1.Should().Be(C0);
    }

    [Fact]
    public void ResetPalette_ClearsNonBlackRemappings()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        manager.ResetPalette();
        manager.TryGetRemapping(C1).Should().BeNull();
    }

    // ── SetTransparency (int) ──

    [Fact]
    public void SetTransparency_Int_MakesColorTransparent()
    {
        var manager = CreateManager();
        manager.SetTransparency(7, true);
        var remapping = manager.TryGetRemapping(C7);
        remapping.Should().NotBeNull();
        remapping!.Trans.Should().BeTrue();
    }

    [Fact]
    public void SetTransparency_Int_MakesColorOpaque()
    {
        var manager = CreateManager();
        // Black starts transparent
        manager.SetTransparency(0, false);
        manager.TryGetRemapping(C0)!.Trans.Should().BeFalse();
    }

    [Fact]
    public void SetTransparency_Int_CreatesRemappingIfNoneExists()
    {
        var manager = CreateManager();
        manager.TryGetRemapping(C7).Should().BeNull();
        manager.SetTransparency(7, true);
        manager.TryGetRemapping(C7).Should().NotBeNull();
    }

    [Fact]
    public void SetTransparency_Int_PreservesExistingRemapping()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        manager.SetTransparency(1, true);
        var remapping = manager.TryGetRemapping(C1);
        remapping!.Trans.Should().BeTrue();
        // The color remapping should still be there
        remapping.C1.Should().Be(C7);
    }

    [Fact]
    public void SetTransparency_Int_ThrowsOnNegativeIndex()
    {
        var manager = CreateManager();
        var act = () => manager.SetTransparency(-1, true);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetTransparency_Int_ThrowsOnIndexTooLarge()
    {
        var manager = CreateManager();
        var act = () => manager.SetTransparency(16, true);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── SetTransparency (Color) ──

    [Fact]
    public void SetTransparency_Color_MakesColorTransparent()
    {
        var manager = CreateManager();
        manager.SetTransparency(C7, true);
        manager.TryGetRemapping(C7)!.Trans.Should().BeTrue();
    }

    [Fact]
    public void SetTransparency_Color_CreatesRemappingIfNoneExists()
    {
        var manager = CreateManager();
        manager.SetTransparency(C14, true);
        var remapping = manager.TryGetRemapping(C14);
        remapping.Should().NotBeNull();
        remapping!.C0.Should().Be(C14);
        remapping.C1.Should().Be(C14); // maps to itself
    }

    // ── ResetTransparency ──

    [Fact]
    public void ResetTransparency_SetsAllRemappingsToOpaque()
    {
        var manager = CreateManager();
        manager.SetTransparency(7, true);
        manager.SetTransparency(3, true);
        manager.ResetTransparency();
        // Non-black colors should be opaque
        manager.TryGetRemapping(C7)!.Trans.Should().BeFalse();
        manager.TryGetRemapping(C3)!.Trans.Should().BeFalse();
    }

    [Fact]
    public void ResetTransparency_KeepsBlackTransparent()
    {
        var manager = CreateManager();
        manager.SetTransparency(7, true);
        manager.ResetTransparency();
        manager.TryGetRemapping(C0)!.Trans.Should().BeTrue();
    }

    [Fact]
    public void ResetTransparency_RestoresBlackEvenIfModified()
    {
        var manager = CreateManager();
        manager.SetTransparency(0, false);
        manager.ResetTransparency();
        manager.TryGetRemapping(C0)!.Trans.Should().BeTrue();
    }

    // ── GetAllRemappings ──

    [Fact]
    public void GetAllRemappings_ReturnsListOfAllMappings()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        manager.SetPalette(3, 9);
        var all = manager.GetAllRemappings();
        // black + 2 custom = 3
        all.Should().HaveCount(3);
    }

    [Fact]
    public void GetAllRemappings_InitiallyContainsOnlyBlack()
    {
        var manager = CreateManager();
        var all = manager.GetAllRemappings();
        all.Should().HaveCount(1);
        all[0].C0.Should().Be(C0);
    }

    // ── Edge cases ──

    [Fact]
    public void MultipleRemappings_AreIndependent()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        manager.SetPalette(3, 9);
        manager.SetPalette(5, 11);

        manager.TryGetRemapping(C1)!.C1.Should().Be(C7);
        manager.TryGetRemapping(C3)!.C1.Should().Be(C9);
        manager.TryGetRemapping(C5)!.C1.Should().Be(C11);
    }

    [Fact]
    public void SetPalette_ThenSetTransparency_BothApply()
    {
        var manager = CreateManager();
        manager.SetPalette(1, 7);
        manager.SetTransparency(1, true);
        var remapping = manager.TryGetRemapping(C1);
        remapping!.C1.Should().Be(C7);
        remapping.Trans.Should().BeTrue();
    }
}
