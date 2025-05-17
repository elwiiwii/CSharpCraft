using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft;

public class TitleScreen(bool animation = false) : IScene, IDisposable
{
    public string SceneName { get => "TitleScreen"; }
    private Pico8Functions p8;
    private readonly string version = "1.1.3";

    private int menuSelected;
    private KeyboardState prevState;
    private int frame;

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        menuSelected = 0;
        prevState = Keyboard.GetState();
        frame = animation ? 0 : 50;
    }

    public void Update()
    {
        KeyboardState state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q) && !prevState.IsKeyDown(Keys.Q))
        {
            Environment.Exit(0);
        }

        if (p8.Btnp(2)) { menuSelected -= 1; }
        if (p8.Btnp(3)) { menuSelected += 1; }

        menuSelected = GeneralFunctions.Loop(menuSelected, p8.Scenes);

        if (frame >= 39 && ((state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter)) || p8.Btnp(4) || p8.Btnp(5)))
        {
            p8.LoadCart(p8.Scenes[menuSelected]);
        }

        prevState = state;
        if (animation) { frame++; }
    }

    public void Draw()
    {
        p8.Batch.GraphicsDevice.Clear(Color.Black);

        // Get the size of the viewport
        int viewportWidth = p8.Batch.GraphicsDevice.Viewport.Width;
        int viewportHeight = p8.Batch.GraphicsDevice.Viewport.Height;

        // Calculate the size of each cell
        int cellWidth = viewportWidth / 128;
        int cellHeight = viewportHeight / 128;

        Vector2 position = new(1 * cellWidth, 1 * cellHeight);
        Vector2 size = new(cellWidth, cellHeight);

        Texture2D logo = p8.TextureDictionary["CSharpCraftLogo"];
        p8.Batch.Draw(logo, position, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        if (frame >= 5) { p8.Print($"c# craft {version}", 0, 18, 6); }
        if (frame >= 6) { p8.Print("by nusan-2016 and ellie-2024", 0, 24, 6); }

        if (frame >= 7) { p8.Batch.Draw(p8.TextureDictionary["MusicNote"], new(3 * cellWidth, 36 * cellHeight), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0); }
        if (frame >= 11) { p8.Batch.Draw(p8.TextureDictionary["MusicNote"], new(11 * cellWidth, 38 * cellHeight), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0); }
        if (frame >= 15) { p8.Batch.Draw(p8.TextureDictionary["MusicNote"], new(19 * cellWidth, 36 * cellHeight), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0); }
        if (frame >= 19) { p8.Batch.Draw(p8.TextureDictionary["MusicNote"], new(27 * cellWidth, 34 * cellHeight), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0); }

        if (frame >= 29) { p8.Print("choose a game mode", 0, 50, 6); }

        if (frame >= 39)
        {
            p8.Print(">", 0, 62 + (menuSelected * 6), 7);
            int i = 0;
            foreach (IScene scene in p8.Scenes)
            {
                p8.Print(scene.SceneName, 8, 62 + i, 7);

                i += 6;
            }
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
