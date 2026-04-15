namespace CSharpCraft.PcraftBase.Menu;

internal interface IMenu
{
    void Update(WorldState state, PcraftGame game);
    void Draw(WorldState state);
}
