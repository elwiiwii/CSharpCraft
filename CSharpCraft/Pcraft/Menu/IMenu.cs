#nullable enable

namespace CSharpCraft.Pcraft.Menu;

internal interface IMenu
{
    void Update(WorldState state, PcraftGame game);
    void Draw(WorldState state);
}
