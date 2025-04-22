using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.RaceMode
{
    public class Menu
    {
        public string Name { get; set; }
        public List<Item> Items { get; set; }
        public int Sel { get; set; }
        public int Off { get; set; }
        public int Xpos { get; set; }
        public int Ypos { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Active { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public Func<Task> Method { get; set; }
    }
}
