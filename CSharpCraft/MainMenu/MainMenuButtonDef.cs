using Microsoft.Xna.Framework;

namespace CSharpCraft.MainMenu;

internal record MainMenuButtonDef(
    int MonoSx, int MonoSy, int MonoSw, int MonoSh,
    int ColorSx, int ColorSy, int ColorSw, int ColorSh,
    int DestX, int DestY, int DestW, int DestH,
    string Label = "", int LabelX = 0, int LabelY = 0, int LabelCol = 17,
    bool IsEnabled = true, Action? Action = null)
{
    internal IReadOnlyList<Vector2> HullVertices { get; } = ComputeHull(DestX, DestY, DestW, DestH, Label, LabelX, LabelY);

    private static Vector2[] ComputeHull(int destX, int destY, int destW, int destH,
        string label, int labelX, int labelY)
    {
        Vector2[] spriteCorners = MainMenuGeometry.GetRectCorners(destX, destY, destW, destH);

        var labelBounds = MainMenuGeometry.GetLabelBounds(label, labelX, labelY);
        if (labelBounds is null)
            return MainMenuGeometry.ConvexHull(spriteCorners.AsSpan());

        Vector2[] labelCorners = MainMenuGeometry.GetRectCorners(
            labelBounds.Value.X, labelBounds.Value.Y,
            labelBounds.Value.W, labelBounds.Value.H);

        Vector2[] allPoints = new Vector2[spriteCorners.Length + labelCorners.Length];
        spriteCorners.CopyTo(allPoints, 0);
        labelCorners.CopyTo(allPoints, spriteCorners.Length);

        return MainMenuGeometry.ConvexHull(allPoints.AsSpan());
    }
}
