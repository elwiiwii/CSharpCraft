using CSharpCraft.MenuShared;
using Microsoft.Xna.Framework;
using PSharp8.Graphics;
using PSharp8.Input;

namespace CSharpCraft.CreditsMenu;

internal class CreditsMenuObj
{
    // Hover animation speed: full transition in 12 frames (~0.2 s at 60 fps)
    private const float HoverStep = 1f / 12f;
    private const float HoverRadius = 4f;

    // Per-button hover progress: 0 = mono (idle), 1 = full color (hovered)
    private readonly float[] _hoverT = new float[CreditsMenuDefs.All.Length];

    // Color 17 (darker-blue background) — text at this color is invisible on the bg
    private static readonly Color DimColor = new(0x11, 0x1D, 0x35);
    // Color 7 (white)
    private static readonly Color WhiteColor = new(0xFF, 0xF1, 0xE8);
    // Disabled label text: white pre-blended through the color-17/alpha-200 tint
    private static readonly Color DisabledTextColor = Color.Lerp(WhiteColor, DimColor, 200f / 255f);

    internal void Update()
    {
        var (mx, my) = Pico8.MouseState().ScenePosition;
        var cursor = new Vector2(mx, my);

        // Find the (enabled) button whose bounds the cursor is furthest inside
        int bestIndex = -1;
        float bestDepth = 0f;
        for (int i = 0; i < CreditsMenuDefs.All.Length; i++)
        {
            if (!CreditsMenuDefs.All[i].IsEnabled) continue;
            float depth = MenuButtonGeometry.SignedDistanceToRoundedHull(
                cursor, CreditsMenuDefs.All[i].HullVertices, HoverRadius);
            if (depth > bestDepth && depth > 0f)
            {
                bestDepth = depth;
                bestIndex = i;
            }
        }

        for (int i = 0; i < CreditsMenuDefs.All.Length; i++)
        {
            MenuButtonDef btn = CreditsMenuDefs.All[i];

            if (!btn.IsEnabled)
            {
                _hoverT[i] = 0f;
                continue;
            }

            bool hovered = i == bestIndex;

            _hoverT[i] = Math.Clamp(_hoverT[i] + (hovered ? HoverStep : -HoverStep), 0f, 1f);

            if (hovered && (Pico8.MouseState().LeftButton == InputState.Press || Pico8.Btnp(PicoButton.Primary, repeat: false)))
            {
                btn.Action?.Invoke();
            }
        }
    }

    internal void Draw()
    {
        Pico8.Cls(PicoColor._17DarkerBlue);

        for (int i = 0; i < CreditsMenuDefs.All.Length; i++)
        {
            MenuButtonDef btn = CreditsMenuDefs.All[i];
            float t = _hoverT[i];

            // Step 1: draw the monochrome sprite (always visible as base)
            Pico8.Pal();
            Pico8.Palt();
            Pico8.Sspr(btn.MonoSx, btn.MonoSy, btn.MonoSw, btn.MonoSh,
                       btn.DestX, btn.DestY, btn.DestW, btn.DestH);
            Pico8.Print(btn.Label, btn.LabelX, btn.LabelY, PicoColor._07White);

            if (!btn.IsEnabled)
            {
                // Step 2 (disabled): fixed color-17 tint overlay at alpha 200
                Pico8.PalAll(PicoColor._17DarkerBlue);
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