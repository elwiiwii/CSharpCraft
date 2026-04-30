using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Menu;

internal interface IMenu
{
    bool Update(PlayerEntity player);
    void Draw(PlayerEntity player, Level level);
}
