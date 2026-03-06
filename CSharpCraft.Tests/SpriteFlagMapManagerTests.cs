using CSharpCraft.Tests.Infrastructure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pico8.Graphics;
using Xunit;

namespace CSharpCraft.Tests;

[Collection("Graphics")]
public partial class SpriteFlagMapManagerTests
{
    private readonly GraphicsDevice _gd;

    public SpriteFlagMapManagerTests(GraphicsFixture fixture)
    {
        _gd = fixture.GraphicsDevice;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a W×H texture uniformly filled with <paramref name="color"/>.
    /// </summary>
    private Texture2D MakeSolidTexture(Color color, int w, int h)
    {
        var tex = new Texture2D(_gd, w, h);
        var pixels = new Color[w * h];
        Array.Fill(pixels, color);
        tex.SetData(pixels);
        return tex;
    }

    /// <summary>
    /// Creates an SFM using only the post-split parameters (no GPU/palette args).
    /// Update this one method when Phase 6 changes the constructor.
    /// </summary>
    private SpriteFlagMapManager MakeSfm(
        Texture2D sprite,
        Texture2D map,
        string flags = "")
        => new(_gd, new PaletteManager(), sprite, map, flags);

    // A simple 16×8 spritesheet (2 sprites wide × 1 tall) and 16×8 map (2×1 tiles).
    private (SpriteFlagMapManager sfm, Texture2D sprite, Texture2D map)
        MakeDefault(Color spriteColor = default)
    {
        if (spriteColor == default) spriteColor = Color.Red;
        var sprite = MakeSolidTexture(spriteColor, 16, 8);
        var map = MakeSolidTexture(spriteColor, 16, 8); // matches sprite 0
        return (MakeSfm(sprite, map), sprite, map);
    }
}
