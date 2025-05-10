using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu
{
    public class BackOptions1 : IScene, IDisposable
    {

        public string SceneName { get => "options"; }
        private Pico8Functions p8;

        KeyboardOptions drawScene = new(-2);

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;

            drawScene.Init(p8);
        }

        public void Update()
        {
            if (p8.Btnp(3)) { p8.LoadCart(new ControlsOptions()); return; }
            if (p8.Btnp(4) || p8.Btnp(5)) { p8.LoadCart(new TitleScreen(false)); return; }
        }

        public void Draw()
        {
            p8.Cls();

            // Get the size of the viewport
            int viewportWidth = p8.GraphicsDevice.Viewport.Width;
            int viewportHeight = p8.GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            drawScene.Draw();
            
            p8.Batch.Draw(p8.TextureDictionary["OptionsBackground0"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        }

        public virtual string SpriteImage => "";
        public string SpriteData => @"";
        public string FlagData => @"";
        public string MapData => @"";
        public Dictionary<string, List<(List<(string name, bool loop)> tracks, int group)>> Music => new();
        public Dictionary<string, Dictionary<int, string>> Sfx => new();
        public void Dispose()
        {
            
        }

    }
}
