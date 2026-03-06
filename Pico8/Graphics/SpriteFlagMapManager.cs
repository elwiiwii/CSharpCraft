using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pico8.Graphics;

public class SpriteFlagMapManager : IDisposable
{
    private const int SPRITE_WIDTH = 8;
    private const int SPRITE_HEIGHT = 8;
    private readonly Texture2D _originalSpritesheetTexture;
    private readonly Color[] _spritesheetData;
    private readonly int _spriteSheetWidth;
    private readonly int _spriteSheetHeight;
    private readonly int _spriteCount;
    private readonly Texture2D _originalMapTexture;
    private readonly int[] _mapData;
    private readonly int _mapWidth;
    private readonly int _mapHeight;
    private readonly int[] _originalFlags;
    private readonly int[] _flagData;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly PaletteManager _paletteManager;
    private Texture2D? _cachedSpritesheetTexture;
    private int _cachedSpriteVersion = -1;
    private int _cachedPaletteVersion = -1;
    private readonly Dictionary<(int, int, int, int, int), (Texture2D tex, int sprV, int mapV, int palV)> _mapTextureCache = new();

    public int SpritesheetVersion { get; private set; }
    public int MapVersion { get; private set; }

    public SpriteFlagMapManager(
        GraphicsDevice graphicsDevice,
        PaletteManager paletteManager,
        Texture2D spriteTexture,
        Texture2D mapTexture,
        string flagString)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _paletteManager = paletteManager ?? throw new ArgumentNullException(nameof(paletteManager));
        
        if (spriteTexture.Width % SPRITE_WIDTH != 0 || spriteTexture.Height % SPRITE_HEIGHT != 0)
            throw new ArgumentException(
                $"Sprite texture dimensions ({spriteTexture.Width}x{spriteTexture.Height}) must be multiples of {SPRITE_WIDTH}.",
                nameof(spriteTexture));

        if (mapTexture.Width % SPRITE_WIDTH != 0 || mapTexture.Height % SPRITE_HEIGHT != 0)
            throw new ArgumentException(
                $"Map texture dimensions ({mapTexture.Width}x{mapTexture.Height}) must be multiples of {SPRITE_WIDTH}.",
                nameof(mapTexture));

        _originalSpritesheetTexture = spriteTexture;
        _spriteSheetWidth = spriteTexture.Width;
        _spriteSheetHeight = spriteTexture.Height;
        _spriteCount = (_spriteSheetWidth / SPRITE_WIDTH) * (_spriteSheetHeight / SPRITE_HEIGHT);
        _originalMapTexture = mapTexture;
        _mapWidth = mapTexture.Width / SPRITE_WIDTH;
        _mapHeight = mapTexture.Height / SPRITE_HEIGHT;
        _originalFlags = flagString.Chunk(2)
            .Select(c => Convert.ToInt32(new string(c), 16))
            .Concat(Enumerable.Repeat(0, _spriteCount))
            .Take(_spriteCount)
            .ToArray();
        _flagData = (int[])_originalFlags.Clone();

        _spritesheetData = new Color[_spriteSheetWidth * _spriteSheetHeight];
        _mapData = new int[_mapWidth * _mapHeight];

