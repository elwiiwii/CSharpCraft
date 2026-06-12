using Microsoft.Xna.Framework;
using PSharp8.Input;

namespace CSharpCraft.MainMenu;

internal class MainMenuObj
{
    // Hover animation speed: full transition in 12 frames (~0.2 s at 60 fps)
    private const float HoverStep = 1f / 12f;
    private const float HoverRadius = 4f;

    // Per-button hover progress: 0 = mono (idle), 1 = full color (hovered)
    private readonly float[] _hoverT = new float[MainMenuDefs.All.Length];

    // Color 17 (darker-blue background) — text at this color is invisible on the bg
    private static readonly Color DimColor = new(0x11, 0x1D, 0x35);
    // Color 7 (white)
    private static readonly Color WhiteColor = new(0xFF, 0xF1, 0xE8);
    // Disabled label text: white pre-blended through the color-17/alpha-200 tint
    private static readonly Color DisabledTextColor = Color.Lerp(WhiteColor, DimColor, 200f / 255f);

    internal void Update()
    {
        var (mx, my) = Pico8.MouseScenePosition();

        for (int i = 0; i < MainMenuDefs.All.Length; i++)
        {
            MainMenuButtonDef btn = MainMenuDefs.All[i];

            if (!btn.IsEnabled)
            {
                _hoverT[i] = 0f;
                continue;
            }

            bool hovered = MainMenuGeometry.IsPointInRoundedHull(
                new Vector2(mx, my), btn.HullVertices, HoverRadius);

            _hoverT[i] = Math.Clamp(_hoverT[i] + (hovered ? HoverStep : -HoverStep), 0f, 1f);

            if (hovered && (Pico8.MouseState().LeftButton == MouseInput.Press || Pico8.Btnp(4)))
            {
                btn.Action?.Invoke();
            }
        }
    }

    internal void Draw()
    {
        Pico8.Cls(17);

        for (int i = 0; i < MainMenuDefs.All.Length; i++)
        {
            MainMenuButtonDef btn = MainMenuDefs.All[i];
            float t = _hoverT[i];

            // Step 1: draw the monochrome sprite (always visible as base)
            Pico8.Pal();
            Pico8.Palt();
            Pico8.Sspr(btn.MonoSx, btn.MonoSy, btn.MonoSw, btn.MonoSh,
                       btn.DestX, btn.DestY, btn.DestW, btn.DestH);
            Pico8.Print(btn.Label, btn.LabelX, btn.LabelY, 7);

            if (!btn.IsEnabled)
            {
                // Step 2 (disabled): fixed color-17 tint overlay at alpha 200
                Pico8.PalAll(17);
                Pico8.PaltAll(200);
                Pico8.Palt(0, true); // keep transparent pixels clear
                Pico8.Sspr(btn.MonoSx, btn.MonoSy, btn.MonoSw, btn.MonoSh,
                           btn.DestX, btn.DestY, btn.DestW, btn.DestH);

                // Step 3 (disabled): pre-blended white label
                if (btn.Label.Length <= 0) continue;
                Pico8.Pal();
                Pico8.Palt();
                Pico8.Print(btn.Label, btn.LabelX, btn.LabelY, DisabledTextColor);
            }
            else
            {
                // Step 2 (enabled): fade in color sprite on hover
                if (t > 0f)
                {
                    Pico8.Pal();
                    Pico8.Palt(0, false);
                    Pico8.PaltAll((int)(255 * t));
                    Pico8.Sspr(btn.ColorSx, btn.ColorSy, btn.ColorSw, btn.ColorSh,
                               btn.DestX, btn.DestY, btn.DestW, btn.DestH);
                }

                // Step 3 (enabled): label fades from white to label color on hover
                if (btn.Label.Length <= 0) continue;
                Pico8.Pal();
                Pico8.Palt();
                Pico8.Print(btn.Label, btn.LabelX, btn.LabelY,
                    Color.Lerp(Pico8.BasePalette.ElementAt(7).Key, Pico8.BasePalette.ElementAt(btn.LabelCol).Key, t));
            }
        }
    }
}