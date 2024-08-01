using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class MainOptions(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile, List<IGameMode> optionsModes) : IGameMode
    {

        public string GameModeName { get => "options"; }

        public int currentOptionsMode;

        public void Init()
        {
            currentOptionsMode = 2;
        }

        public void Update()
        {

        }

        public void Draw()
        {
            
        }

    }

}
