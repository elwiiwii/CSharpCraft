using CSharpCraft.Credits.Credits;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Credits;

public class CreditsScene : IScene, IDisposable
{
    public string SceneName => @"credits";
    private Pico8Functions p8;
    List<CreditsItem> credits = [];

    private (int hor, int ver) menuSelected;
    private KeyboardState prevState;

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        menuSelected = (0, 0);
        prevState = Keyboard.GetState();

        credits = new();

        CreditsItem nusan = new()
        {
            Name = "nusan",
            Description = new(),
            Links = new()
        };
        nusan.Description.Add("THANK YOU NUSAN FOR");
        nusan.Description.Add("MAKING PCRAFT! IT'S HER");
        nusan.Description.Add("GAME AND YOU CAN SUPPORT");
        nusan.Description.Add("HER ON ITCH.IO OR STEAM!");
        nusan.Links.Add(("Itchio", "https://nusan.itch.io"));
        nusan.Links.Add(("Steam", "https://steampowered.com/developer/NuSan"));
        nusan.Links.Add(("Website", "https://nusan.fr"));
        nusan.Links.Add(("Website", "https://linktr.ee/poticogames"));
        credits.Add(nusan);

        CreditsItem lexaloffle = new()
        {
            Name = "lexaloffle",
            Description = new(),
            Links = new()
        };
        lexaloffle.Description.Add("CHECK OUT THE MAKERS OF");
        lexaloffle.Description.Add("pico 8! THEY MADE ALL OF THIS");
        lexaloffle.Description.Add("POSSIBLE AND INSPIRED A LOT");
        lexaloffle.Description.Add("OF THE STYLE OF THIS GAME");
        lexaloffle.Links.Add(("Pico8", "https://lexaloffle.com"));
        credits.Add(lexaloffle);

        CreditsItem ellie = new()
        {
            Name = "ellie",
            Description = new(),
            Links = new()
        };
        ellie.Description.Add("OH LOOK ITS ME I MADE THIS!");
        ellie.Description.Add("ALSO SHOUT OUT TO MY DAD <3");
        ellie.Links.Add(("Youtube", "https://youtube.com/@elwiiwii"));
        ellie.Links.Add(("Discord", "@elwiiwii"));
        ellie.Links.Add(("Github", "https://github.com/elwiiwii"));
        credits.Add(ellie);

        CreditsItem holoknight = new()
        {
            Name = "holoknight",
            Description = new(),
            Links = new()
        };
        holoknight.Description.Add("THANK YOU HOLOKNIGHT FOR THE");
        holoknight.Description.Add("AWESOME NEW MUSIC! YOU CAN");
        holoknight.Description.Add("SUPPORT THEM ON BANDCAMP!");
        holoknight.Links.Add(("Bandcamp", "https://holoknight.bandcamp.com"));
        holoknight.Links.Add(("Youtube", "https://youtube.com/@holoknight"));
        holoknight.Links.Add(("Discord", "@holoknight"));
        credits.Add(holoknight);

