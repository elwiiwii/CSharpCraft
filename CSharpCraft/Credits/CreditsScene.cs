using CSharpCraft.Credits.Credits;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Credits
{
    public class CreditsScene : IScene, IDisposable
    {
        public string SceneName => @"credits";
        private Pico8Functions p8;
        List<CreditsItem> credits;

        private int menuSelected;
        private KeyboardState prevState;

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;

            menuSelected = 0;
            prevState = Keyboard.GetState();

            credits = new();

            CreditsItem nusan = new()
            {
                Name = "nusan",
                Description = "",
                Links = new()
            };
            nusan.Links.Add("website", "https://nusan.fr");
            nusan.Links.Add("website", "https://linktr.ee/poticogames");
            nusan.Links.Add("itchio", "https://nusan.itch.io");
            nusan.Links.Add("steam", "https://store.steampowered.com/developer/NuSan");
            credits.Add(nusan);

            CreditsItem lexaloffle = new()
            {
                Name = "lexaloffle",
                Description = "",
                Links = new()
            };
            lexaloffle.Links.Add("pico8", "https://www.lexaloffle.com");
            credits.Add(lexaloffle);

            CreditsItem ellie = new()
            {
                Name = "ellie",
                Description = "",
                Links = new()
            };
            ellie.Links.Add("youtube", "https://www.youtube.com/@elwiiwii");
            ellie.Links.Add("discord", "@elwiiwii");
            ellie.Links.Add("github", "https://github.com/elwiiwii");
            credits.Add(ellie);

            CreditsItem holoknight = new()
            {
                Name = "holoknight",
                Description = "",
                Links = new()
            };
            holoknight.Links.Add("bandcamp", "https://holoknight.bandcamp.com/");
            holoknight.Links.Add("youtube", "https://www.youtube.com/@holoknight");
            holoknight.Links.Add("discord", "@holoknight");
            credits.Add(holoknight);

            CreditsItem cassie = new()
            {
                Name = "cassie",
                Description = "",
                Links = new()
            };
            cassie.Links.Add("twitter", "https://x.com/rythin_rta");
            cassie.Links.Add("bluesky", "https://bsky.app/profile/rythin.bsky.social");
            cassie.Links.Add("youtube", "https://www.youtube.com/@rythin");
            credits.Add(cassie);
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (p8.Btnp(2)) { menuSelected -= 1; }
            if (p8.Btnp(3)) { menuSelected += 1; }

            menuSelected = GeneralFunctions.Loop(menuSelected, credits);

            if ((state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter)) || p8.Btnp(4) || p8.Btnp(5))
            {
                OpenBrowser.OpenUrl(credits[menuSelected].Links.ElementAt(0).Value);
            }

            prevState = state;
        }

        public void Draw()
        {
            p8.Cls();

            p8.Print(">", 0, 62 + (menuSelected * 6), 7);

            int i = 0;
            foreach (CreditsItem item in credits)
            {
                p8.Print(item.Name, 8, 62 + i, 7);

                i += 6;
            }
        }

        public string SpriteData => @"";
        public string FlagData => @"";
        public string MapData => @"";

        public void Dispose()
        {

        }

    }
}
