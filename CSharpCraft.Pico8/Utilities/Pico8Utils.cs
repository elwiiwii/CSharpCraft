using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Pico8.Utilities;

public static class Pico8Utils
{
    public static Color[] ImageToColorArray(Dictionary<string, Texture2D> textureDictionary, string filename)
    {
        Texture2D texture = textureDictionary[filename];
        
        if (texture is null)
            throw new ArgumentNullException(nameof(filename));

        if (texture.Format != SurfaceFormat.Color)
            throw new ArgumentException("Texture must use SurfaceFormat.Color");

        if (texture.Width % 8 != 0 && texture.Height % 8 != 0)
            throw new FileLoadException("Texture must be a multiple of 8 in both dimensions");

        Color[] colorArray = new Color[texture.Width * texture.Height];
        texture.GetData(colorArray);
        return colorArray;
    }

    public static Color[] DataToColorArray(List<Color> colors, string s, int n)
    {
        Color[] val = new Color[s.Length / n];
        for (int i = 0; i < s.Length / n; i++)
        {
            int index = Convert.ToInt32($"0x{s.Substring(i * n, n)}", 16);
            val[i] = colors[index % 16];
        }
        return val;
    }

    public static Color[] MapDataToColorArray(List<Color> colors, string s, int n)
    {
        Color[] val = new Color[s.Length / n];
        for (int i = 0; i < s.Length / n; i++)
        {
            char[] chunk = s.Substring(i * n, n).ToCharArray();
            int index = 0;
            for (int j = 0; j < n; j++)
            {
                index += (chunk[j] - 35) * 91 * (n - j);
            }
            val[i] = colors[index % 16];
        }
        return val;
    }

    public static int[] DataToArray(string s, int n)
    {
        int[] val = new int[s.Length / n];
        for (int i = 0; i < s.Length / n; i++)
        {
            val[i] = Convert.ToInt32($"0x{s.Substring(i * n, n)}", 16);
        }
        return val;
    }

    public static int[] MapDataToArray(string s)
    {
        int[] val = new int[s.Length / 2];
        for (int i = 0; i < s.Length / 2; i++)
        {
            char[] chunk = s.Substring(i * 2, 2).ToCharArray();
            int sprNum = (chunk[0] - 35) * 91 + chunk[1] - 35;
            val[i] = sprNum;
        }
        return val;
    }

    public static string MapFlip(string s)
    {
        return string.Concat(
            Enumerable.Range(0, (int)Math.Ceiling(s.Length / 2.0))
                .Select(i => new string(s
                    .Skip(i * 2)
                    .Take(2)
                    .Reverse()
                    .ToArray()))
        );
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.TrimStart('#');
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return new Color(r, g, b);
    }

    public static Texture2D CreateTextureFromSpriteData(
        GraphicsDevice graphicsDevice,
        Color[] spriteData,
        List<PalCol> palColors,
        int spriteX, int spriteY, int spriteWidth, int spriteHeight)
    {
        Texture2D texture = new(graphicsDevice, spriteWidth, spriteHeight);

        Color[] colorData = new Color[spriteWidth * spriteHeight];

        for (int i = spriteX + spriteY * 128, j = 0; j < spriteWidth * spriteHeight; i++, j++)
        {
            Color col = palColors.FindAll(x => x.C0 == spriteData[i]).Count > 0 ? palColors.First(x => x.C0 == spriteData[i]).C1 : spriteData[i];
            if (palColors.FindAll(x => x.C0 == spriteData[i]).Count > 0 && palColors.First(x => x.C0 == spriteData[i]).Trans == false)
            {
                colorData[j] = palColors.First(x => x.C0 == spriteData[i]).C1;
            }
            else if (palColors.FindAll(x => x.C0 == spriteData[i]).Count <= 0)
            {
                colorData[j] = spriteData[i];
            }

            if (j % spriteWidth == spriteWidth - 1) { i += 128 - spriteWidth; }
        }

        texture.SetData(colorData);
        return texture;
    }

    public static bool IsBindingDown(int device, string bind)
    {
        KeyboardState Kbm_state = Keyboard.GetState();
        GamePadState con_state = GamePad.GetState(PlayerIndex.One);

        if (device == 0)
        {
            if (Enum.TryParse(bind, out Keys key))
            {
                return Kbm_state.IsKeyDown(key);
            }

            if (IsMouseButton(bind))
            {
                return true;
            }
        }
        else
        {
            if (Enum.TryParse(bind, out Buttons button))
            {
                return con_state.IsButtonDown(button);
            }
        }

        return false;
    }

    private static bool IsMouseButton(string bind)
    {
        MouseState mouse_state = Mouse.GetState();

        return bind switch
        {
            "LeftButton" => mouse_state.LeftButton == ButtonState.Pressed,
            "RightButton" => mouse_state.RightButton == ButtonState.Pressed,
            "MiddleButton" => mouse_state.MiddleButton == ButtonState.Pressed,
            "XButton1" => mouse_state.XButton1 == ButtonState.Pressed,
            "XButton2" => mouse_state.XButton2 == ButtonState.Pressed,
            _ => false,
        };
    }

    /// <summary>
    /// Physical button test - checks raw input state for a button index.
    /// Receives InputBindings explicitly to avoid coupling to Pico8 static class.
    /// </summary>
    public static bool Ptn(int i, IInputBindingProvider bindings, int p = 0)
    {
        return i switch
        {
            0 => IsBindingDown(0, bindings.KeyboardLeft.Bind1) ||
                 IsBindingDown(0, bindings.KeyboardLeft.Bind2) ||
                 IsBindingDown(1, bindings.ControllerLeft.Bind1) ||
                 IsBindingDown(1, bindings.ControllerLeft.Bind2),

            1 => IsBindingDown(0, bindings.KeyboardRight.Bind1) ||
                 IsBindingDown(0, bindings.KeyboardRight.Bind2) ||
                 IsBindingDown(1, bindings.ControllerRight.Bind1) ||
                 IsBindingDown(1, bindings.ControllerRight.Bind2),

            2 => IsBindingDown(0, bindings.KeyboardUp.Bind1) ||
                 IsBindingDown(0, bindings.KeyboardUp.Bind2) ||
                 IsBindingDown(1, bindings.ControllerUp.Bind1) ||
                 IsBindingDown(1, bindings.ControllerUp.Bind2),

            3 => IsBindingDown(0, bindings.KeyboardDown.Bind1) ||
                 IsBindingDown(0, bindings.KeyboardDown.Bind2) ||
                 IsBindingDown(1, bindings.ControllerDown.Bind1) ||
                 IsBindingDown(1, bindings.ControllerDown.Bind2),

            4 => IsBindingDown(0, bindings.KeyboardMenu.Bind1) ||
                 IsBindingDown(0, bindings.KeyboardMenu.Bind2) ||
                 IsBindingDown(1, bindings.ControllerMenu.Bind1) ||
                 IsBindingDown(1, bindings.ControllerMenu.Bind2),

            5 => IsBindingDown(0, bindings.KeyboardUse.Bind1) ||
                 IsBindingDown(0, bindings.KeyboardUse.Bind2) ||
                 IsBindingDown(1, bindings.ControllerUse.Bind1) ||
                 IsBindingDown(1, bindings.ControllerUse.Bind2),

            6 => IsBindingDown(0, bindings.KeyboardPause.Bind1) ||
                 IsBindingDown(0, bindings.KeyboardPause.Bind2) ||
                 IsBindingDown(1, bindings.ControllerPause.Bind1) ||
                 IsBindingDown(1, bindings.ControllerPause.Bind2),
            _ => false,
        };
    }
}
