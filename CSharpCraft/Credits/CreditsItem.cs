using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft.Credits
{
    public class CreditsItem
    {
        public string Name = "";
        public List<string> Description = [];
        public List<(string type, string link)> Links = [];
    }
}
