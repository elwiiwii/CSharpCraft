using CSharpCraft.OptionsMenu;
using System.IO.Pipelines;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;


namespace CSharpCraft.Competitive
{
    public class CompetitiveScene : IScene
    {
        public string SceneName { get => "competitive"; }
        private Pico8Functions p8;
        private Icon back;
        private Icon ranked;
        private Icon speedrun;
        private Icon unranked;
        private Icon @private;
        private Icon replays;
        private Icon statistics;
        private Icon search;
        private Icon profile;
        private Icon settings;
        private float labelLength;

        private Icon[] icons;
        private Icon? curIcon;
        private float cursorX;
        private float cursorY;
        private MouseState prevState;

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;
            back = new() { StartPos = (120, 3), EndPos = (126, 11), Label = "back", ShadowTexture = "BackShadow", IconTexture = "BackIcon", Scene = p8.TitleScreen };
            ranked = new() { StartPos = (30, 32), EndPos = (62, 64), Label = "ranked", ShadowTexture = "ModeShadow", IconTexture = "RankedIcon", Scene = new RankedScene(this) };
            speedrun = new() { StartPos = (66, 32), EndPos = (98, 64), Label = "speedrun", ShadowTexture = "ModeShadow", IconTexture = "SpeedrunIcon", Scene = new SpeedrunScene(this) };
            unranked = new() { StartPos = (30, 68), EndPos = (62, 100), Label = "unranked", ShadowTexture = "ModeShadow", IconTexture = "UnrankedIcon", Scene = new UnrankedScene(this) };
            @private = new() { StartPos = (66, 68), EndPos = (98, 100), Label = "private", ShadowTexture = "ModeShadow", IconTexture = "PrivateIcon", Scene = new PrivateScene(this) };
            replays = new() { StartPos = (111, 46), EndPos = (126, 60), Label = "replays", ShadowTexture = "ReplaysShadow", IconTexture = "ReplaysIcon", Scene = new ReplaysScene(this) };
            statistics = new() { StartPos = (111, 63), EndPos = (126, 76), Label = "statistics", ShadowTexture = "StatisticsShadow", IconTexture = "StatisticsIcon", Scene = new StatisticsScene(this) };
            search = new() { StartPos = (112, 78), EndPos = (125, 91), Label = "search", ShadowTexture = "SearchShadow", IconTexture = "SearchIcon", Scene = new SearchScene(this) };
            profile = new() { StartPos = (112, 93), EndPos = (125, 109), Label = "profile", ShadowTexture = "ProfileShadow", IconTexture = "ProfileIcon", Scene = new ProfileScene(this) };
            settings = new() { StartPos = (111, 111), EndPos = (126, 126), Label = "settings", ShadowTexture = "SettingsShadow", IconTexture = "SettingsIcon", Scene = new SettingsScene(this) };
            icons = [back, ranked, speedrun, unranked, @private, replays, statistics, search, profile, settings];

            labelLength = 0;
            curIcon = null;
            prevState = Mouse.GetState();
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

            curIcon = null;
            foreach (Icon icon in icons)
            {
                if (cursorX > icon.StartPos.x * w && cursorX < icon.EndPos.x * w && cursorY > icon.StartPos.y * h && cursorY < icon.EndPos.y * h) { curIcon = icon; break; }
            }

            if (state.LeftButton == ButtonState.Pressed && prevState.LeftButton == ButtonState.Released && curIcon is not null && curIcon.Scene is not null) { p8.LoadCart(curIcon.Scene); }
            prevState = state;
        }

        private void Printcb(string t, double x, double y, int c1, int c2)
        {
            p8.Print(t, x + 1 - t.Length * 2 + 1, y, c2);
            p8.Print(t, x + 1 - t.Length * 2 - 1, y, c2);
            p8.Print(t, x + 1 - t.Length * 2, y + 1, c2);
            p8.Print(t, x + 1 - t.Length * 2, y - 1, c2);
            p8.Print(t, x + 1 - t.Length * 2, y, c1);
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

            p8.Batch.Draw(p8.TextureDictionary["CompetitiveBackground"], new(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            
            float off = 0.6f;
            string curLabel = "";

            foreach (Icon icon in icons)
            {
                bool sel = cursorX > icon.StartPos.x * w && cursorX < icon.EndPos.x * w && cursorY > icon.StartPos.y * h && cursorY < icon.EndPos.y * h;
                if (icon.ShadowTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.ShadowTexture], new(icon.StartPos.x * w, icon.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
                if (icon.IconTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.IconTexture], new((icon.StartPos.x - (sel ? off : 0)) * w, (icon.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
            }

            if (curIcon is not null) { labelLength = curIcon.Label.Length * 4; }
            else { labelLength = Math.Max(labelLength - labelLength / 3, 0); }
            p8.Batch.Draw(p8.TextureDictionary["NameBubbleCenter"], new((63 - labelLength / 2) * w, 108 * h), null, Color.White, 0, Vector2.Zero, new Vector2(w * (labelLength + 1), h), SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["NameBubbleEdge"], new((63 - 5 - labelLength / 2 - 1) * w, 108 * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["NameBubbleEdge"], new((63 + labelLength / 2 + 1) * w, 108 * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
            if (curIcon is not null) { Printcb(curIcon.Label, 63, 111, 15, 1); }

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
