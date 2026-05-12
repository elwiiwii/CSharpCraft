using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftDeluxe.Map;

namespace CSharpCraft.PcraftDeluxe;

internal class DeluxeServices : PcraftServices
{
    internal DeluxeServices()
    {
        SetServices(this);
    }

    protected override TileId[,] OnCreateMapStep(int sx, int sy, TileId a, TileId b, TileId c, TileId d, TileId e)
        => MapGenerator.CreateMapStep(sx, sy, a, b, c, d, e);
}
