using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8.Graphics;

public class GraphicsManager
{
    private readonly SpriteBatch _batch;
    private (int X, int Y) _cameraOffset;
    private readonly GraphicsDeviceManager _graphics;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly PaletteManager _paletteManager;
    private readonly Texture2D _pixel;
    private readonly Dictionary<string, Texture2D> _textureDictionary;
    private readonly GameWindow _window;
    private readonly Dictionary<string, FontAtlas> _fontAtlases = new(StringComparer.OrdinalIgnoreCase);
    private readonly PrintDrawState _printState = new();

    public GraphicsManager(
        SpriteBatch batch,
        GraphicsDeviceManager graphics,
        GraphicsDevice graphicsDevice,
        PaletteManager paletteManager,
        Texture2D pixel,
        Dictionary<string, Texture2D> textureDictionary,
        GameWindow window)
    {
        _cameraOffset = (0, 0);
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _paletteManager = paletteManager ?? throw new ArgumentNullException(nameof(paletteManager));
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
        _textureDictionary = textureDictionary ?? throw new ArgumentNullException(nameof(textureDictionary));
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _batch = batch ?? throw new ArgumentNullException(nameof(batch));
    }

    public SpriteBatch Batch => _batch;
    public (int X, int Y) CameraOffset => _cameraOffset;
    public GraphicsDeviceManager Graphics => _graphics;
    public GraphicsDevice GraphicsDevice => _graphicsDevice;
    public PaletteManager PaletteManager => _paletteManager;
    public Texture2D Pixel => _pixel;
    public Dictionary<string, Texture2D> TextureDictionary => _textureDictionary;
    public GameWindow Window => _window;


