﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class ControllerOptions(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile, List<IGameMode> optionsModes, MainOptions mainOptions) : IGameMode
    {

        public string GameModeName { get => "5"; }

        private int menuX;
        private int menuY;
        private int menuWidth;
        private int menuLength;
        private bool waitingForInput;
        private int delay;

        private int Loop(int sel, int size)
        {
            return ((sel % size) + size) % size;
        }

        public void Init()
        {
            menuX = 0;
            menuY = -1;
            menuWidth = 2;
            menuLength = typeof(KeyboardOptionsFile).GetProperties().Length;
            waitingForInput = false;
            delay = 0;
            mainOptions.currentOptionsMode = 5;
        }

        public void Update()
        {
            if (p8.Btnp(0)) { menuX -= 1; }
            if (p8.Btnp(1)) { menuX += 1; }
            if (p8.Btnp(2)) { menuY -= 1; }
            if (p8.Btnp(3)) { menuY += 1; }

            if (menuY == -1)
            {
                if (p8.Btnp(0)) { optionsModes[4].Init(); return; }
                if (p8.Btnp(2)) { optionsModes[2].Init(); return; }
                if (p8.Btnp(3)) { menuY += 1; }
                return;
            }

            menuX = Loop(menuX, menuWidth);
            menuY = Loop(menuY, menuLength);

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

            batch.Draw(textureDictionary["OptionsBackground4"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        }

    }

}
