using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft
{
    
    public class Level
    {
        public double[]? Dat { get; set; }
        public List<Entity>? Ene { get; set; }
        public List<Entity>? Ent { get; set; }
        public bool Isunder { get; set; }
        public double Stx { get; set; }
        public double Sty { get; set; }
        public double Sx { get; set; }
        public double Sy { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

}
