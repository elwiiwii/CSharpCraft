using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Draw;
using PSharp8.Audio;
using PSharp8.Scene;

namespace CSharpCraft.PcraftBase;

internal abstract class PcraftSceneBase : IScene
{
    public virtual string? Name => "Pcraft Base";

    public virtual void Init(ISceneSetup setup)
    {
        setup.Resolution = (128, 128);

        var player   = new PlayerEntity(F32.Zero, F32.Zero);
        var game     = new PcraftGame();
        Level? cave        = null;
        Level? island      = null;
        Level? currentLevel = null;
        bool switchLevel    = false;
        bool canSwitchLevel = false;
        bool initialized    = false;

        setup.RegisterUpdate(() =>
        {
            if (!initialized)
            {
                game.Init();
                PcraftServices.ResetLevel(player, game, out cave, out island);
                currentLevel = island!;
                Pico8.Music(1);
                initialized = true;
            }

            if (game.NeedsReset)
            {
                game.NeedsReset = false;
                PcraftServices.ResetLevel(player, game, out cave, out island);
                currentLevel = island!;
                Pico8.Music(1);
            }

            if (switchLevel)
            {
                currentLevel = (currentLevel == cave) ? island! : cave!;
                PcraftServices.SetLevel(currentLevel, player);
                PcraftServices.FillEne(currentLevel, player);
                switchLevel    = false;
                canSwitchLevel = false;
                Pico8.Music(currentLevel == cave ? 2 : 1);
            }

            PcraftServices.UpdateMain(player, currentLevel!, game, ref switchLevel, ref canSwitchLevel);
        }, fps: 30);

        setup.RegisterDraw(() => PcraftDraw.Draw(player, currentLevel!, game), fps: 30);
    }

    public virtual string? SpritesPath => "pcraft_sprites";

    public virtual string? MapPath => "pcraft_map";

    public virtual string? FlagData => null;

    public virtual IReadOnlyList<Soundtrack> Music => [
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

    public virtual IReadOnlyList<SfxPack> Sfx => [
        new SfxPack(name: "original", prefix: "pcraft_og_"),
        new SfxPack(name: "soft", prefix: "pcraft_soft_"),
        new SfxPack(name: "pog edition", prefix: "pcraft_pe_")
    ];
}
