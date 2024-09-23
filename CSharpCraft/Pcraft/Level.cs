using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Pcraft
{

    public class Level
    {
        public double[]? Dat { get; set; }
        public List<Entity>? Ene { get; set; }
        public List<Entity>? Ent { get; set; }
        public bool IsUnder { get; set; }
        public double Stx { get; set; }
        public double Sty { get; set; }
        public int Sx { get; set; }
        public int Sy { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

}
