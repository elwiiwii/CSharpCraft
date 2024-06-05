using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft
{
    public static class Font
    {
        private static int[,] a = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static int[,] b = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] c = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 0, 1, 1 }
        };

        private static int[,] d = new int[,]
        {
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] e = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static int[,] f = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 }
        };

        private static int[,] g = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] h = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static int[,] i = new int[,]
        {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 }
        };

        private static int[,] j = new int[,]
        {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 0 }
        };

        private static int[,] k = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static int[,] l = new int[,]
        {
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static int[,] m = new int[,]
        {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static int[,] n = new int[,]
        {
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static int[,] o = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 }
        };

        private static int[,] p = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 }
        };

        private static int[,] q = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 0, 1, 1 }
        };

        private static int[,] r = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static int[,] s = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 0 }
        };

        private static int[,] t = new int[,]
        {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        };

        private static int[,] u = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 1 }
        };

        private static int[,] v = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 }
        };

        private static int[,] w = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        private static int[,] x = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static int[,] y = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] z = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static int[,] zero = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] one = new int[,]
        {
            { 1, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 }
        };

        private static int[,] two = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static int[,] three = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] four = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 }
        };

        private static int[,] five = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] six = new int[,]
        {
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] seven = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 }
        };

        private static int[,] eight = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] nine = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 }
        };

        private static int[,] tilda = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 0, 0, 0 }
        };

        private static int[,] exclamation = new int[,]
        {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 }
        };

        private static int[,] at = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 0 },
            { 0, 1, 1 }
        };

        private static int[,] hash = new int[,]
        {
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 }
        };

        private static int[,] dollar = new int[,]
        {
            { 1, 1, 1 },
            { 1, 1, 0 },
            { 0, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 }
        };

        private static int[,] percent = new int[,]
        {
            { 1, 0, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 1 }
        };

        private static int[,] power = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        private static int[,] ampersand = new int[,]
        {
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static int[,] asterisk = new int[,]
        {
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 }
        };

        private static int[,] openBracket = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 0, 1, 0 }
        };

        private static int[,] closeBracket = new int[,]
        {
            { 0, 1, 0 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 }
        };

        private static int[,] underscore = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 1, 1, 1 }
        };

        private static int[,] plus = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        private static int[,] minus = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        private static int[,] equals = new int[,]
        {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 }
        };

        private static int[,] question = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 1 },
            { 0, 0, 0 },
            { 0, 1, 0 }
        };

        private static int[,] colon = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        private static int[,] period = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 }
        };

        private static int[,] space = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        public static Dictionary<char, int[,]> chars = new Dictionary<char, int[,]>
        {
            { 'a', a },
            { 'b', b },
            { 'c', c },
            { 'd', d },
            { 'e', e },
            { 'f', f },
            { 'g', g },
            { 'h', h },
            { 'i', i },
            { 'j', j },
            { 'k', k },
            { 'l', l },
            { 'm', m },
            { 'n', n },
            { 'o', o },
            { 'p', p },
            { 'q', q },
            { 'r', r },
            { 's', s },
            { 't', t },
            { 'u', u },
            { 'v', v },
            { 'w', w },
            { 'x', x },
            { 'y', y },
            { 'z', z },
            { '0', zero },
            { '1', one },
            { '2', two },
            { '3', three },
            { '4', four },
            { '5', five },
            { '6', six },
            { '7', seven },
            { '8', eight },
            { '9', nine },
            { '~', tilda },
            { '!', exclamation },
            { '@', at },
            { '#', hash },
            { '$', dollar },
            { '%', percent },
            { '^', power },
            { '&', ampersand },
            { '*', asterisk },
            { '(', openBracket },
            { ')', closeBracket },
            { '_', underscore },
            { '+', plus },
            { '-', minus },
            { '=', equals },
            { '?', question },
            { ':', colon },
            { '.', period },
            { ' ', space }
        };
    }
}
