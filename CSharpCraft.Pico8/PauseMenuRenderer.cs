using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8;

/// <summary>
/// Renders the pause menu overlay on top of the game scene.
/// Uses IGraphicsAPI for PICO-8 primitives and ITextureRenderer for the PauseArrow texture.
/// Extracted from OverlayRenderer — separated from popup notifications (SRP).
/// </summary>
public class PauseMenuRenderer : IPauseMenuRenderer
{
    private readonly IGraphicsAPI _graphics;
    private readonly ITextureRenderer _textureRenderer;

    public PauseMenuRenderer(IGraphicsAPI graphics, ITextureRenderer textureRenderer)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _textureRenderer = textureRenderer ?? throw new ArgumentNullException(nameof(textureRenderer));
    }

    /// <summary>
    /// Draw the pause menu overlay. Renders border, selection arrow, and menu item text.
    /// </summary>
    public void DrawPauseMenu(List<MenuItem> menuItems, int selectedIndex, (int Width, int Height) cell)
    {
        int i = (int)Math.Floor(64 - (menuItems.Count / 2.0) * 8);

        int xborder = 23;
        _graphics.Rectfill(0 + xborder, i - 7, 127 - xborder, i + menuItems.Count * 8 + 2, 0);
        _graphics.Rectfill(0 + xborder + 1, i - 7 + 1, 127 - xborder - 1, i + menuItems.Count * 8 + 2 - 1, 7);
        _graphics.Rectfill(0 + xborder + 2, i - 7 + 2, 127 - xborder - 2, i + menuItems.Count * 8 + 2 - 2, 0);

        // PauseArrow via ITextureRenderer (virtual coordinates, not raw SpriteBatch)
        _textureRenderer.Draw("PauseArrow",
            xborder + 4, i - 1 + selectedIndex * 8,
            Color.White, cell.Width, cell.Height);

        for (int j = 0; j < menuItems.Count; j++)
        {
            int indent = selectedIndex == j ? 1 : 0;
            _graphics.Print(menuItems[j].GetName(), xborder + indent + 12, i, 7);
            i += 8;
        }
    }
}
