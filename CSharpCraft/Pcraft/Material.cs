using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Pcraft
{

    public class Material
    {
        public bool BeCraft { get; set; }
        public int? BigSpr { get; set; }
        public bool Drop { get; set; }
        public int? GiveLife { get; set; }
        public string? Name { get; set; }
        public int[]? Pal { get; set; }
        public List<Entity>? Sl { get; set; }
        public int Spr { get; set; }
    }

}
