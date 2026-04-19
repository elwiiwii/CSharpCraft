using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Menu;

internal interface IMenu
{
    void Update(PlayerEntity player, PcraftGame game);
    void Draw(PlayerEntity player, Level level);
}
