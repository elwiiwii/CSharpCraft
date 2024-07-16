using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Transactions;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class Options(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, OptionsFile optionsFile) : IGameMode
    {

        public string GameModeName { get => "options"; }

        List<FieldInfo> keys = [];
        private bool menuSelectedX;
        private int menuSelectedY;
        private int currentMenuOption;
        private int[] menuOptions = new int[4];

        private static string[] keyboardBinds1 = ["left"];
        private static string[] keyboardBinds2 = ["left"];
        private static string[] controllerBinds1 = ["left"];
        private static string[] controllerBinds2 = ["left"];

        private static Dictionary<string, string[]> keyboardControls = new()
        {
            { "binds-1", keyboardBinds1 },
            { "binds-2", keyboardBinds2 }
        };

        private static Dictionary<string, string[]> controllerControls = new()
        {
            { "binds-1", controllerBinds1 },
            { "binds-2", controllerBinds2 }
        };

        private static Dictionary<string, Dictionary<string, string[]>> controlsOptions = new()
        {
            { "keyboard", keyboardControls },
            { "controller", controllerControls }
        };

        private static Dictionary<string, Dictionary<string, Dictionary<string, string[]>>> options = new()
        {
            { "controls", controlsOptions },
            { "graphics", controlsOptions }
        };

        



        private int Loop(int sel)
        {
            return sel < -1 ? -1 : sel > 0 ? 0 : sel;
        }
        
        private void Printc(string t, int x, int y, double c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        public void Init()
        {
            //keys = [];
            //keys = optionsFile.GetAllFields();

            //menuSelectedX = false;
            //menuSelectedY = 0;

            currentMenuOption = 1;
            menuOptions[currentMenuOption] = 0;
        }

        public void Update()
        {
            if (p8.Btnp(0)) { menuOptions[currentMenuOption] += 1; }
            if (p8.Btnp(1)) { menuOptions[currentMenuOption] -= 1; }
            if (p8.Btnp(2)) { currentMenuOption -= 1; }
            if (p8.Btnp(3)) { currentMenuOption += 1; }
            
            menuOptions[currentMenuOption] = Loop(menuOptions[currentMenuOption]);
        }

        public void Draw()
        {
            batch.GraphicsDevice.Clear(Color.Black);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            batch.Draw(textureDictionary["OptionsMenuBackground"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["Arrow"], new Vector2(0 * cellW, 1 * cellH), null, p8.colors[currentMenuOption == 0 ? 7 : 5], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Print("back", 5, 1, currentMenuOption == 0 ? 7 : 5);
            //batch.Draw(textureDictionary["OptionsMenuTab"], new Vector2(15 * cellW, 22 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            //batch.Draw(textureDictionary["SelectorHalf"], new Vector2(16 * cellW, 31 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            //batch.Draw(textureDictionary["SelectorHalf"], new Vector2(31 * cellW, 31 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);

            //p8.Print("controls", 19, 19, 7);
            //p8.Print("graphics", 77, 18, 7);
            //p8.Print("keyboard", 19, 33, 7);
            //p8.Print("controller", 73, 33, 7);

            var position = new Vector2((15 + Math.Abs(58 * menuOptions[1])) * cellW, 22 * cellH);
            batch.Draw(textureDictionary["OptionsMenuTab"], position, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            
            for (int i = 0; i <= 1; i++)
            {
                p8.Print(options.Keys.ElementAt(i), 19 + 58 * i, 19 - Math.Abs(i + menuOptions[1]), 7);
            }

            //if (currentMenuOption == 1)
            //{
            //    if (menuOptions[1] == 0)
            //    {
            //        batch.Draw(textureDictionary["KeybindsMenu"], new Vector2(8 * cellW, 46 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            //        batch.Draw(textureDictionary["SelectorHalf"], new Vector2(16 * cellW, 31 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            //        batch.Draw(textureDictionary["SelectorHalf"], new Vector2(31 * cellW, 31 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
            //
            //        for (int i = 0; i <= 1; i++)
            //        {
            //            p8.Print(controlsOptions.Keys.ElementAt(i), 19 + 54 * i, 33, 7);
            //        }
            //    }
            //}
            
            if (currentMenuOption == 2)
            {
                if (menuOptions[2] == 0)
                {
                    //p8.Rectfill(17, 32, 51, 38, 13);
                }
            }
            if (menuOptions[1] == 0)
            {
                var position2 = new Vector2((16 + Math.Abs(54 * menuOptions[2])) * cellW, 31 * cellH);
                var position3 = new Vector2((30 + Math.Abs(62 * menuOptions[2])) * cellW, 31 * cellH);

                batch.Draw(textureDictionary["SelectorHalf"], position2, null, p8.colors[currentMenuOption == 2 ? 7 : 5], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SelectorHalf"], position3, null, p8.colors[currentMenuOption == 2 ? 7 : 5], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
                batch.Draw(textureDictionary["KeybindsMenu"], new Vector2(8 * cellW, 46 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

                for (int i = 0; i <= 1; i++)
                {
                    p8.Print(controlsOptions.Keys.ElementAt(i), 19 + 54 * i, 33, 7);
                }
            }



            //int i = 0;
            //foreach (var key in keys)
            //{
            //    p8.Print($"{key}", 8, 62 + i, 7);
            //    i += 6;
            //}
        }

    }


}
