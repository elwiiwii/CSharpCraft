using Microsoft.Xna.Framework;

namespace CSharpCraft.MainMenu;

internal class MainMenuObj
{
    // Spotlight tuning — adjust to taste
    internal float InnerRadius = 15f; // px from button edge where reveal is 1.0
    internal float FadeWidth   = 20f; // px over which reveal fades from 1.0 to 0.0

    private float _mx = -1f;
    private float _my = -1f;

    // Color 17 (darker-blue background) — text at this color is invisible on the bg
    private static readonly Color DimColor = new(0x11, 0x1D, 0x35);
    // Color 7 (white)
    private static readonly Color WhiteColor = new(0xFF, 0xF1, 0xE8);
    // Disabled label text: white pre-blended through the color-17/alpha-200 tint
    private static readonly Color DisabledTextColor = Color.Lerp(WhiteColor, DimColor, 200f / 255f);

    internal void Update()
    {
        (_mx, _my) = Pico8.MouseScenePosition();
    }

    internal void Draw()
    {
        Pico8.Cls(17);

        for (int i = 0; i < MainMenuDefs.All.Length; i++)
        {
            MainMenuButtonDef btn = MainMenuDefs.All[i];

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
                float reveal = ComputeReveal(_mx, _my, btn, InnerRadius, FadeWidth);

                // Step 2 (enabled): fade in color sprite by spotlight amount
                if (reveal > 0f)
                {
                    Pico8.Pal();
                    Pico8.Palt(0, false);
                    Pico8.Sspr(btn.ColorSx, btn.ColorSy, btn.ColorSw, btn.ColorSh,
                               btn.DestX, btn.DestY, btn.DestW, btn.DestH);
                }

                // Step 3 (enabled): label fades from dim to label color by spotlight amount
                if (btn.Label.Length <= 0) continue;
                Pico8.Pal();
                Pico8.Palt();
                Pico8.Print(btn.Label, btn.LabelX, btn.LabelY,
                    Color.Lerp(Pico8.BasePalette.ElementAt(7).Key, Pico8.BasePalette.ElementAt(btn.LabelCol).Key, reveal));
            }
        }
    }

    /// <summary>
    /// Computes how much of the color sprite to reveal [0, 1] based on the
    /// distance from the cursor to the nearest edge of the button's dest rect.
    /// Inside <paramref name="innerRadius"/> → 1.0; fades to 0 over <paramref name="fadeWidth"/>.
    /// </summary>
    private static float ComputeReveal(
        float mx, float my, MainMenuButtonDef btn,
        float innerRadius, float fadeWidth)
    {
        float nearestX = Math.Clamp(mx, btn.DestX, btn.DestX + btn.DestW);
        float nearestY = Math.Clamp(my, btn.DestY, btn.DestY + btn.DestH);
        float dist = MathF.Sqrt((mx - nearestX) * (mx - nearestX)
                               + (my - nearestY) * (my - nearestY));
        return 1f - Math.Clamp((dist - innerRadius) / fadeWidth, 0f, 1f);
    }
}
