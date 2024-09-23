using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu
{
    public class BackOptions1(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile, List<IGameMode> optionsModes, MainOptions mainOptions, TitleScreen titleScreen) : IGameMode
    {

        public string GameModeName { get => "0"; }

        public void Init()
        {
            mainOptions.currentOptionsMode = 0;
        }

        public void Update()
        {
            if (p8.Btnp(3)) { optionsModes[2].Init(); return; }
            if (p8.Btnp(4) || p8.Btnp(5)) { titleScreen.currentGameMode = 0; return; }
        }

        public void Draw()
        {
            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            batch.Draw(textureDictionary["OptionsBackground0"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        }

    }

}
