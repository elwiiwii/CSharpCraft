using CSharpCraft.Pcraft.Draw;
using CSharpCraft.Pcraft.Update;
using PSharp8.Audio;
using PSharp8.Scene;

namespace CSharpCraft.Pcraft;

internal sealed class PcraftSceneBase : IScene
{
    public string? Name => "Pcraft Base";

    public void Init(ISceneSetup setup)
    {
        setup.Resolution = (128, 128);

        var state       = new WorldState();
        var game        = new PcraftGame();
        bool initialized = false;

        setup.RegisterUpdate(() =>
        {
            if (!initialized) { game.Init(); initialized = true; }
            PcraftUpdate.Update(state, game);
        }, fps: 30);

        setup.RegisterDraw(() => PcraftDraw.Draw(state, game), fps: 30);
    }

    public string? SpritesPath => "pcraft_sprites";

    public string? MapPath => "pcraft_map";

    public string? FlagData => null;

    public IReadOnlyList<Soundtrack> Music => [
        new Soundtrack(name: "original", 
        tracks: [
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 0),
            new Track(parts: [new(filename: "pcraft_og_surface", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 2),
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 3),
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 4)]
        ),
        new Soundtrack(name: "new!", 
        tracks: [
            new Track(parts: [new(filename: "pcraft_new_title", loop: true)], channel: 0),
            new Track(parts: [new(filename: "pcraft_new_surface", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_new_cave", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_new_title", loop: false), new(filename: "pcraft_new_title", loop: true)], channel: 3),
            new Track(parts: [new(filename: "pcraft_new_death", loop: true)], channel: 4)]
        ),
        new Soundtrack(name: "pog edition", 
        tracks: [
            new Track(parts: [new(filename: "pcraft_pe_title_0", loop: false), new(filename: "pcraft_pe_title_1", loop: true)], channel: 0),
            new Track(parts: [new(filename: "pcraft_pe_surface_0", loop: false), new(filename: "pcraft_pe_surface_1", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_pe_cave_0", loop: false), new(filename: "pcraft_pe_cave_1", loop: true)], channel: 2),
            new Track(parts: [new(filename: "pcraft_pe_win", loop: false)], channel: 3),
            new Track(parts: [new(filename: "pcraft_pe_death", loop: true)], channel: 4)]
        )
    ];

    public IReadOnlyList<SfxPack> Sfx => [
        new SfxPack(name: "original", prefix: "pcraft_og_"),
        new SfxPack(name: "soft", prefix: "pcraft_soft_"),
        new SfxPack(name: "pog edition", prefix: "pcraft_pe_")
    ];
}
