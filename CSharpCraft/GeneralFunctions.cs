namespace CSharpCraft
{
    public static class GeneralFunctions
    {
        public static int Loop<T>(int sel, List<T> l)
        {
            int lp = l.Count;
            return ((sel % lp) + lp) % lp;
        }
    }
}
