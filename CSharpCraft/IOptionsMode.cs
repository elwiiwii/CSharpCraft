using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft
{
    public interface IOptionsMode
    {
        int Current { get; set; }
        void Init();
        void Update();
        void Draw();
    }
}
