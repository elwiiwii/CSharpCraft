namespace CSharpCraft.PcraftBase.Draw;

internal static class EntDrawer
{
    internal static void DrawEnt(WorldState state)
    {
        foreach (var e in state.Entities)
        {
            Pico8.Pal();
            if (e.Type.Pal != null) DrawHelpers.SetPal(e.Type.Pal);

            if (e.Type.BigSpr != 0)
            {
                Pico8.Spr(e.Type.BigSpr, e.X.Double - 8, e.Y.Double - 8, 2, 2);
            }
            else if (e.Type == PcraftData.EText)
            {
                DrawHelpers.PrintB(e.TextValue.Double.ToString("0"), e.X.Double - 2, e.Y.Double - 4, e.TextColor);
            }
            else
            {
                if (e.Timer.HasValue &&
                    e.Timer.Value < 45 &&
                    e.Timer.Value % 4 > 2)
                {
                    for (int k = 0; k <= 15; k++)
                        Pico8.Palt(k, true);
                }
                Pico8.Spr(e.Type.Spr, e.X.Double - 4, e.Y.Double - 4);
            }
        }
    }
}