    #region DRAWING PRIMITIVES

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Circ
    /// </summary>
    public void Circ(int centerX, int centerY, int radius, Color color)
    {
        if (radius <= 0) return;

        centerX -= CameraOffset.X;
        centerY -= CameraOffset.Y;
        _drawState.SetGetDrawColor(color, out _);

        for (int dx = radius, dy = 0, error = 0; dx >= dy; )
        {
            DrawPixel(centerX + dx, centerY + dy, color, 1, 1);
            DrawPixel(centerX + dy, centerY + dx, color, 1, 1);
            DrawPixel(centerX - dy, centerY + dx, color, 1, 1);
            DrawPixel(centerX - dx, centerY + dy, color, 1, 1);
            DrawPixel(centerX - dx, centerY - dy, color, 1, 1);
            DrawPixel(centerX - dy, centerY - dx, color, 1, 1);
            DrawPixel(centerX + dy, centerY - dx, color, 1, 1);
            DrawPixel(centerX + dx, centerY - dy, color, 1, 1);

            dy += 1;
            if (error < radius - 1)
                error += 1 + 2 * dy;
            else
            {
                dx -= 1;
                error += 1 + 2 * (dy - dx);
            }
        }
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Circfill
    /// </summary>
    public void Circfill(int centerX, int centerY, int radius, Color color)
    {
        if (radius <= 0) return;

        centerX -= CameraOffset.X;
        centerY -= CameraOffset.Y;

        for (int dx = radius, dy = 0, error = 0; dx >= dy; )
        {
            DrawPixel(centerX - dx, centerY + dy, color, 2 * dx + 1, 1);
            DrawPixel(centerX - dx, centerY - dy, color, 2 * dx + 1, 1);
            DrawPixel(centerX - dy, centerY + dx, color, 2 * dy + 1, 1);
            DrawPixel(centerX - dy, centerY - dx, color, 2 * dy + 1, 1);

            dy += 1;
            if (error < radius - 1)
                error += 1 + 2 * dy;
            else
            {
                dx -= 1;
                error += 1 + 2 * (dy - dx);
            }
        }
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Cls
    /// </summary>
    public void Cls(Color color)
    {
        GraphicsDevice.Clear(color);
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Pset
    /// </summary>
    public void Pset(int x, int y, Color color)
    {
        DrawPixel(x, y, color, 1, 1);
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Rect
    /// </summary>
    public void Rect(int xLeft, int yTop, int xRight, int yBottom, Color color)
    {
        xLeft -= CameraOffset.X;
        yTop -= CameraOffset.Y;
        int width = xRight - xLeft + 1;
        int height = yBottom - yTop + 1;

        DrawPixel(xLeft, yTop, color, width, 1);
        DrawPixel(xLeft, yTop + height - 1, color, width, 1);
        DrawPixel(xLeft, yTop, color, 1, height);
        DrawPixel(xLeft + width - 1, yTop, color, 1, height);
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Rectfill
    /// </summary>
    public void Rectfill(int xLeft, int yTop, int xRight, int yBottom, Color color)
    {
        xLeft -= CameraOffset.X;
        yTop -= CameraOffset.Y;
        int width = xRight - xLeft + 1;
        int height = yBottom - yTop + 1;

        DrawPixel(xLeft, yTop, color, width, height);
    }

    /// <summary>
    /// https://www.lexaloffle.com/bbs/?tid=150992
    /// </summary>
    public void Rrect(int x, int y, int width, int height, int radius, Color color)
    {
        if (width <= 0 || height <= 0) return;

        x -= CameraOffset.X;
        y -= CameraOffset.Y;

        int r = Math.Clamp(radius, 0, Math.Min(width, height) / 2);

        //if (r == 0)
        //{
        //    Rect(x, y, x + width - 1, y + height - 1, color);
        //    return;
        //}

        // Straight edges (shortened by r on each end)
        int edgeW = width - 2 * r;
        int edgeH = height - 2 * r;
        if (edgeW > 0)
        {
            DrawPixel(x + r, y, color, edgeW, 1);               // Top
            DrawPixel(x + r, y + height - 1, color, edgeW, 1);  // Bottom
        }
        if (edgeH > 0)
        {
            DrawPixel(x, y + r, color, 1, edgeH);               // Left
            DrawPixel(x + width - 1, y + r, color, 1, edgeH);   // Right
        }

        // Corner arc centers (one quadrant per corner via midpoint algorithm)
        int tlx = x + r, tly = y + r;
        int trx = x + width - 1 - r, try_ = y + r;
        int blx = x + r, bly = y + height - 1 - r;
        int brx = x + width - 1 - r, bry = y + height - 1 - r;

        for (int dx = r, dy = 0, error = 0; dx >= dy; )
        {
            DrawPixel(tlx - dx, tly - dy, color);   // Top-left
            DrawPixel(tlx - dy, tly - dx, color);
            DrawPixel(trx + dx, try_ - dy, color);  // Top-right
            DrawPixel(trx + dy, try_ - dx, color);
            DrawPixel(blx - dx, bly + dy, color);   // Bottom-left
            DrawPixel(blx - dy, bly + dx, color);
            DrawPixel(brx + dx, bry + dy, color);   // Bottom-right
            DrawPixel(brx + dy, bry + dx, color);

            dy++;
            if (error < r - 1) error += 1 + 2 * dy;
            else { dx--; error += 1 + 2 * (dy - dx); }
        }
    }

    /// <summary>
    /// https://www.lexaloffle.com/bbs/?tid=150992
    /// </summary>
    public void Rrectfill(int x, int y, int width, int height, int radius, Color color)
    {
        if (width <= 0 || height <= 0) return;

        x -= CameraOffset.X;
        y -= CameraOffset.Y;

        int r = Math.Clamp(radius, 0, Math.Min(width, height) / 2);

        //if (r == 0)
        //{
        //    Rectfill(x, y, x + width - 1, y + height - 1, color);
        //    return;
        //}

        // Middle band — full width, between the two corner bands
        int edgeH = height - 2 * r;
        if (edgeH > 0)
            DrawPixel(x, y + r, color, width, edgeH);

        // Corner regions filled via horizontal scanlines using midpoint algorithm.
        // tlx/trx: x centers of left/right corner columns
        // tly/bly: y centers of top/bottom corner rows
        int tlx = x + r;
        int trx = x + width - 1 - r;
        int tly = y + r;
        int bly = y + height - 1 - r;

        for (int dx = r, dy = 0, error = 0; dx >= dy; )
        {
            // Scanline at vertical offset dy from corner center
            // Spans from (tlx - dx) to (trx + dx)
            int left1 = tlx - dx;
            int w1 = trx + dx - left1 + 1; // = width - 2*r + 2*dx
            DrawPixel(left1, tly - dy, color, w1, 1);  // Top band
            if (bly + dy != tly - dy)
                DrawPixel(left1, bly + dy, color, w1, 1);  // Bottom band

            // Scanline at vertical offset dx from corner center (other octant)
            if (dx != dy)
            {
                int left2 = tlx - dy;
                int w2 = trx + dy - left2 + 1; // = width - 2*r + 2*dy
                DrawPixel(left2, tly - dx, color, w2, 1);  // Top band
                if (bly + dx != tly - dx)
                    DrawPixel(left2, bly + dx, color, w2, 1);  // Bottom band
            }

            dy++;
            if (error < r - 1) error += 1 + 2 * dy;
            else { dx--; error += 1 + 2 * (dy - dx); }
        }
    }

    #endregion

    #region PALETTE OPERATIONS

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Pal
    /// </summary>
    public void Pal()
    {
        _paletteManager.ResetPalette();
    }

    public void Pal(Color key, Color value)
    {
        _paletteManager.SetPalette(key, value);
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Palt
    /// </summary>
    public void Palt()
    {
        _paletteManager.ResetTransparency();
    }

    public void Palt(Color key, int opacity)
    {
        _paletteManager.SetTransparency(key, opacity);
    }

    #endregion

    #region CAMERA STATE

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Camera
    /// </summary>
    public void Camera(int x = 0, int y = 0)
    {
        _cameraOffset = (x, y);
    }

    #endregion

    #region RENDERING OPERATIONS

    /// <summary>
    /// Registers a pixel-font atlas for use with <see cref="Print"/>.
    /// The built-in name <c>"P8SCII"</c> is used as the fallback when the requested font
    /// is not found.
    /// </summary>
    /// <param name="name">Case-insensitive font name, e.g. <c>"P8SCII"</c> or <c>"BigFont"</c>.</param>
    /// <param name="texture">Atlas texture (white glyphs on transparent background).</param>
    /// <param name="charWidth">Width of one standard glyph cell in the atlas, in pixels.</param>
    /// <param name="charHeight">Height of every glyph cell in the atlas, in pixels.</param>
    /// <param name="extCharWidth">
    /// Width of one extended-range glyph cell in the atlas, in pixels.
    /// Pass ≤ 0 to use <c>charWidth * 2</c> (the P8SCII default).
    /// </param>
    public void RegisterFont(string name, Texture2D texture, int charWidth, int charHeight, int extCharWidth = -1)
        => _fontAtlases[name] = new FontAtlas(texture, charWidth, charHeight, extCharWidth);

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Print
    /// </summary>
    public void Print(string text, int x, int y, Color color, string font)
    {
        if (!_fontAtlases.TryGetValue(font, out var atlas) &&
            !_fontAtlases.TryGetValue("P8SCII", out atlas))
            return;

        _printState.CursorX       = x - CameraOffset.X;
        _printState.CursorY       = y - CameraOffset.Y;
        _printState.ForegroundColor = color;
        _printState.Reset();

        for (int i = 0; i < text.Length; )
        {
            char ch = text[i++];
            switch (ch)
            {
                case '\x00': // jump to home position
                    _printState.CursorX = _printState.HomeX;
                    _printState.CursorY = _printState.HomeY;
                    break;
                case '\x01': // \* — toggle solid background
                    _printState.SolidBackground = !_printState.SolidBackground;
                    break;
                case '\x02': // toggle wide
                    _printState.Wide = !_printState.Wide;
                    break;
                case '\x03': // toggle tall
                    _printState.Tall = !_printState.Tall;
                    break;
                case '\x04': // set foreground colour — 1 param byte
                    if (i < text.Length) _printState.ForegroundColor = LookupColor(text[i++]);
                    break;
                case '\x05': // set background colour — 1 param byte
                    if (i < text.Length) _printState.BackgroundColor = LookupColor(text[i++]);
                    break;
                case '\x06': // set tab width — 1 param byte
                    if (i < text.Length) _printState.TabWidth = Math.Max(1, P8Val(text[i++]));
                    break;
                case '\x08': // \b — backspace: move cursor left by one glyph
                {
                    int bw = atlas.GetCharWidth('\x20') * (_printState.Wide ? 2 : 1);
                    _printState.CursorX -= bw + (_printState.Padding ? 1 : 0);
                    break;
                }
                case '\x09': // \t — tab: snap to next tab stop
                {
                    int cw    = atlas.CharWidth * (_printState.Wide ? 2 : 1);
                    int tabPx = _printState.TabWidth * cw;
                    int relX  = _printState.CursorX - _printState.HomeX;
                    _printState.CursorX = tabPx > 0
                        ? _printState.HomeX + ((relX / tabPx) + 1) * tabPx
                        : _printState.CursorX;
                    break;
                }
                case '\x0a': // \n — newline
                {
                    int lh = atlas.CharHeight * (_printState.Tall ? 2 : 1);
                    _printState.CursorX  = _printState.HomeX;
                    _printState.CursorY += lh + 1;
                    break;
                }
                case '\x0b': // \v — cursor up one line
                {
                    int lh = atlas.CharHeight * (_printState.Tall ? 2 : 1);
                    _printState.CursorY -= lh + 1;
                    break;
                }
                case '\x0c': // \f — clear screen (no-op: call Cls() separately)
                    break;
                case '\x0d': // \r — carriage return: return to home X
                    _printState.CursorX = _printState.HomeX;
                    break;
                case '\x0e': // \^ — two-byte caret sequence
                    if (i < text.Length)
                        ProcessCaretCode(text[i++], text, ref i, ref atlas);
                    break;
                case '\x0f': // revert to default P8SCII font
                    if (_fontAtlases.TryGetValue("P8SCII", out var p8Font))
                        atlas = p8Font;
                    break;
                default:
                    DrawGlyph(ch, atlas);
                    break;
            }
        }
    }

    private void ProcessCaretCode(char code, string text, ref int i, ref FontAtlas atlas)
    {
        switch (code)
        {
            case 'd': // delay — deferred (typewriter pattern); skip 1 param byte
                if (i < text.Length) i++;
                break;
            case 'j': // jump: absolute cursor position relative to home
                if (i + 1 < text.Length)
                {
                    _printState.CursorX = _printState.HomeX + P8CursorVal(text[i++]) * atlas.CharWidth;
                    _printState.CursorY = _printState.HomeY + P8CursorVal(text[i++]) * atlas.CharHeight;
                }
                break;
            case 'r': // define custom character — skip char-index byte + 8 data bytes
                i += Math.Min(9, text.Length - i);
                break;
            case 's': // set right border (in char-width units, relative to home X)
                if (i < text.Length)
                    _printState.RightBorder = _printState.HomeX + P8Val(text[i++]) * atlas.CharWidth;
                break;
            case 'u': // underline toggle
                _printState.Underline = !_printState.Underline;
                break;
            case 'x': // set cursor X absolute (relative to home)
                if (i < text.Length)
                    _printState.CursorX = _printState.HomeX + P8CursorVal(text[i++]) * atlas.CharWidth;
                break;
            case 'y': // set cursor Y absolute (relative to home)
                if (i < text.Length)
                    _printState.CursorY = _printState.HomeY + P8CursorVal(text[i++]) * atlas.CharHeight;
                break;
            case 'w': // wide toggle
                _printState.Wide = !_printState.Wide;
                break;
            case 'h': // home X + advance one line (\r\n equivalent)
            {
                int lh = atlas.CharHeight * (_printState.Tall ? 2 : 1);
                _printState.CursorX  = _printState.HomeX;
                _printState.CursorY += lh + 1;
                break;
            }
            case '=': // stripey — requires shader; no-op
                break;
            case 'p': // pinball — deferred; no-op
                break;
            case 'i': // invert toggle
                _printState.Invert = !_printState.Invert;
                break;
            case 'b': // solid background toggle
                _printState.SolidBackground = !_printState.SolidBackground;
                break;
            case '#': // set outline colour — 1 param byte
                if (i < text.Length)
                    _printState.OutlineColor = LookupColor(text[i++]);
                break;
            case 'o': // padding toggle
                _printState.Padding = !_printState.Padding;
                break;
        }
    }

    private void DrawGlyph(char ch, FontAtlas atlas)
    {
        int charW = atlas.GetCharWidth(ch) * (_printState.Wide ? 2 : 1);
        int charH = atlas.CharHeight        * (_printState.Tall ? 2 : 1);

        // Auto-wrap at right border
        if (_printState.RightBorder < int.MaxValue &&
            _printState.CursorX + charW > _printState.RightBorder)
        {
            _printState.CursorX  = _printState.HomeX;
            _printState.CursorY += charH + 1;
        }

        var srcRect  = atlas.GetSourceRect(ch);
        var destRect = new Rectangle(_printState.CursorX, _printState.CursorY, charW, charH);

        Color fg = _printState.Invert ? _printState.BackgroundColor : _printState.ForegroundColor;
        Color bg = _printState.Invert ? _printState.ForegroundColor : _printState.BackgroundColor;

        // Solid background box
        if (_printState.SolidBackground || _printState.Invert)
            _batch.Draw(_pixel, destRect, bg);

        // Outline: render glyph in outline colour at up to 8 surrounding offsets
        if (_printState.OutlineColor.HasValue)
        {
            Color oc = _printState.OutlineColor.Value;
            ReadOnlySpan<(int X, int Y)> dirs =
            [
                (-1, -1), (0, -1), (1, -1),
                (-1,  0),          (1,  0),
                (-1,  1), (0,  1), (1,  1)
            ];
            for (int bit = 0; bit < 8; bit++)
            {
                if ((_printState.OutlineMask & (1 << bit)) != 0)
                {
                    var (ox, oy) = dirs[bit];
                    _batch.Draw(atlas.Texture,
                        new Rectangle(destRect.X + ox, destRect.Y + oy, charW, charH),
                        srcRect, oc);
                }
            }
        }

        // Main glyph
        _batch.Draw(atlas.Texture, destRect, srcRect, fg);

        // Underline: 1-pixel line at the bottom of the glyph cell
        if (_printState.Underline)
            _batch.Draw(_pixel,
                new Rectangle(_printState.CursorX, _printState.CursorY + charH - 1, charW, 1), fg);

        // Advance cursor horizontally
        _printState.CursorX += charW + (_printState.Padding ? 1 : 0);
    }

    // ── P8SCII helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Decodes a P8SCII parameter byte to a non-negative integer:
    /// <c>'0'–'9'</c> → 0–9; <c>'a'–'z'</c> → 10–35.
    /// </summary>
    private static int P8Val(char ch)
        => ch >= '0' && ch <= '9' ? ch - '0' : ch - 'a' + 10;

    /// <summary>
    /// Decodes a P8SCII cursor-offset parameter byte (range −16 … +19).
    /// </summary>
    private static int P8CursorVal(char ch) => P8Val(ch) - 16;

    /// <summary>
    /// Returns the palette colour whose index is encoded in <paramref name="ch"/>
    /// via <see cref="P8Val"/>.
    /// </summary>
    private Color LookupColor(char ch)
    {
        int idx = P8Val(ch) & 0x0F;
        return idx < _paletteManager.PaletteMap.Count
            ? _paletteManager.PaletteMap.ElementAt(idx).Key
            : Color.White;
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Spr
    /// </summary>
    public void Spr(int index, int x, int y, int width = 1, int height = 1, bool flipX = false, bool flipY = false)
    {
        
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Sspr
    /// </summary>
    public void Sspr(int sourceX, int sourceY, int sourceWidth, int sourceHeight, int destX, int destY,
            int destWidth, int destHeight, bool flipX = false, bool flipY = false)
    {
        
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Map
    /// </summary>
    public void Map(int sourceX, int sourceY, int destX, int destY, int sourceWidth, int sourceHeight, int flags = 0)
    {
        
    }

    /// <summary>
    /// https://pico-8.fandom.com/wiki/Line
    /// </summary>
    public void Line(int x0, int y0, int x1, int y1, Color c)
    {
        
    }

    #endregion

    #region LOW-LEVEL DRAWING

    public void DrawTexture(string textureName, double x, double y, Color color,
            double scaleX = 1, double scaleY = 1, bool flipX = false, bool flipY = false)
    {
        
    }

    public void DrawPixel(double x, double y, Color color, double scaleX = 1,
            double scaleY = 1, bool flipX = false, bool flipY = false)
    {
        _batch.Draw(_pixel,
            new Rectangle((int)x, (int)y, Math.Max(1, (int)scaleX), Math.Max(1, (int)scaleY)),
            color);
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, double thickness)
    {
        
    }

    public void DrawCirc(Vector2 center, double radius, Color color, double thickness, int segments)
    {
        
    }

    public void DrawRect(Vector2 topLeft, double width, double height, Color color, double thickness)
    {
        
    }

    #endregion
}
