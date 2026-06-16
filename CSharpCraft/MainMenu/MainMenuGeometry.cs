using Microsoft.Xna.Framework;

namespace CSharpCraft.MainMenu;

internal static class MainMenuGeometry
{
    private const int CharWidth = 4;
    private const int CharHeight = 6;

    internal static Vector2[] GetRectCorners(int x, int y, int w, int h)
    {
        return
        [
            new(x, y),                // top-left
            new(x + w, y),            // top-right
            new(x + w, y + h),        // bottom-right
            new(x, y + h),            // bottom-left
        ];
    }

    internal static (int X, int Y, int W, int H)? GetLabelBounds(string label, int labelX, int labelY)
    {
        if (string.IsNullOrEmpty(label))
            return null;

        return (labelX, labelY, label.Length * CharWidth, CharHeight);
    }

    internal static Vector2[] ConvexHull(ReadOnlySpan<Vector2> points)
    {
        if (points.Length <= 2)
            return points.ToArray();

        // Sort by x, then y
        Vector2[] sorted = points.ToArray();
        Array.Sort(sorted, (a, b) =>
        {
            int cmp = a.X.CompareTo(b.X);
            return cmp != 0 ? cmp : a.Y.CompareTo(b.Y);
        });

        // Build lower hull
        List<Vector2> lower = new(sorted.Length);
        foreach (var t in sorted)
        {
            while (lower.Count >= 2 && Cross(lower[^2], lower[^1], t) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(t);
        }

        // Build upper hull
        List<Vector2> upper = new(sorted.Length);
        for (int i = sorted.Length - 1; i >= 0; i--)
        {
            while (upper.Count >= 2 && Cross(upper[^2], upper[^1], sorted[i]) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(sorted[i]);
        }

        // Remove duplicate endpoints (last point of lower == first point of upper)
        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        // Combine in CCW order: lower (left to right) + upper (right to left)
        Vector2[] hull = new Vector2[lower.Count + upper.Count];
        lower.CopyTo(hull, 0);
        upper.CopyTo(hull, lower.Count);

        return hull;
    }

    private static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }

    internal static bool IsPointInRoundedHull(Vector2 point, IReadOnlyList<Vector2> hull, float radius)
    {
        if (hull.Count == 0)
            return false;

        if (hull.Count == 1)
            return Vector2.Distance(point, hull[0]) <= radius;

        // Check if point is inside the convex polygon
        if (IsPointInConvexPolygon(point, hull))
            return true;

        // Check distance to each edge segment
        for (int i = 0; i < hull.Count; i++)
        {
            Vector2 a = hull[i];
            Vector2 b = hull[(i + 1) % hull.Count];
            float dist = DistanceToSegment(point, a, b);
            if (dist <= radius)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a continuous signed distance from <paramref name="point"/> to the
    /// outer boundary of the rounded convex hull.
    /// Positive = inside, zero = on the rounded boundary, negative = outside.
    /// The value is continuous across the polygon/rounded-zone boundary:
    /// interior depth = <c>radius + dist_to_polygon_edge</c>,
    /// exterior (within radius) depth = <c>radius - dist_to_polygon_edge</c>.
    /// </summary>
    internal static float SignedDistanceToRoundedHull(Vector2 point, IReadOnlyList<Vector2> hull, float radius)
    {
        if (hull.Count == 0)
            return float.MinValue;

        if (hull.Count == 1)
            return radius - Vector2.Distance(point, hull[0]);

        // Distance to the nearest edge segment
        float minEdgeDist = float.MaxValue;
        for (int i = 0; i < hull.Count; i++)
        {
            Vector2 a = hull[i];
            Vector2 b = hull[(i + 1) % hull.Count];
            float dist = DistanceToSegment(point, a, b);
            if (dist < minEdgeDist)
                minEdgeDist = dist;
        }

        if (IsPointInConvexPolygon(point, hull))
            return minEdgeDist + radius; // depth from outer boundary = radius + dist inside polygon

        // Outside polygon — check rounded radius
        if (minEdgeDist <= radius)
            return radius - minEdgeDist; // positive: depth within rounded zone

        // Outside entirely
        return -(minEdgeDist - radius); // negative: how far outside
    }

    private static bool IsPointInConvexPolygon(Vector2 point, IReadOnlyList<Vector2> hull)
    {
        // For CCW hull: all cross products should be >= 0 (or all <= 0 for CW)
        bool? positive = null;

        for (int i = 0; i < hull.Count; i++)
        {
            Vector2 a = hull[i];
            Vector2 b = hull[(i + 1) % hull.Count];
            float cross = Cross(a, b, point);

            if (cross == 0)
                continue; // on the edge line — treat as inside

            bool isPositive = cross > 0;

            if (positive is null)
            {
                positive = isPositive;
            }
            else if (positive != isPositive)
            {
                return false; // sign change => outside
            }
        }

        return true;
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float lenSq = ab.LengthSquared();

        if (lenSq == 0)
            return Vector2.Distance(p, a); // a == b, it's a point

        // Projection parameter t, clamped to [0, 1]
        float t = Math.Clamp(Vector2.Dot(ap, ab) / lenSq, 0f, 1f);

        Vector2 closest = a + t * ab;
        return Vector2.Distance(p, closest);
    }
}
