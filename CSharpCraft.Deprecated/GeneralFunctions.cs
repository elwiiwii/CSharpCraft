namespace CSharpCraft;

public static class GeneralFunctions
{
    public static int Loop<T>(int sel, List<T> l)
    {
        int lp = l.Count;
        return ((sel % lp) + lp) % lp;
    }

    public static int Loop(int sel, int count)
    {
        return ((sel % count) + count) % count;
    }
}
