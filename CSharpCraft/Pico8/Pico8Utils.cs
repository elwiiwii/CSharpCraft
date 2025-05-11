using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Pico8;

public static class Pico8Utils
{
    public static Color[] ImageToColorArray(Pico8Functions p8, string filename)
    {
        Texture2D texture = p8.TextureDictionary[filename];
        
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

    public static Color[] DataToColorArray(Pico8Functions p8, string s, int n)
    {
        Color[] val = new Color[s.Length / n];
        for (int i = 0; i < s.Length / n; i++)
        {
            int index = Convert.ToInt32($"0x{s.Substring(i * n, n)}", 16);
            val[i] = p8.Colors[index % 16];
        }

        return val;
    }

    public static Color[] MapDataToColorArray(Pico8Functions p8, string s)
    {
        Color[] val = new Color[s.Length / 2];
        for (int i = 0; i < s.Length / 2; i++)
        {
            char[] chunk = s.Substring(i * 2, 2).ToCharArray();
            int index = (chunk[0] - 35) * 91 + chunk[1] - 35;
            val[i] = p8.Colors[index % 16];
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

    public static Texture2D CreateTextureFromSpriteData(Pico8Functions p8, Color[] spriteData, int spriteX, int spriteY, int spriteWidth, int spriteHeight)
    {
        Texture2D texture = new(p8.GraphicsDevice, spriteWidth, spriteHeight);

        Color[] colorData = new Color[spriteWidth * spriteHeight];

        for (int i = spriteX + spriteY * 128, j = 0; j < spriteWidth * spriteHeight; i++, j++)
        {
            Color col = p8.PalColors.FindAll(x => x.C0 == spriteData[i]).Count > 0 ? p8.PalColors.First(x => x.C0 == spriteData[i]).C1 : spriteData[i];
            if (p8.PalColors.FindAll(x => x.C0 == spriteData[i]).Count > 0 && p8.PalColors.First(x => x.C0 == spriteData[i]).Trans == false)
            {
                colorData[j] = p8.PalColors.First(x => x.C0 == spriteData[i]).C1;
            }
            else if (p8.PalColors.FindAll(x => x.C0 == spriteData[i]).Count <= 0)
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


    public static bool Ptn(Pico8Functions p8, int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
    {
        return i switch
        {
            0 => IsBindingDown(0, p8.OptionsFile.Kbm_Left.Bind1) ||
                 IsBindingDown(0, p8.OptionsFile.Kbm_Left.Bind2) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Left.Bind1) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Left.Bind2),

            1 => IsBindingDown(0, p8.OptionsFile.Kbm_Right.Bind1) ||
                 IsBindingDown(0, p8.OptionsFile.Kbm_Right.Bind2) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Right.Bind1) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Right.Bind2),

            2 => IsBindingDown(0, p8.OptionsFile.Kbm_Up.Bind1) ||
                 IsBindingDown(0, p8.OptionsFile.Kbm_Up.Bind2) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Up.Bind1) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Up.Bind2),

            3 => IsBindingDown(0, p8.OptionsFile.Kbm_Down.Bind1) ||
                 IsBindingDown(0, p8.OptionsFile.Kbm_Down.Bind2) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Down.Bind1) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Down.Bind2),

            4 => IsBindingDown(0, p8.OptionsFile.Kbm_Menu.Bind1) ||
                 IsBindingDown(0, p8.OptionsFile.Kbm_Menu.Bind2) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Menu.Bind1) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Menu.Bind2),

            5 => IsBindingDown(0, p8.OptionsFile.Kbm_Use.Bind1) ||
                 IsBindingDown(0, p8.OptionsFile.Kbm_Use.Bind2) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Use.Bind1) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Use.Bind2),

            6 => IsBindingDown(0, p8.OptionsFile.Kbm_Pause.Bind1) ||
                 IsBindingDown(0, p8.OptionsFile.Kbm_Pause.Bind2) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Pause.Bind1) ||
                 IsBindingDown(1, p8.OptionsFile.Con_Pause.Bind2),
            _ => false,
        };
    }

}
