using FixMath;

namespace CSharpCraft.Pcraft
{

    public class Level
    {
        public F32[]? Dat { get; set; }
        public DataItem[]? DatIt { get; set; }
        public List<Entity>? Ene { get; set; }
        public List<Entity>? Ent { get; set; }
        public bool IsUnder { get; set; }
        public F32 Stx { get; set; }
        public F32 Sty { get; set; }
        public int Sx { get; set; }
        public int Sy { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

}
