using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft
{
    public interface IGameMode
    {
        string GameModeName { get; }
        void Init();
        void Update();
        void Draw();
        string Sprites { get; }
        string Flags { get; }
        string Map { get; }
    }
}
