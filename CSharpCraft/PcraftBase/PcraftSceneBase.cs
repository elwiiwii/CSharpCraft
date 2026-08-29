using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Draw;
using PSharp8.Audio;
using PSharp8.Scene;

namespace CSharpCraft.PcraftBase;

internal abstract class PcraftSceneBase : IScene
{
    public virtual string Name => "Pcraft Base";

    private PlayerEntity? _player;
    private bool _initialized;

    public virtual void Init(ISceneSetup setup)
    {
        setup.Resolution = (128, 128);

        PcraftSession session = CreateSession();
        PcraftSession.SetCurrent(session);
        _player = session.Player;
        _initialized = false;
    }

    public virtual void Update()
    {
        PlayerEntity player = _player!;
        if (!_initialized)
        {
            Pico8.Music(4, 10000);
            PcraftServices.ResetLevel(player);
            Pico8.Music(1);
            _initialized = true;
        }

        PcraftServices.UpdateMain(player);
    }

    public virtual void Draw()
    {
        PlayerEntity player = _player!;
        PcraftDraw.Draw(player, player.CurrentLevel!);
    }

    protected virtual PcraftSession CreateSession()
    {
        return new PcraftSession(new PlayerEntity(F32.Zero, F32.Zero));
    }

    public virtual string SpritesPath => "pcraft_sprites";

    public virtual string MapPath => "pcraft_map";

    public virtual string? FlagData => null;

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
