using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Pcraft
{

    public class Ground
    {
        public int Gr { get; set; }
        public int Id { get; set; }
        public bool IsTree { get; set; }
        public int Life { get; set; }
        public Material? Mat { get; set; }
        public int[]? Pal { get; set; }
        public Ground? Tile { get; set; }
    }

}
