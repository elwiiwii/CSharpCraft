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
        private Icon ranked;
        private Icon speedrun;
        private Icon unranked;
        private Icon @private;
        private Icon replays;
        private Icon statistics;
        private Icon search;
        private Icon profile;
        private Icon settings;

        private class Icon
        {
            public (int x, int y) StartPos { get; init; }
            public (int x, int y) EndPos { get; init; }
            public string Label { get; init; } = "";
        }

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;
            ranked = new() { StartPos = (30, 32), EndPos = (61, 63), Label = "ranked" };
            speedrun = new() { StartPos = (66, 32), EndPos = (97, 63), Label = "speedrun" };
            unranked = new() { StartPos = (30, 68), EndPos = (61, 99), Label = "unranked" };
            @private = new() { StartPos = (66, 68), EndPos = (97, 99), Label = "private" };
            replays = new() { StartPos = (111, 46), EndPos = (125, 59), Label = "replays" };
            statistics = new() { StartPos = (111, 63), EndPos = (125, 75), Label = "statistics" };
            search = new() { StartPos = (112, 78), EndPos = (124, 90), Label = "search" };
            profile = new() { StartPos = (112, 93), EndPos = (124, 108), Label = "profile" };
            settings = new() { StartPos = (111, 111), EndPos = (125, 125), Label = "settings" };
        }

        public void Update()
        {

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

            p8.Batch.Draw(p8.TextureDictionary["CompetitiveBackground"], new(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            float off = 0.6f;
            bool sel = cursorX > ranked.StartPos.x * w && cursorX < ranked.EndPos.x * w && cursorY > ranked.StartPos.y * h && cursorY < ranked.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["ModeShadow"], new(ranked.StartPos.x * w, ranked.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["RankedIcon"], new((ranked.StartPos.x - (sel ? off : 0)) * w, (ranked.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > speedrun.StartPos.x * w && cursorX < speedrun.EndPos.x * w && cursorY > speedrun.StartPos.y * h && cursorY < speedrun.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["ModeShadow"], new(speedrun.StartPos.x * w, speedrun.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["SpeedrunIcon"], new((speedrun.StartPos.x - (sel ? off : 0)) * w, (speedrun.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > unranked.StartPos.x * w && cursorX < unranked.EndPos.x * w && cursorY > unranked.StartPos.y * h && cursorY < unranked.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["ModeShadow"], new(unranked.StartPos.x * w, unranked.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["UnrankedIcon"], new((unranked.StartPos.x - (sel ? off : 0)) * w, (unranked.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > @private.StartPos.x * w && cursorX < @private.EndPos.x * w && cursorY > @private.StartPos.y * h && cursorY < @private.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["ModeShadow"], new(@private.StartPos.x * w, @private.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["PrivateIcon"], new((@private.StartPos.x - (sel ? off : 0)) * w, (@private.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > replays.StartPos.x * w && cursorX < replays.EndPos.x * w && cursorY > replays.StartPos.y * h && cursorY < replays.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["ReplaysShadow"], new(replays.StartPos.x * w, replays.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["ReplaysIcon"], new((replays.StartPos.x - (sel ? off : 0)) * w, (replays.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > statistics.StartPos.x * w && cursorX < statistics.EndPos.x * w && cursorY > statistics.StartPos.y * h && cursorY < statistics.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["StatisticsShadow"], new(statistics.StartPos.x * w, statistics.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["StatisticsIcon"], new((statistics.StartPos.x - (sel ? off : 0)) * w, (statistics.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > search.StartPos.x * w && cursorX < search.EndPos.x * w && cursorY > search.StartPos.y * h && cursorY < search.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["SearchShadow"], new(search.StartPos.x * w, search.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["SearchIcon"], new((search.StartPos.x - (sel ? off : 0)) * w, (search.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > profile.StartPos.x * w && cursorX < profile.EndPos.x * w && cursorY > profile.StartPos.y * h && cursorY < profile.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["ProfileShadow"], new(profile.StartPos.x * w, profile.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["ProfileIcon"], new((profile.StartPos.x - (sel ? off : 0)) * w, (profile.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            sel = cursorX > settings.StartPos.x * w && cursorX < settings.EndPos.x * w && cursorY > settings.StartPos.y * h && cursorY < settings.EndPos.y * h;
            p8.Batch.Draw(p8.TextureDictionary["SettingsShadow"], new(settings.StartPos.x * w, settings.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["SettingsIcon"], new((settings.StartPos.x - (sel ? off : 0)) * w, (settings.StartPos.y - (sel ? off : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            p8.Batch.Draw(p8.TextureDictionary["NameBubbleEdge"], new((63 - 5 - 000) * w, 108 * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["NameBubbleEdge"], new((63 + 000) * w, 108 * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
            //p8.Batch.Draw(p8.TextureDictionary["NameBubbleCenter"], new(63 * w, 108 * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

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
