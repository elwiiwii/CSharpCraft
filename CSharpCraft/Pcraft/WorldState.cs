using CSharpCraft.Pcraft.Data;

namespace CSharpCraft.Pcraft;

internal sealed class WorldState
{
    internal F32 Plx { get; set; } = F32.Zero;
    internal F32 Ply { get; set; } = F32.Zero;
    internal F32 Prot { get; set; } = F32.Zero;
    internal F32 Lrot { get; set; } = F32.Zero;
    internal F32 Panim { get; set; } = F32.Zero;
    internal F32 Banim { get; set; } = F32.Zero;
    internal F32 Pstam { get; set; } = F32.Zero;
    internal F32 Lstam { get; set; } = F32.Zero;
    internal F32 Plife { get; set; } = F32.Zero;
    internal F32 Llife { get; set; } = F32.Zero;
    internal bool Lb4 { get; set; } = false;
    internal bool Lb5 { get; set; } = false;
    internal bool Block5 { get; set; } = false;
    internal List<ItemStack> Invent { get; } = [];
    internal ItemStack? CurItem { get; set; } = null;
    internal MenuState? MenuInvent { get; set; } = null;
    internal Level? CurrentLevel { get; set; } = null;
    internal Level? Cave { get; set; } = null;
    internal Level? Island { get; set; } = null;
    internal int LevelX => CurrentLevel?.X ?? 0;
    internal int LevelY => CurrentLevel?.Y ?? 0;
    internal int LevelSx => CurrentLevel?.Sx ?? 0;
    internal int LevelSy => CurrentLevel?.Sy ?? 0;
    internal bool LevelUnder => CurrentLevel?.IsUnder ?? false;
    internal List<ItemEntity> Entities { get; } = [];
    internal List<CharacterEntity> Enemies { get; set; } = [];
    internal Dictionary<int, F32> Data { get; } = [];
    internal F32 Clx { get; set; } = F32.Zero;
    internal F32 Cly { get; set; } = F32.Zero;
    internal F32 Cmx { get; set; } = F32.Zero;
    internal F32 Cmy { get; set; } = F32.Zero;
    internal F32 Coffx { get; set; } = F32.Zero;
    internal F32 Coffy { get; set; } = F32.Zero;
    internal F32 Time { get; set; } = F32.Zero;
    internal bool SwitchLevel { get; set; } = false;
    internal bool CanSwitchLevel { get; set; } = false;
    internal int ToogleMenu { get; set; } = 0;
    internal MenuState? CurMenu { get; set; } = null;
    internal List<CharacterEntity> NearEnemies { get; } = [];
    internal F32[][] RndWat { get; }

    internal WorldState()
    {
        RndWat = new F32[16][];
        for (int i = 0; i < 16; i++)
        {
            RndWat[i] = new F32[16];
        }
    }

    internal void SetLevel(Level level)
    {
        CurrentLevel = level;
        
        Entities.Clear();
        Entities.AddRange(level.Ent);
        
        Enemies.Clear();
        Enemies.AddRange(level.Ene);
        
        Data.Clear();
        foreach (var (key, value) in level.Dat)
        {
            Data[key] = value;
        }
    }
}
