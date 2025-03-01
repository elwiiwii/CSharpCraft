using CSharpCraft.OptionsMenu;
using System.IO.Pipelines;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;


namespace CSharpCraft
{
    public class ExitScene : IScene, IDisposable
    {
        public string SceneName { get => "exit"; }

        public void Init(Pico8Functions pico8)
        {
            Environment.Exit(0);
        }

        public void Update()
        {

        }

        public void Draw()
        {
            
        }

        public string SpriteData => @"";
        public string FlagData => @"";
        public string MapData => @"";

        public void Dispose()
        {

        }

    }
}
