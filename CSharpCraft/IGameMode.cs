using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCraft.Pico8;

namespace CSharpCraft
{
    public interface IGameMode
    {
        string GameModeName { get; }
        void Init(Pico8Functions p8);
        void Update();
        void Draw();
        string SpriteData { get; }
        string FlagData { get; }
        string MapData { get; }
        void Dispose();
    }
}
