using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.RaceMode
{
    public class SeedType
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Xpos { get; set; }
        public int Ypos { get; set; }
        public bool Selected { get; set; } = false;
        public bool Unavailable { get; set; } = false;
    }
}
