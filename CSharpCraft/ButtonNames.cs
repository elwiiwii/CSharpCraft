using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft
{
    public class ButtonNames
    {
        public static Dictionary<string, string> buttonNames = new()
        {
            { "DPadUp", "dpad ^" },
            { "DPadDown", "dpad v" },
            { "DPadLeft", "dpad <" },
            { "DPadRight", "dpad >" },
            { "Start", "start" },
            { "Back", "select" },
            { "LeftStick", "l3" },
            { "RightStick", "r3" },
            { "LeftShoulder", "l1" },
            { "RightShoulder", "r1" },
            { "BigButton", "power" },
            { "A", "a" },
            { "B", "b" },
            { "X", "x" },
            { "Y", "y" },
            { "LeftThumbstickLeft", "ls <" },
            { "RightTrigger", "r2" },
            { "LeftTrigger", "l2" },
            { "RightThumbstickUp", "rs ^" },
            { "RightThumbstickDown", "rs v" },
            { "RightThumbstickRight", "rs >" },
            { "RightThumbstickLeft", "rs <" },
            { "LeftThumbstickUp", "ls ^" },
            { "LeftThumbstickDown", "ls v" },
            { "LeftThumbstickRight", "ls >" },
            { "Misc1EXT", "misc1" },
            { "Paddle1EXT", "padl 1" },
            { "Paddle2EXT", "padl 2" },
            { "Paddle3EXT", "padl 3" },
            { "Paddle4EXT", "padl 4" },
            { "TouchPadEXT", "touch" }
        };

    }
}
