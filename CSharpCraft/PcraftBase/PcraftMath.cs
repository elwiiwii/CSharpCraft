namespace CSharpCraft.PcraftBase;

internal static class PcraftMath
{
    internal static F32 GetLen(F32 x, F32 y)
    {
        return F32.Sqrt((x * x) + (y * y) + F32.FromFloat(0.001f));
    }

    internal static F32 GetInvLen(F32 x, F32 y)
    {
        return F32.One / PcraftServices.GetLen(x, y);
    }

    internal static F32 GetRot(F32 dx, F32 dy)
    {
        return dy >= F32.Zero
            ? (dx + F32.FromInt(3)) * F32.FromFloat(0.25f)
            : (F32.One - dx) * F32.FromFloat(0.25f);
    }

    internal static F32 UpRot(F32 grot, F32 rot)
    {
        if (F32.Abs(rot - grot) > F32.FromFloat(0.5f))
        {
            if (rot > grot)
                grot += F32.One;
            else
                grot -= F32.One;
        }
        return ((F32.Lerp(rot, grot, F32.FromFloat(0.4f)) % F32.One) + F32.One) % F32.One;
    }

    internal static (int flipX, int flipY) Mirror(F32 rot)
    {
        if (rot < F32.FromFloat(0.125f)) return (0, 1);
        if (rot < F32.FromFloat(0.325f)) return (0, 0);
        if (rot < F32.FromFloat(0.625f)) return (1, 0);
        if (rot < F32.FromFloat(0.825f)) return (1, 1);
        return (0, 1);
    }
}