        CreditsItem cassie = new()
        {
            Name = "cassie",
            Description = new(),
            Links = new()
        };
        cassie.Description.Add("THANK YOU CASSIE FOR ALL THE");
        cassie.Description.Add("PICO 8 MODS YOU HAVE MADE");
        cassie.Description.Add("AND ALL THE TASES TOO :3");
        cassie.Links.Add(("Twitter", "https://x.com/rythin_rta"));
        cassie.Links.Add(("Bluesky", "https://bsky.app/profile/rythin.bsky.social"));
        cassie.Links.Add(("Youtube", "https://youtube.com/@rythin"));
        credits.Add(cassie);
    }

    public void Update()
    {
        KeyboardState state = Keyboard.GetState();

        if (p8.Btnp(0)) { menuSelected.hor -= 1; }
        if (p8.Btnp(1)) { menuSelected.hor += 1; }
        if (p8.Btnp(2)) { menuSelected.ver -= 1; menuSelected.hor = 0; }
        if (p8.Btnp(3)) { menuSelected.ver += 1; menuSelected.hor = 0; }

        menuSelected.ver = GeneralFunctions.Loop(menuSelected.ver, credits.Count);
        menuSelected.hor = GeneralFunctions.Loop(menuSelected.hor, credits[menuSelected.ver].Links.Count + 1);

        if ((state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter)) || p8.Btnp(4) || p8.Btnp(5))
        {
            if (menuSelected.hor == 0)
            {
                menuSelected.hor = 1;
            }
            else
            {
                OpenBrowser.OpenUrl(credits[menuSelected.ver].Links[menuSelected.hor - 1].link);
            }
        }

        prevState = state;
    }

    public void Draw()
    {
        p8.Cls(1);

        // Get the size of the viewport
        int viewportWidth = p8.Batch.GraphicsDevice.Viewport.Width;
        int viewportHeight = p8.Batch.GraphicsDevice.Viewport.Height;

        // Calculate the size of each cell
        int cellWidth = viewportWidth / 128;
        int cellHeight = viewportHeight / 128;

        Vector2 size = new(cellWidth, cellHeight);
        Vector2 halfsize = new(cellWidth / 2f, cellHeight / 2f);

        p8.Batch.Draw(p8.TextureDictionary["Credits"], new Vector2(27 * cellWidth, 8 * cellHeight), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        int icon_gap = 4;
        int item_gap = 4;
        int description_indent = 3;
        int xstart = 6;
        int ystart = 40;
        int ypos = 0;
        int yoff = 0;
        foreach (CreditsItem item in credits)
        {
            if (ypos / 9 == menuSelected.ver)
            {
                if (!(ypos == 0))
                {
                    yoff += item_gap;
                }

                if (menuSelected.hor == 0)
                {
                    p8.Print(">", 0, ystart + (menuSelected.ver * 9) + yoff, 7);
                }
                else
                {
                    Vector2 position2 = new((xstart + credits[menuSelected.ver].Name.Length * 4 + (menuSelected.hor - 1) * 8 + icon_gap) * cellWidth, (ystart - 5 + menuSelected.ver * 9 + yoff) * cellHeight);
                    p8.Batch.Draw(p8.TextureDictionary["ArrowV"], position2, null, p8.Colors[7], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    p8.Print(credits[menuSelected.ver].Links[menuSelected.hor - 1].link.Replace("https://", ""),
                        xstart + 6 + credits[menuSelected.ver].Name.Length * 4 + 8 * credits[menuSelected.ver].Links.Count,
                        ystart + menuSelected.ver * 9 + yoff,
                        6);
                }
            }

            p8.Print(item.Name, xstart, ypos + yoff + ystart, 7);

            int xpos = 0;
            foreach ((string type , string link) icon in item.Links)
            {
                Vector2 position = new((xstart + item.Name.Length * 4 + xpos + icon_gap - 1) * cellWidth, (ypos + yoff + ystart - 1) * cellHeight);
                p8.Batch.Draw(p8.TextureDictionary[icon.type], position, null, Color.White, 0, Vector2.Zero, halfsize, SpriteEffects.None, 0);
                xpos += 8;
            }

            if (ypos / 9 == menuSelected.ver)
            {
                ypos += 4;

                int linecount = 6;
                foreach (string line in item.Description)
                {
                    p8.Print(line,
                        xstart + description_indent,
                        ypos + yoff + linecount + ystart - 1,
                        6);
                    linecount += 6;
                }

                yoff += item.Description.Count * 6 + item_gap - 1;
            }

            ypos += 9;
        }
    }
    public string SpriteImage => "";
    public string SpriteData => @"";
    public string FlagData => @"";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => @"";
    public Dictionary<string, List<SongInst>> Music => new();
    public Dictionary<string, Dictionary<int, string>> Sfx => new();
    public void Dispose()
    {

    }

}
