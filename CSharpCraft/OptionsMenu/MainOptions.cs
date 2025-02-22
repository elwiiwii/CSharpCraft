﻿using CSharpCraft.Pico8;
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
    public class MainOptions(List<IGameMode> optionsModes) : IGameMode
    {
        //public MainOptions(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile, List<IGameMode> optionsModes)
        //{

        //}
        public string GameModeName { get => "options"; }
        private Pico8Functions p8;

        public int currentOptionsMode;

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;

            currentOptionsMode = 2;
            optionsModes[currentOptionsMode].Init(p8);
        }

        public void Update()
        {
            currentOptionsMode = int.Parse(optionsModes[currentOptionsMode].GameModeName);
            optionsModes[currentOptionsMode].Update();
        }

        public void Draw()
        {
            optionsModes[currentOptionsMode].Draw();
        }

        public string SpriteData => @"";
        public string FlagData => @"";
        public string MapData => @"";

    }

}
