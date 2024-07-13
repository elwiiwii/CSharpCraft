using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft
{

    public class Ground
    {
        public double Gr { get; set; }
        public double Id { get; set; }
        public bool Istree { get; set; }
        public double Life { get; set; }
        public Material? Mat { get; set; }
        public int[]? Pal { get; set; }
        public Ground? Tile { get; set; }
    }

}
