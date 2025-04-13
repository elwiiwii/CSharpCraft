using FixMath;

namespace CSharpCraft
{
    public class DataItem
    {
        public F32 Val { get; set; } = F32.Zero;
        public int[] Hits { get; set; } = new int[4];
    }
}
