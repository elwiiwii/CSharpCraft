using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft
{
    public static class SpritesOLD
    {
        private static int[,] zero = new int[,]
        {
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1, -1, -1, -1 }
        };

        private static int[,] one = new int[,]
        {
            { -1, -1,  0,  0,  0,  0, -1, -1 },
            { -1,  0, 10, 10, 10, 10,  0, -1 },
            {  0, 10,  0, 10, 10,  0, 10,  0 },
            {  0, 10, 10, 10, 10, 10, 10,  0 },
            {  0, 10,  0, 10, 10,  0, 10,  0 },
            {  0, 10, 10,  0,  0, 10, 10,  0 },
            { -1,  0, 10, 10, 10, 10,  0, -1 },
            { -1, -1,  0,  0,  0,  0, -1, -1 }
        };

        public static Dictionary<char, int[,]> sprites = new Dictionary<char, int[,]>
        {
            { '0', zero },
            { '1', one },
        };
    }
}
