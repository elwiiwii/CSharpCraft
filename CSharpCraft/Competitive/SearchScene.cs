using CSharpCraft.OptionsMenu;
using System.IO.Pipelines;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using System;


namespace CSharpCraft.Competitive
{
    public class SearchScene(IScene prevScene) : IScene
    {
        public string SceneName { get => "ranked"; }
        private Pico8Functions p8;
        private Icon back;
        private Icon replays;
        private Icon statistics;
        private Icon search;
        private Icon profile;
        private Icon settings;

        private Icon[] icons;
        private Icon? curIcon;
        private float cursorX;
        private float cursorY;
        private MouseState prevState;

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;
            back = new() { StartPos = (120, 3), EndPos = (126, 11), Label = "back", ShadowTexture = "BackShadow", IconTexture = "BackIcon", Scene = prevScene };
            replays = new() { StartPos = (111, 46), EndPos = (126, 60), Label = "replays", ShadowTexture = "ReplaysShadow", IconTexture = "ReplaysIcon", Scene = new ReplaysScene(this) };
            statistics = new() { StartPos = (111, 63), EndPos = (126, 76), Label = "statistics", ShadowTexture = "StatisticsShadow", IconTexture = "StatisticsIcon", Scene = new StatisticsScene(this) };
            search = new() { StartPos = (112, 78), EndPos = (125, 91), Label = "search", ShadowTexture = "SearchShadow", IconTexture = "SearchIcon", Scene = new SearchScene(this) };
            profile = new() { StartPos = (112, 93), EndPos = (125, 109), Label = "profile", ShadowTexture = "ProfileShadow", IconTexture = "ProfileIcon", Scene = new ProfileScene(this) };
            settings = new() { StartPos = (111, 111), EndPos = (126, 126), Label = "settings", ShadowTexture = "SettingsShadow", IconTexture = "SettingsIcon", Scene = new SettingsScene(this) };
            icons = [back, replays, statistics, search, profile, settings];

            curIcon = null;
        }

        public void Update()
        {
            MouseState state = Mouse.GetState();
            cursorX = state.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            cursorY = state.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

            // Get the size of the viewport
            int viewportWidth = p8.Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = p8.Batch.GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int w = viewportWidth / 128;
            int h = viewportHeight / 128;

            foreach (Icon icon in icons)
            {
                if (cursorX > icon.StartPos.x * w && cursorX < icon.EndPos.x * w && cursorY > icon.StartPos.y * h && cursorY < icon.EndPos.y * h) { curIcon = icon; break; }
            }

            if (state.LeftButton == ButtonState.Pressed && prevState.LeftButton == ButtonState.Released && curIcon is not null && curIcon.Scene is not null) { p8.LoadCart(curIcon.Scene); }
            prevState = state;
        }

        public void Draw()
        {
            p8.Batch.GraphicsDevice.Clear(Color.Black);

            // Get the size of the viewport
            int viewportWidth = p8.Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = p8.Batch.GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int w = viewportWidth / 128;
            int h = viewportHeight / 128;

            Vector2 size = new(w, h);
            Vector2 halfsize = new(w / 2.0f, h / 2.0f);

            MouseState state = Mouse.GetState();
            float cursorX = state.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            float cursorY = state.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

            p8.Batch.Draw(p8.TextureDictionary["BlankBackground"], new(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            float off = 0.6f;

            foreach (Icon icon in icons)
            {
                bool sel = cursorX > icon.StartPos.x * w && cursorX < icon.EndPos.x * w && cursorY > icon.StartPos.y * h && cursorY < icon.EndPos.y * h;
                if (icon.ShadowTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.ShadowTexture], new(icon.StartPos.x * w, icon.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
                if (icon.IconTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.IconTexture], new((icon.StartPos.x - (sel ? off : 0)) * w, (icon.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
            }

            p8.Batch.Draw(p8.TextureDictionary["Cursor"], new(cursorX - 15 * (w / 2.0f), cursorY - 15 * (h / 2.0f)), null, Color.White, 0, Vector2.Zero, halfsize, SpriteEffects.None, 0);
        }

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
