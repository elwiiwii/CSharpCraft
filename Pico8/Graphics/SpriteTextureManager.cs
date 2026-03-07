using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pico8.Graphics;

public class SpriteTextureManager : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly PaletteManager _paletteManager;
    private readonly SpriteMapData _data;
    private Texture2D? _cachedSpritesheetTexture;
    private int _cachedSpriteVersion = -1;
    private int _cachedPaletteVersion = -1;
    private readonly Dictionary<(int, int, int, int, int), (Texture2D tex, int sprV, int mapV, int palV)> _mapTextureCache = new();

    public SpriteTextureManager(
        GraphicsDevice graphicsDevice,
        PaletteManager paletteManager,
        SpriteMapData data)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _paletteManager = paletteManager ?? throw new ArgumentNullException(nameof(paletteManager));
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public Texture2D GetSpritesheetTexture()
    {
        if (_cachedSpritesheetTexture is null ||
            _data.SpritesheetVersion != _cachedSpriteVersion ||
            _paletteManager.PaletteVersion != _cachedPaletteVersion)
        {
            _cachedSpritesheetTexture ??= new Texture2D(_graphicsDevice, _data.SpriteSheetWidth, _data.SpriteSheetHeight);
            Color[] spritesheetData = _data.SpritesheetData;
            Color[] applied = new Color[spritesheetData.Length];
            for (int i = 0; i < spritesheetData.Length; i++)
                applied[i] = _paletteManager.PaletteMap.TryGetValue(spritesheetData[i], out Color mapped) ? mapped : spritesheetData[i];
            _cachedSpritesheetTexture.SetData(applied);
            _cachedSpriteVersion = _data.SpritesheetVersion;
            _cachedPaletteVersion = _paletteManager.PaletteVersion;
        }
        return _cachedSpritesheetTexture;
    }

    public Texture2D GetMapRegionTexture(int mapX, int mapY, int mapW, int mapH, int flags = 0)
    {
        var key = (mapX, mapY, mapW, mapH, flags);
        int texW = mapW * 8;
        int texH = mapH * 8;
        int spritesPerRow = _data.SpritesPerRow;
        int spriteSheetWidth = _data.SpriteSheetWidth;
        Color[] spritesheetData = _data.SpritesheetData;

        if (_mapTextureCache.TryGetValue(key, out var cached) &&
            cached.sprV == _data.SpritesheetVersion &&
            cached.mapV == _data.MapVersion &&
            cached.palV == _paletteManager.PaletteVersion)
        {
            return cached.tex;
        }

        Color[] pixels = new Color[texW * texH];
        for (int cy = 0; cy < mapH; cy++)
        {
            for (int cx = 0; cx < mapW; cx++)
            {
                int spriteIndex = _data.GetMapTile(mapX + cx, mapY + cy);
                bool drawSprite = flags == 0 || (_data.GetFlag(spriteIndex) & flags) != 0;
                int spriteOriginX = (spriteIndex % spritesPerRow) * 8;
                int spriteOriginY = (spriteIndex / spritesPerRow) * 8;
                for (int py = 0; py < 8; py++)
                {
                    for (int px = 0; px < 8; px++)
                    {
                        int destIdx = (cx * 8 + px) + (cy * 8 + py) * texW;
                        if (!drawSprite)
                        {
                            pixels[destIdx] = Color.Transparent;
                            continue;
                        }
                        Color src = spritesheetData[(spriteOriginX + px) + (spriteOriginY + py) * spriteSheetWidth];
                        pixels[destIdx] = _paletteManager.PaletteMap.TryGetValue(src, out Color mapped) ? mapped : src;
                    }
                }
            }
        }

        if (_mapTextureCache.TryGetValue(key, out var existing))
        {
            existing.tex.SetData(pixels);
            _mapTextureCache[key] = (existing.tex, _data.SpritesheetVersion, _data.MapVersion, _paletteManager.PaletteVersion);
            return existing.tex;
        }

        Texture2D tex = new Texture2D(_graphicsDevice, texW, texH);
        tex.SetData(pixels);
        _mapTextureCache[key] = (tex, _data.SpritesheetVersion, _data.MapVersion, _paletteManager.PaletteVersion);
        return tex;
    }

    public Rectangle GetSpriteSourceRect(int spriteIndex, int widthSprites = 1, int heightSprites = 1)
        => _data.GetSpriteSourceRect(spriteIndex, widthSprites, heightSprites);

    public void Dispose()
    {
        _cachedSpritesheetTexture?.Dispose();
        foreach (var entry in _mapTextureCache.Values)
            entry.tex.Dispose();
        _mapTextureCache.Clear();
    }
}
