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
    public class ProfileScene(IScene prevScene) : IScene
    {
        public string SceneName { get => "profile"; }
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
            back = new() { StartPos = (120, 3), EndPos = (125, 10), Label = "back", ShadowTexture = "BackShadow", IconTexture = "BackIcon", Scene = prevScene };
            replays = new() { StartPos = (111, 46), EndPos = (125, 59), Label = "replays", ShadowTexture = "ReplaysShadow", IconTexture = "ReplaysIcon", Scene = new ReplaysScene(prevScene) };
            statistics = new() { StartPos = (111, 63), EndPos = (125, 75), Label = "statistics", ShadowTexture = "StatisticsShadow", IconTexture = "StatisticsIcon", Scene = new StatisticsScene(prevScene) };
            search = new() { StartPos = (112, 78), EndPos = (124, 90), Label = "search", ShadowTexture = "SearchShadow", IconTexture = "SearchIcon", Scene = new SearchScene(prevScene) };
            profile = new() { StartPos = (112, 93), EndPos = (124, 108), Label = "profile", ShadowTexture = "ProfileShadow", IconTexture = "ProfileIcon" };
            settings = new() { StartPos = (111, 111), EndPos = (125, 125), Label = "settings", ShadowTexture = "SettingsShadow", IconTexture = "SettingsIcon", Scene = new SettingsScene(prevScene) };
            icons = [back, replays, statistics, search, profile, settings];

            curIcon = null;
            prevState = Mouse.GetState();
            cursorX = prevState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            cursorY = prevState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);
        }

        public void Update()
        {
            MouseState state = Mouse.GetState();
            cursorX = state.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            cursorY = state.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

            curIcon = Shared.IconUpdate(p8, icons, cursorX, cursorY);

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

            p8.Batch.Draw(p8.TextureDictionary["BlankBackground"], new(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            Shared.DrawIcons(p8, icons, cursorX, cursorY);

            Shared.DrawCursor(p8, cursorX, cursorY);
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
