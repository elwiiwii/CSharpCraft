using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft
{
    public class ButtonsToString
    {
        public static Dictionary<Buttons, string> buttonsToString = new()
        {
            { Buttons.DPadUp, "DPadUp" },
			{ Buttons.DPadDown, "DPadDown" },
			{ Buttons.DPadLeft, "DPadLeft" },
			{ Buttons.DPadRight, "DPadRight" },
			{ Buttons.Start, "Start" },
			{ Buttons.Back, "Back" },
			{ Buttons.LeftStick, "LeftStick" },
			{ Buttons.RightStick, "RightStick" },
			{ Buttons.LeftShoulder, "LeftShoulder" },
			{ Buttons.RightShoulder, "RightShoulder" },
			{ Buttons.BigButton, "BigButton" },
			{ Buttons.A, "A" },
			{ Buttons.B, "B" },
			{ Buttons.X, "X" },
			{ Buttons.Y, "Y" },
			{ Buttons.LeftThumbstickLeft, "LeftThumbstickLeft" },
			{ Buttons.RightTrigger, "RightTrigger" },
			{ Buttons.LeftTrigger, "LeftTrigger" },
			{ Buttons.RightThumbstickUp, "RightThumbstickUp" },
			{ Buttons.RightThumbstickDown, "RightThumbstickDown" },
			{ Buttons.RightThumbstickRight, "RightThumbstickRight" },
			{ Buttons.RightThumbstickLeft, "RightThumbstickLeft" },
			{ Buttons.LeftThumbstickUp, "LeftThumbstickUp" },
			{ Buttons.LeftThumbstickDown, "LeftThumbstickDown" },
			{ Buttons.LeftThumbstickRight, "LeftThumbstickRight" },
			{ Buttons.Misc1EXT, "Misc1EXT" },
			{ Buttons.Paddle1EXT, "Paddle1EXT" },
			{ Buttons.Paddle2EXT, "Paddle2EXT" },
			{ Buttons.Paddle3EXT, "Paddle3EXT" },
			{ Buttons.Paddle4EXT, "Paddle4EXT" },
			{ Buttons.TouchPadEXT, "TouchPadEXT" }
        };

    }
}
