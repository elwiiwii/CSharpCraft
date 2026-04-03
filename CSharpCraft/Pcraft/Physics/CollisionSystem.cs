namespace CSharpCraft.Pcraft.Physics;

using CSharpCraft.Pcraft.Data;

internal static class CollisionSystem
{
    internal static bool EntColFree(F32 x, F32 y, Entity e)
    {
        var ax = F32.Abs(e.X - x);
        var ay = F32.Abs(e.Y - y);
        return F32.Max(ax, ay) > F32.FromInt(8);
    }

    internal static (F32 dx, F32 dy) ReflectCol(
        F32 x, F32 y,
        F32 dx, F32 dy,
        Func<F32, F32, bool> check,
        F32 dp,
        Entity? e = null)
    {
        var newx = x + dx;
        var newy = y + dy;

        bool ccur   = check(x,    y);
        bool ctotal = check(newx, newy);
        bool chor   = check(newx, y);
        bool cver   = check(x,    newy);

        if (ccur)
        {
            if (chor || cver)
            {
                if (!ctotal)
                {
                    if (chor)
                        dy = -dy * dp;
                    else
                        dx = -dx * dp;
                }
            }
            else
            {
                dx = -dx * dp;
                dy = -dy * dp;
            }
        }

        return (dx, dy);
    }

    internal static bool IsIn(Entity e, F32 size, F32 clx, F32 cly)
        => e.X > clx - size && e.X < clx + size
        && e.Y > cly - size && e.Y < cly + size;
}
