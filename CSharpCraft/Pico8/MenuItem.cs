using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Pico8
{
    public class MenuItem
    {
        public Func<string> GetName { get; }
        public Action Function { get; }

        public MenuItem(Func<string> getName, Action function)
        {
            GetName = getName;
            Function = function;
        }
    }
}
