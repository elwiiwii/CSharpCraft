using System;
using FixMath;

namespace CSharpCraft.Pico8.Services;

/// <summary>
/// Service responsible for map rendering and tile management
/// Extracted from Pico8Functions to enable service-based architecture
/// Phase 3: Service Extraction - Part 2
/// </summary>
public class MapService
{
    private readonly IGraphicsEngine _graphicsService;
    private readonly int[] _map;
    private readonly IScene _cart;
    private readonly UtilityService _utilities;

    public MapService(IGraphicsEngine graphicsService, int[] map, IScene cart, UtilityService utilities)
    {
        _graphicsService = graphicsService ?? throw new ArgumentNullException(nameof(graphicsService));
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _cart = cart ?? throw new ArgumentNullException(nameof(cart));
        _utilities = utilities ?? throw new ArgumentNullException(nameof(utilities));
    }

    /// <summary>
    /// Render map tiles to screen (Pico-8: Map)
    /// https://pico-8.fandom.com/wiki/Map
    /// </summary>
    public void Map(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0)
    {
        int cwFlr = (int)Math.Floor(celw);
        int chFlr = (int)Math.Floor(celh);

        for (int i = 0; i <= cwFlr; i++)
        {
            for (int j = 0; j <= chFlr; j++)
            {
                int mapTile = Mget(celx + i, cely + j);
                if (flags == 0 || flags == _utilities.Fget(mapTile))
                {
                    _graphicsService.DrawSprite(mapTile, F32.FromInt((int)(sx + i * 8)), F32.FromInt((int)(sy + j * 8)));
                }
            }
        }
    }

    /// <summary>
    /// Get a map tile value (Pico-8: Mget)
    /// https://pico-8.fandom.com/wiki/Mget
    /// </summary>
    public int Mget(double celx, double cely)
    {
        int xFlr = Math.Abs((int)Math.Floor(celx));
        int yFlr = Math.Abs((int)Math.Floor(cely));

        return _map[xFlr + yFlr * _cart.MapDimensions.x];
    }

    /// <summary>
    /// Set a map tile value (Pico-8: Mset)
    /// https://pico-8.fandom.com/wiki/Mset
    /// </summary>
    public void Mset(double celx, double cely, double snum = 0)
    {
        int xFlr = (int)Math.Floor(celx);
        int yFlr = (int)Math.Floor(cely);
        int sFlr = (int)Math.Floor(snum);

        _map[xFlr + yFlr * _cart.MapDimensions.x] = sFlr;
    }

    /// <summary>
    /// Get map dimensions
    /// </summary>
    public (int x, int y) MapDimensions => _cart.MapDimensions;
}