        Reload();
    }
    
    public void Reload()
    {
        _originalSpritesheetTexture.GetData(_spritesheetData);

        int mapTexWidth = _originalMapTexture.Width;
        Color[] mapPixels = new Color[_originalMapTexture.Width * _originalMapTexture.Height];
        _originalMapTexture.GetData(mapPixels);

        int spritesPerRow = _spriteSheetWidth / SPRITE_WIDTH;

        for (int cy = 0; cy < _mapHeight; cy++)
        {
            for (int cx = 0; cx < _mapWidth; cx++)
            {
                _mapData[cx + cy * _mapWidth] = FindMatchingSpriteIndex(
                    mapPixels, mapTexWidth,
                    cx * SPRITE_WIDTH, cy * SPRITE_HEIGHT,
                    spritesPerRow);
            }
        }

        Array.Copy(_originalFlags, _flagData, _spriteCount);
        SpritesheetVersion++;
        MapVersion++;
    }

    private int FindMatchingSpriteIndex(
        Color[] mapPixels, int mapTexWidth,
        int tileOriginX, int tileOriginY,
        int spritesPerRow)
    {
        for (int si = 0; si < _spriteCount; si++)
        {
            int spriteOriginX = (si % spritesPerRow) * SPRITE_WIDTH;
            int spriteOriginY = (si / spritesPerRow) * SPRITE_HEIGHT;

            bool match = true;
            for (int py = 0; py < SPRITE_HEIGHT && match; py++)
            {
                for (int px = 0; px < SPRITE_WIDTH && match; px++)
                {
                    Color mapColor    = mapPixels[(tileOriginX + px) + (tileOriginY + py) * mapTexWidth];
                    Color spriteColor = _spritesheetData[(spriteOriginX + px) + (spriteOriginY + py) * _spriteSheetWidth];
                    if (mapColor != spriteColor)
                    {
                        match = false;
                        break;
                    }
                }
                if (!match) break;
            }

            if (match) return si;
        }

        return 0;
    }

    public Color GetSpritePixel(int x, int y)
    {
        if (x < 0 || x >= _spriteSheetWidth || y < 0 || y >= _spriteSheetHeight)
            return Color.Black;

        return _spritesheetData[x + y * _spriteSheetWidth];
    }

    public void SetSpritePixel(int x, int y, Color color)
    {
        if (x < 0 || x >= _spriteSheetWidth || y < 0 || y >= _spriteSheetHeight)
            return;

        _spritesheetData[x + y * _spriteSheetWidth] = color;
        SpritesheetVersion++;
    }

    public int GetFlag(int n)
    {
        if (n < 0 || n >= _spriteCount) return 0;
        return _flagData[n];
    }

    public bool GetFlag(int n, int bit)
    {
        if (n < 0 || n >= _spriteCount) return false;
        if (bit < 0 || bit > 7) return false;
        return (_flagData[n] >> bit & 1) == 1;
    }

    public void SetFlag(int n, int value)
    {
        if (n < 0 || n >= _spriteCount) return;
        _flagData[n] = value & 0xFF;
    }

    public void SetFlag(int n, int bit, bool v)
    {
        if (n < 0 || n >= _spriteCount) return;
        if (bit < 0 || bit > 7) return;
        if (v)
            _flagData[n] |= 1 << bit;
        else
            _flagData[n] &= ~(1 << bit);
    }

    public int GetMapTile(int x, int y)
    {
        if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight)
            return 0;

        return _mapData[x + y * _mapWidth];
    }

    public void SetMapTile(int x, int y, int spriteNumber)
    {
        if (x < 0 || x >= _mapWidth || y < 0 || y >= _mapHeight )
            return;

        if (spriteNumber < 0 || spriteNumber >= _spriteCount)
            return;

        _mapData[x + y * _mapWidth] = spriteNumber;
        MapVersion++;
    }

    public void MapToSpritesheet(
        int mapStartX, int mapStartY,
        int mapEndX, int mapEndY,
        int spriteDestX, int spriteDestY)
    {
        int cellCount = (mapEndX - mapStartX) * (mapEndY - mapStartY);

        for (int i = 0; i < cellCount; i++)
        {
            int tile = _mapData[mapStartX + (mapStartY * _mapWidth) + i];
            Color color = Pico8.Palette.ElementAt(tile % 16).Key;
            _spritesheetData[spriteDestX + (spriteDestY * _spriteSheetWidth) + i] = color;
        }
    }

    #region TEXTURE GENERATION

    public int SpriteSheetWidth => _spriteSheetWidth;
    public int SpriteSheetHeight => _spriteSheetHeight;
    public int SpritesPerRow => _spriteSheetWidth / SPRITE_WIDTH;
    public int SpriteCount => _spriteCount;
    public int MapWidth => _mapWidth;
    public int MapHeight => _mapHeight;

    public Rectangle GetSpriteSourceRect(int spriteIndex, int widthSprites = 1, int heightSprites = 1)
    {
        int spritesPerRow = _spriteSheetWidth / SPRITE_WIDTH;
        return new Rectangle(
            (spriteIndex % spritesPerRow) * SPRITE_WIDTH,
            (spriteIndex / spritesPerRow) * SPRITE_HEIGHT,
            widthSprites * SPRITE_WIDTH,
            heightSprites * SPRITE_HEIGHT);
    }

    public Texture2D GetSpritesheetTexture()
    {
        if (_cachedSpritesheetTexture is null ||
            SpritesheetVersion != _cachedSpriteVersion ||
            _paletteManager.PaletteVersion != _cachedPaletteVersion)
        {
            _cachedSpritesheetTexture ??= new Texture2D(_graphicsDevice, _spriteSheetWidth, _spriteSheetHeight);
            Color[] applied = new Color[_spritesheetData.Length];
            for (int i = 0; i < _spritesheetData.Length; i++)
                applied[i] = _paletteManager.PaletteMap.TryGetValue(_spritesheetData[i], out Color mapped) ? mapped : _spritesheetData[i];
            _cachedSpritesheetTexture.SetData(applied);
            _cachedSpriteVersion = SpritesheetVersion;
            _cachedPaletteVersion = _paletteManager.PaletteVersion;
        }
        return _cachedSpritesheetTexture;
    }

    public Texture2D GetMapRegionTexture(int mapX, int mapY, int mapW, int mapH, int flags = 0)
    {
        var key = (mapX, mapY, mapW, mapH, flags);
        int texW = mapW * SPRITE_WIDTH;
        int texH = mapH * SPRITE_HEIGHT;
        int spritesPerRow = _spriteSheetWidth / SPRITE_WIDTH;

        if (_mapTextureCache.TryGetValue(key, out var cached) &&
            cached.sprV == SpritesheetVersion &&
            cached.mapV == MapVersion &&
            cached.palV == _paletteManager.PaletteVersion)
        {
            return cached.tex;
        }

        Color[] pixels = new Color[texW * texH];
        for (int cy = 0; cy < mapH; cy++)
        {
            for (int cx = 0; cx < mapW; cx++)
            {
                int spriteIndex = GetMapTile(mapX + cx, mapY + cy);
                bool drawSprite = flags == 0 || (GetFlag(spriteIndex) & flags) != 0;
                int spriteOriginX = (spriteIndex % spritesPerRow) * SPRITE_WIDTH;
                int spriteOriginY = (spriteIndex / spritesPerRow) * SPRITE_HEIGHT;
                for (int py = 0; py < SPRITE_HEIGHT; py++)
                {
                    for (int px = 0; px < SPRITE_WIDTH; px++)
                    {
                        int destIdx = (cx * SPRITE_WIDTH + px) + (cy * SPRITE_HEIGHT + py) * texW;
                        if (!drawSprite)
                        {
                            pixels[destIdx] = Color.Transparent;
                            continue;
                        }
                        Color src = _spritesheetData[(spriteOriginX + px) + (spriteOriginY + py) * _spriteSheetWidth];
                        pixels[destIdx] = _paletteManager.PaletteMap.TryGetValue(src, out Color mapped) ? mapped : src;
                    }
                }
            }
        }

        if (_mapTextureCache.TryGetValue(key, out var existing))
        {
            existing.tex.SetData(pixels);
            _mapTextureCache[key] = (existing.tex, SpritesheetVersion, MapVersion, _paletteManager.PaletteVersion);
            return existing.tex;
        }

        Texture2D tex = new Texture2D(_graphicsDevice, texW, texH);
        tex.SetData(pixels);
        _mapTextureCache[key] = (tex, SpritesheetVersion, MapVersion, _paletteManager.PaletteVersion);
        return tex;
    }

    public void Dispose()
    {
        _cachedSpritesheetTexture?.Dispose();
        foreach (var entry in _mapTextureCache.Values)
            entry.tex.Dispose();
        _mapTextureCache.Clear();
    }

    #endregion
}