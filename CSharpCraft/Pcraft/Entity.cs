using FixMath;

namespace CSharpCraft.Pcraft
{

    public class Entity
    {
        public F32 Banim { get; set; }
        public int C { get; set; }
        public int? Count { get; set; }
        public F32 Dtim { get; set; }
        public F32 Dx { get; set; }
        public F32 Dy { get; set; }
        public Material? GiveItem { get; set; }
        public bool HasCol { get; set; }
        public F32 Life { get; set; }
        public List<Entity>? List { get; set; }
        public F32 Lrot { get; set; }
        public int Off { get; set; }
        public F32 Ox { get; set; }
        public F32 Oy { get; set; }
        public F32 Panim { get; set; }
        public int? Power { get; set; }
        public F32 Prot { get; set; }
        public List<Entity>? Req { get; set; }
        public int Sel { get; set; }
        public int? Spr { get; set; }
        public int Step { get; set; }
        public string? Text { get; set; }
        public string? Text2 { get; set; }
        public F32? Timer { get; set; }
        public Material? Type { get; set; }
        public F32 Vx { get; set; }
        public F32 Vy { get; set; }
        public F32 X { get; set; }
        public F32 Y { get; set; }
        public Random? PosRnd { get; set; } = null;
        public Random? TimRnd { get; set; } = null;
    }
}
