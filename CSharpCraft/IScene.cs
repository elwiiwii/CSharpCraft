using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCraft.Pico8;

namespace CSharpCraft
{
    public interface IScene
    {
        string SceneName { get; }
        void Init(Pico8Functions p8);
        void Update();
        void Draw();
        string SpriteImage { get; }
        string SpriteData { get; }
        string FlagData { get; }
        string MapData { get; }
        Dictionary<string, List<(List<(string name, bool loop)> tracks, int group)>> Music { get; }
        Dictionary<string, Dictionary<int, string>> Sfx { get; }
        void Dispose();
    }
}
