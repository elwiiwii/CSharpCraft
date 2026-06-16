using CSharpCraft.MainMenu;
using PSharp8.Audio;
using PSharp8.Scene;

namespace CSharpCraft.PcraftScenes;

internal class MainScene : IScene
{
    public string Name => "Pcraft Base";

    public void Init(ISceneSetup setup)
    {
        setup.Resolution = (141, 141);
        MainMenuObj menu = new();

        _ = setup.RegisterUpdate(menu.Update, fps: 60);

        _ = setup.RegisterDraw(menu.Draw, fps: 60);
    }

    public string SpritesPath => "Spritesheet_Main";

    public string? MapPath => null;

    public string? FlagData => null;

    public IReadOnlyList<Soundtrack> Music => [
        new(name: "original",
        tracks: [
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false),
                              new(filename: "pcraft_og_cave_1", loop: true)], channel: 0),
            
            new Track(parts: [new(filename: "pcraft_og_surface", loop: true)], channel: 1),

            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false),
                              new(filename: "pcraft_og_cave_1", loop: true)], channel: 2),

            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false),
                              new(filename: "pcraft_og_cave_1", loop: true)], channel: 3),

            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false),
                              new(filename: "pcraft_og_cave_1", loop: true)], channel: 4)]
        ),
        
        new(name: "new!",
        tracks: [
            new Track(parts: [new(filename: "pcraft_new_title", loop: true)], channel: 0),

            new Track(parts: [new(filename: "pcraft_new_surface", loop: true)], channel: 1),

            new Track(parts: [new(filename: "pcraft_new_cave", loop: true)], channel: 1),

            new Track(parts: [new(filename: "pcraft_new_title", loop: false),
                              new(filename: "pcraft_new_title", loop: true)], channel: 3),
            
            new Track(parts: [new(filename: "pcraft_new_death", loop: true)], channel: 4)]
        ),

        new(name: "pog edition",
        tracks: [
            new Track(parts: [new(filename: "pcraft_pe_title_0", loop: false),
                              new(filename: "pcraft_pe_title_1", loop: true)], channel: 0),
            
            new Track(parts: [new(filename: "pcraft_pe_surface_0", loop: false),
                              new(filename: "pcraft_pe_surface_1", loop: true)], channel: 1),
            
            new Track(parts: [new(filename: "pcraft_pe_cave_0", loop: false),
                              new(filename: "pcraft_pe_cave_1", loop: true)], channel: 2),
            
            new Track(parts: [new(filename: "pcraft_pe_win", loop: false)], channel: 3),

            new Track(parts: [new(filename: "pcraft_pe_death", loop: true)], channel: 4)]
        )
    ];

    public IReadOnlyList<SfxPack> Sfx => [
        new(name: "original", prefix: "pcraft_og_"),
        new(name: "soft", prefix: "pcraft_soft_"),
        new(name: "pog edition", prefix: "pcraft_pe_")
    ];
}
