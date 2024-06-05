using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft
{
    class FNAGame : Game
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (FNAGame g = new FNAGame())
            {
                g.Run();
            }
        }

#nullable enable
        private SpriteBatch batch;
        private Texture2D pixel;
        private Texture2D SpriteSheet1;
        private Pico8Functions pico8Functions;
        private int MoveSpriteX = 0;
        private int MoveSpriteY = 0;
        private double time = 0;
        private GraphicsDeviceManager graphics;
        private double frameRate = 0.0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

#nullable disable

        private FNAGame()
        {
            graphics = new GraphicsDeviceManager(this);

            // Allow the user to resize the window
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            // All graphics loaded will be in a "Graphics" folder
            Content.RootDirectory = "Graphics";

            // Set fixed time step to true
            this.IsFixedTimeStep = true;

            // Set the target elapsed time to 1/30th of a second (30 frames per second)
            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0/ 30.0);

            // Decouple the frame rate from the monitor's refresh rate
            graphics.SynchronizeWithVerticalRetrace = true;
        }

        public void spr8(int spriteNumber, int x, int y)
        {
            // Calculate the top left corner of the 4x4 block
            int startRow = spriteNumber / 16 * 8;
            int startCol = spriteNumber % 16 * 8;

            // Draw each 8x8 sprite in the 4x4 block
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // Calculate the sprite number for the current 8x8 sprite
                    int currentSpriteNumber = (startRow + row) * 16 + (startCol + col);

                    // Calculate the position of the sprite
                    int spriteX = x + col * 8;
                    int spriteY = y + row * 8;

                    pico8Functions.spr(currentSpriteNumber, spriteX, spriteY);
                }
            }
        }

        public void printc(string t, int x, int y, int c)
        {
            pico8Functions.print(t, x - (t.Length * 2), y, c);
        }

        protected override void Initialize()
        {
            base.Initialize();

            UpdateViewport();
        }

        protected override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                frameRate = frameCounter / elapsedTime.TotalSeconds;
                elapsedTime = TimeSpan.Zero;
                frameCounter = 0;
                Console.WriteLine("FPS: " + frameRate); // Print the frame rate to the console
            }

            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.W))
            {
                MoveSpriteY -= 1;
            }
            if (state.IsKeyDown(Keys.S))
            {
                MoveSpriteY += 1;
            }
            if (state.IsKeyDown(Keys.A))
            {
                MoveSpriteX -= 1;
            }
            if (state.IsKeyDown(Keys.D))
            {
                MoveSpriteX += 1;
            }

            time += 0.1;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;

            GraphicsDevice.Clear(Color.Black);

            batch.Begin();

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            //batch.Draw(texture, Vector2.Zero, Color.White);
            //batch.Draw(font, Vector2.Zero, Color.White);
            //print("Hello, world!", new Vector2(10, 10), Color.White);

            //pico8Functions.print("abcdefghijklmnopqrstuvwxyz", 1, 1, 14);

            //pico8Functions.circfill(12, 12, 5, 8);

            //pico8Functions.spr(90, 1, 1);

            pico8Functions.rectfill(0, 0, 128, 46, 12);
            pico8Functions.rectfill(0, 46, 128, 128, 1);
            spr8(16, 32, 14);
            printc("by nusan", 64, 80, 6);
            printc("2016", 64, 90, 6);
            printc("press button 1", 64, 112, (int)(6 + time % 2));

            // Draw the grid
            /*for (int i = 0; i <= 128; i++)
            {
                // Draw vertical lines
                batch.DrawLine(pixel, new Vector2(i * cellWidth, 0), new Vector2(i * cellWidth, viewportHeight), Color.White, 1);
                // Draw horizontal lines
                batch.DrawLine(pixel, new Vector2(0, i * cellHeight), new Vector2(viewportWidth, i * cellHeight), Color.White, 1);
            }*/

            batch.End();
    
            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            // Create the batch...
            batch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white pixel texture for drawing lines
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // ... then load a texture from ./Graphics/FNATexture.png
            SpriteSheet1 = Content.Load<Texture2D>("SpriteSheet1");

            pico8Functions = new Pico8Functions(pixel, batch, GraphicsDevice);

        }

        protected override void UnloadContent()
        {
            batch.Dispose();
            SpriteSheet1.Dispose();
            pixel.Dispose();
            pico8Functions.Dispose();
        }

        private void UpdateViewport()
        {
            // Calculate the size of the largest square that fits in the client area
            int maxSize = Math.Min(Window.ClientBounds.Width, Window.ClientBounds.Height);

            // Round the size down to the nearest multiple of 128
            int size = (maxSize / 128) * 128;

            // Calculate the exact center of the client area
            double centerX = Window.ClientBounds.Width / 2.0f;
            double centerY = Window.ClientBounds.Height / 2.0f;

            // Calculate the top left corner of the square so that its center aligns with the client area's center
            int left = (int)Math.Round(centerX - size / 2.0f);
            int top = (int)Math.Round(centerY - size / 2.0f);

            // Set the viewport to the square area
            GraphicsDevice.Viewport = new Viewport(left, top, size, size);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewport();
        }


    }
}