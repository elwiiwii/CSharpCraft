﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CSharpCraft
{
    public class Level
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Sx { get; set; }
        public double Sy { get; set; }
        public bool Isunder { get; set; }
        public double Stx { get; set; }
        public double Sty { get; set; }
    }

    class FNAGame : Game
    {
        [STAThread]
        static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            using FNAGame g = new();
            g.Run();
        }

#nullable enable
        private SpriteBatch batch;
        private Texture2D pixel;
        private Texture2D SpriteSheet1;
        private Pico8Functions pico8Functions;
        private readonly GraphicsDeviceManager graphics;
        private double frameRate = 0.0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        private double time;

        public double plx;
        public double ply;
        public double prot;
        public double lrot;
        public double panim;
        public double banim;

        public Level currentlevel;

        public bool levelunder = false;
        public double levelsx;
        public double levelsy;
        public double levelx;
        public double levely;
        public double holex;
        public double holey;
        public double clx;
        public double cly;
        public double cmx;
        public double cmy;

        public double[][] level;
        readonly int[] typecount = new int[11];

        private readonly Level currentLevel;

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

            //plx = 64.0;
            //ply = 64.0;
            prot = 0.0;
            lrot = 0.0;
            panim = 0.0;
            banim = 0.0;

            Createlevel(0,0,64,64,false);
        }

        public void Spr8(int spriteNumber, int x, int y)
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

                    pico8Functions.Spr(currentSpriteNumber, spriteX, spriteY);
                }
            }
        }

        public Level Createlevel(double xx, double yy, double sizex, double sizey, bool isUnderground)
        {
            int xxFlr = (int)Math.Floor(xx);
            int yyFlr = (int)Math.Floor(yy);
            int sizexFlr = (int)Math.Floor(sizex);
            int sizeyFlr = (int)Math.Floor(sizey);

            var l = new Level
            {
                X = xxFlr,
                Y = yyFlr,
                Sx = sizexFlr,
                Sy = sizeyFlr,
                Isunder = isUnderground
            };

            Setlevel(l);
            Createmap();
            l.Stx = (holex - levelx) * 16 + 8;
            l.Sty = (holey - levely) * 16 + 8;

            return l;
        }

        public void Setlevel(Level l)
        {
            currentlevel = l;
            levelx = l.X;
            levely = l.Y;
            levelsx = l.Sx;
            levelsy = l.Sy;
            levelunder = l.Isunder;
            plx = l.Stx;
            ply = l.Sty;
        }

        public double Lerp(double a, double b, double alpha)
        {
            return a * (1.0 - alpha) + b * alpha;
        }

        public double Getlen(double x, double y)
        {
            return Math.Sqrt(x * x + y * y + 0.001);
        }

        public double Getinvlen(double x, double y)
        {
            return 1 / Getlen(x, y);
        }

        public double Getrot(double dx, double dy)
        {
            return dy >= 0 ? (dx + 3) * 0.25 : (1 - dx) * 0.25;
        }
        
        public (int, int) Mirror(double rot)
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

        public (double, double) Reflectcol(double dx, double dy, double dp)
        {
            dx = -dx * dp;
            dy = -dy * dp;

            return (dx, dy);
        }

        public double Uprot(double grot, double rot)
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
            return (Lerp(rot, grot, 0.4) % 1 + 1) % 1;
        }

        public void Dplayer(double x, double y, double rot, double anim, double subanim)
        {
            rot = -rot * 2 * Math.PI;

            var cr = Math.Cos(rot);
            var sr = Math.Sin(rot);
            var cv = -sr;
            var sv = cr;

            x = Math.Floor(x);
            y = Math.Floor(y - 4);

            var lan = Math.Sin(anim * 2) * 1.5;

            pico8Functions.Circfill(x + cv * 2 - cr * lan, y + 3 + sv * 2 - sr * lan, 3, 1);
            pico8Functions.Circfill(x - cv * 2 + cr * lan, y + 3 - sv * 2 + sr * lan, 3, 1);

            var blade = (rot + 0.25) % 1;

            if (subanim > 0)
            {
                blade = blade - 0.3 + subanim * 0.04;
            }

            var bcr = Math.Cos(blade);
            var bsr = Math.Sin(blade);

            //var mx = mirror(blade);
            //var my = mirror(blade);

            (int mx, int my) = Mirror(blade);

            var weap = 75;

            //pico8Functions.spr(weap, x + bcr * 4 - cr * lan - mx * 8 + 1, y + bsr * 4 - sr * lan + my * 8 - 7, 1, 1, mx == 1, my == 1);

            pico8Functions.Circfill(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 2);
            pico8Functions.Circfill(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 2);

            //var mx2 = mirror((rot + 0.75) % 1);
            //var my2 = mirror((rot + 0.75) % 1);

            (int mx2, int my2) = Mirror((rot + 0.75) % 1);

            //pico8Functions.spr(75, x + cv * 4 + cr * lan - 8 + mx2 * 8 + 1, y + sv * 4 + sr * lan + my2 * 8 - 7, 1, 1, mx2 == 0, my2 == 1);

            pico8Functions.Circfill(x + cr + 0.001, y + sr - 2 + 0.001, 4, 2);
            pico8Functions.Circfill(x + cr + 0.001, y + sr + 0.001, 4, 2);
            pico8Functions.Circfill(x + cr * 1.5, y + sr * 1.5 - 2, 2.5, 15);
            pico8Functions.Circfill(x - cr + 0.001, y - sr - 3, 3, 4);

        }

        public double[][] Noise(double sx, double sy, double startscale, double scalemod, double featstep)
        {
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);
            int featstepFlr = (int)Math.Floor(featstep);

            double[][] n = new double[sxFlr + 1][];

            for (int i = 0; i <= sxFlr; i++)
            {
                n[i] = new double[syFlr + 1];
                for (int j = 0; j <= syFlr; j++)
                {
                    n[i][j] = 0.5;
                }
            }

            var step = sxFlr;
            var scale = startscale;

            while (step > 1)
            {
                var cscal = scale;
                if (step == featstepFlr) { cscal = 1; }

                for (int i = 0; i <= sxFlr - 1; i += step)
                {
                    for (int j = 0; j <= syFlr - 1; j += step)
                    {
                        var c1 = n[i][j];
                        var c2 = n[i + step][j];
                        var c3 = n[i][j + step];
                        n[i + step / 2][j] = (c1 + c2) * 0.5 + (new Random().NextDouble() - 0.5) * cscal;
                        n[i][j + step / 2] = (c1 + c3) * 0.5 + (new Random().NextDouble() - 0.5) * cscal;
                    }
                }

                for (int i = 0; i <= sxFlr - 1; i += step)
                {
                    for (int j = 0; j <= syFlr - 1; j += step)
                    {
                        var c1 = n[i][j];
                        var c2 = n[i + step][j];
                        var c3 = n[i][j + step];
                        var c4 = n[i + step][j + step];
                        n[i + step / 2][j + step / 2] = (c1 + c2 + c3 + c4) * 0.25 + (new Random().NextDouble() - 0.5) * cscal;
                    }
                }

                step /= 2;
                scale *= scalemod;
            }

            return n;
        }

        public double[][] Createmapstep(double sx, double sy, double a, double b, double c, double d, double e)
        {
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);

            var cur = Noise(sxFlr, syFlr, 0.9, 0.2, sxFlr);
            var cur2 = Noise(sxFlr, syFlr, 0.9, 0.4, 8);
            var cur3 = Noise(sxFlr, syFlr, 0.9, 0.3, 8);
            var cur4 = Noise(sxFlr, syFlr, 0.8, 1.1, 4);

            for (int i = 0; i < 11; i++)
            {
                typecount[i] = 0;
            }

            for (int i = 0; i <= sxFlr; i++)
            {
                for (int j = 0; j <= syFlr; j++)
                {
                    var v = Math.Abs(cur[i][j] - cur2[i][j]);
                    var v2 = Math.Abs(cur[i][j] - cur3[i][j]);
                    var v3 = Math.Abs(cur[i][j] - cur4[i][j]);
                    var dist = Math.Max(Math.Abs((double)i / sxFlr - 0.5) * 2, Math.Abs((double)j / syFlr - 0.5) * 2);
                    dist = dist * dist * dist * dist;
                    //Math.Pow(dist, 5);
                    var coast = v * 4 - dist * 4;

                    var id = a;
                    if (coast > 0.3) { id = b; } // sand
                    if (coast > 0.6) { id = c; } // grass
                    if (coast > 0.3 && v2 > 0.5) { id = d; } // stone
                    if (id == c && v3 > 0.5) { id = e; } // tree

                    typecount[(int)id] += 1;

                    cur[i][j] = id;
                }
            }

            return cur;
        }

        public void Createmap()
        {
            var needmap = true;

            while (needmap)
            {
                needmap = false;

                if (levelunder)
                {
                    level = Createmapstep(levelsx, levelsy, 3, 8, 1, 9, 10);

                    if (typecount[8] < 30) { needmap = true; }
                    if (typecount[9] < 20) { needmap = true; }
                    if (typecount[10] < 15) { needmap = true; }
                }
                else
                {
                    level = Createmapstep(levelsx, levelsy, 0, 1, 2, 3, 4);

                    if (typecount[3] < 30) { needmap = true; }
                    if (typecount[4] < 30) { needmap = true; }

                    //if (typecount[0] < 100) { needmap = true; }
                }

                if (!needmap)
                {
                    plx = -1;
                    ply = -1;

                    for (int i = 0; i <= 500; i++)
                    {
                        var depx = (int)Math.Floor(levelsx / 8 + new Random().NextDouble() * levelsx * 6 / 8);
                        var depy = (int)Math.Floor(levelsy / 8 + new Random().NextDouble() * levelsy * 6 / 8);
                        var c = level[depx][depy];

                        if (c == 1 || c == 2)
                        {
                            plx = depx * 16 + 8;
                            ply = depy * 16 + 8;
                            break;
                        }
                    }

                    if (plx < 0) 
                    {
                        needmap = true; 
                    }
                }
            }

            for (int i = 0; i < levelsx; i++)
            {
                for (int j = 0; j < levelsy; j++)
                {
                    pico8Functions.Mset(i + levelx, j + levely, level[i][j]);
                }
            }

            holex = levelsx / 2 + levelx;
            holey = levelsy / 2 + levely;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    pico8Functions.Mset(holex + i, holey + j, levelunder ? 1 : 3);
                }
            }

            pico8Functions.Mset(holex, holey, 11);

            clx = plx;
            cly = ply;

            cmx = plx;
            cmy = ply;
        }

        public void Printc(string t, int x, int y, int c)
        {
            pico8Functions.Print(t, x - (t.Length * 2), y, c);
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

            if (state.IsKeyDown(Keys.Tab)) Createlevel(0, 0, 64, 64, false);

            double dx = 0.0;
            double dy = 0.0;

            if (state.IsKeyDown(Keys.A)) dx -= 1.0;
            if (state.IsKeyDown(Keys.D)) dx += 1.0;
            if (state.IsKeyDown(Keys.W)) dy -= 1.0;
            if (state.IsKeyDown(Keys.S)) dy += 1.0;

            double dl = Getinvlen(dx, dy);

            dx *= dl;
            dy *= dl;

            if (Math.Abs(dx) > 0 || Math.Abs(dy) > 0)
            {
                lrot = Getrot(dx, dy);
                panim += 1.0 / 5.5;
            }
            else
            {
                panim = 0;
            }

            var s = 1.0;

            dx *= s;
            dy *= s;

            //dx = reflectcol(dx, dy, -1).Item1;
            //dy = reflectcol(dx, dy, -1).Item2;

            (dx, dy) = Reflectcol(dx, dy, 0);

            plx += dx;
            ply += dy;

            prot = Uprot(lrot, prot);

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

            pico8Functions.Rectfill(0, 0, 128, 128, 7);

            /*
            pico8Functions.rectfill(0, 0, 128, 46, 12);
            pico8Functions.rectfill(0, 46, 128, 128, 1);
            spr8(16, 32, 14);
            printc("by nusan", 64, 80, 6);
            printc("2016", 64, 90, 6);
            printc("press button 1", 64, 112, (int)(6 + time % 2));
            */

            /*
            dplayer(44, 44, 0.375, panim, banim);
            dplayer(64, 44, 0.25, panim, banim);
            dplayer(84, 44, 0.125, panim, banim);

            dplayer(44, 64, 0.5, panim, banim);
            dplayer(plx, ply, prot, panim, banim);
            dplayer(84, 64, 0, panim, banim);

            dplayer(44, 84, 0.625, panim, banim);
            dplayer(64, 84, 0.75, panim, banim);
            dplayer(84, 84, 0.875, panim, banim);
            */

            if (false)
            {
                pico8Functions.Rectfill(31, 31, 65, 65, 8);
                pico8Functions.Rectfill(32, 32, 64, 64, 7);
                for (int i = 0; i <= 31; i++)
                {
                    for (int j = 0; j <= 31; j++)
                    {
                        var c = pico8Functions.Mget(i + 64, j);
                        pico8Functions.Pset(i + 32, j + 32, c);
                    }
                }
            }
            else
            {
                pico8Functions.Rectfill(31, 31, 97, 97, 8);
                pico8Functions.Rectfill(32, 32, 96, 96, 7);
                for (int i = 0; i <= 63; i++)
                {
                    for (int j = 0; j <= 63; j++)
                    {
                        var c = pico8Functions.Mget(i, j);
                        pico8Functions.Pset(i + 32, j + 32, c);
                    }
                }
            }

            //pico8Functions.mset(1, 1, 8);
            //var ec = pico8Functions.mget(1, 1);
            //pico8Functions.pset(1, 1, ec);

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