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
        private double time = 0;
        private GraphicsDeviceManager graphics;
        private double frameRate = 0.0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        public double plx;
        public double ply;
        public double prot;
        public double lrot;
        public double panim;
        public double banim;

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
            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30.0);

            // Decouple the frame rate from the monitor's refresh rate
            graphics.SynchronizeWithVerticalRetrace = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            UpdateViewport();

            plx = 64.0;
            ply = 64.0;
            prot = 0.0;
            lrot = 0.0;
            panim = 0.0;
            banim = 0.0;
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

        public double lerp(double a, double b, double alpha)
        {
            return a * (1.0 - alpha) + b * alpha;
        }

        public double getlen(double x, double y)
        {
            return Math.Sqrt(x * x + y * y + 0.001);
        }

        public double getinvlen(double x, double y)
        {
            return 1 / getlen(x, y);
        }

        public double getrot(double dx, double dy)
        {
            return dy >= 0 ? (dx + 3) * 0.25 : (1 - dx) * 0.25;
        }

        public (int, int) mirror(double rot)
        {
            if (rot < 0.125)
            {
                return (0, 1);
            }
            else if (rot < 0.325)
            {
                return (0, 0);
            }
            else if (rot < 0.625)
            {
                return (1, 0);
            }
            else if (rot < 0.825)
            {
                return (1, 1);
            }
            else
            {
                return (0, 1);
            }
        }

        public (double, double) reflectcol(double dx, double dy, double dp)
        {
            dx = -dx * dp;
            dy = -dy * dp;

            return (dx, dy);
        }

        public double uprot(double grot, double rot)
        {
            if (Math.Abs(rot - grot) > 0.5)
            {
                if (rot > grot)
                {
                    grot += 1;
                }
                else
                {
                    grot -= 1;
                }
            }
            return (lerp(rot, grot, 0.4) % 1 + 1) % 1;
        }

        public void dplayer(double x, double y, double rot, double anim, double subanim)
        {
            var cr = Math.Cos(rot);
            var sr = Math.Sin(rot);
            var cv = -sr;
            var sv = cr;

            x = (int)x;
            y = (int)(y - 4);

            var lan = Math.Sin(anim * 2) * 1.5;

            pico8Functions.circfill(x + cv * 2 - cr * lan, y + 3 + sv * 2 - sr * lan, 3, 1);
            pico8Functions.circfill(x - cv * 2 + cr * lan, y + 3 - sv * 2 + sr * lan, 3, 1);

            var blade = (rot + 0.25) % 1;

            if (subanim > 0)
            {
                blade = blade - 0.3 + subanim * 0.04;
            }

            var bcr = Math.Cos(blade);
            var bsr = Math.Sin(blade);

            var mx = mirror(blade);
            var my = mirror(blade);

            var weap = 75;

            pico8Functions.spr(weap, x + bcr * 4 - cr * lan - mx.Item1 * 8 + 1, y + bsr * 4 - sr * lan + my.Item1 * 8 - 7, 1, 1, mx.Item2 == 1, my.Item2 == 1);

            pico8Functions.circfill(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 2);
            pico8Functions.circfill(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 2);

            var mx2 = mirror((rot + 0.75) % 1);
            var my2 = mirror((rot + 0.75) % 1);

            pico8Functions.spr(75, x + cv * 4 + cr * lan - 8 + mx2.Item1 * 8 + 1, y + sv * 4 + sr * lan + my2.Item1 * 8 - 7, 1, 1, mx2.Item2 == 0, my2.Item2 == 1);

            pico8Functions.circfill(x + cr, y + sr - 2, 4, 2);
            pico8Functions.circfill(x + cr, y + sr, 4, 2);
            pico8Functions.circfill(x + cr * 1.5, y + sr * 1.5 - 2, 2.5, 15);
            pico8Functions.circfill(x - cr, y - sr - 3, 3, 4);

        }

        public void printc(string t, int x, int y, int c)
        {
            pico8Functions.print(t, x - (t.Length * 2), y, c);
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

            var dx = 0.0;
            var dy = 0.0;

            if (state.IsKeyDown(Keys.A))
            {
                dx -= 1;
            }
            if (state.IsKeyDown(Keys.D))
            {
                dx += 1;
            }
            if (state.IsKeyDown(Keys.W))
            {
                dy -= 1;
            }
            if (state.IsKeyDown(Keys.S))
            {
                dy += 1;
            }

            var dl = getinvlen(dx, dy);

            dx *= dl;
            dy *= dl;

            if (Math.Abs(dx) > 0 || Math.Abs(dy) > 0)
            {
                lrot = getrot(dx, dy);
                panim += 1.0 / 33.0;
            }
            else
            {
                panim = 0;
            }

            dx *= 2;
            dy *= 2;

            dx = reflectcol(dx, dy, -1).Item1;
            dy = reflectcol(dx, dy, -1).Item2;

            plx += dx;
            ply += dy;
            prot = uprot(lrot, prot);

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

            pico8Functions.rectfill(0, 0, 128, 128, 7);

            /*
            pico8Functions.rectfill(0, 0, 128, 46, 12);
            pico8Functions.rectfill(0, 46, 128, 128, 1);
            spr8(16, 32, 14);
            printc("by nusan", 64, 80, 6);
            printc("2016", 64, 90, 6);
            printc("press button 1", 64, 112, (int)(6 + time % 2));
            */

            dplayer(plx, ply, prot, panim, banim);

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