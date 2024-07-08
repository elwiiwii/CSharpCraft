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
        private static readonly int[,] a = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] b = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] c = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 0, 1, 1 }
        };

        private static readonly int[,] d = new int[,]
        {
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] e = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static readonly int[,] f = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 }
        };

        private static readonly int[,] g = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] h = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] i = new int[,]
        {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 }
        };

        private static readonly int[,] j = new int[,]
        {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 0 }
        };

        private static readonly int[,] k = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] l = new int[,]
        {
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static readonly int[,] m = new int[,]
        {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] n = new int[,]
        {
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] o = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 }
        };

        private static readonly int[,] p = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 0, 0 }
        };

        private static readonly int[,] q = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 0, 1, 1 }
        };

        private static readonly int[,] r = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] s = new int[,]
        {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 0 }
        };

        private static readonly int[,] t = new int[,]
        {
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 }
        };

        private static readonly int[,] u = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 1 }
        };

        private static readonly int[,] v = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 }
        };

        private static readonly int[,] w = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] x = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] y = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] z = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static readonly int[,] zero = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] one = new int[,]
        {
            { 1, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 }
        };

        private static readonly int[,] two = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 }
        };

        private static readonly int[,] three = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] four = new int[,]
        {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 }
        };

        private static readonly int[,] five = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] six = new int[,]
        {
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] seven = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 }
        };

        private static readonly int[,] eight = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] nine = new int[,]
        {
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 }
        };

        private static readonly int[,] tilda = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 0 },
            { 0, 0, 0 }
        };

        private static readonly int[,] exclamation = new int[,]
        {
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 }
        };

        private static readonly int[,] at = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 1, 0, 0 },
            { 0, 1, 1 }
        };

        private static readonly int[,] hash = new int[,]
        {
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 }
        };

        private static readonly int[,] dollar = new int[,]
        {
            { 1, 1, 1 },
            { 1, 1, 0 },
            { 0, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 }
        };

        private static readonly int[,] percent = new int[,]
        {
            { 1, 0, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 1 }
        };

        private static readonly int[,] power = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        private static readonly int[,] ampersand = new int[,]
        {
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 }
        };

        private static readonly int[,] asterisk = new int[,]
        {
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 }
        };

        private static readonly int[,] openBracket = new int[,]
        {
            { 0, 1, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 0, 1, 0 }
        };

        private static readonly int[,] closeBracket = new int[,]
        {
            { 0, 1, 0 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 0, 1 },
            { 0, 1, 0 }
        };

        private static readonly int[,] underscore = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 1, 1, 1 }
        };

        private static readonly int[,] plus = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        private static readonly int[,] minus = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        private static readonly int[,] equals = new int[,]
        {
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 }
        };

        private static readonly int[,] question = new int[,]
        {
            { 1, 1, 1 },
            { 0, 0, 1 },
            { 0, 1, 1 },
            { 0, 0, 0 },
            { 0, 1, 0 }
        };

        private static readonly int[,] colon = new int[,]
        {
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 },
            { 0, 0, 0 }
        };

        private static readonly int[,] period = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 1, 0 }
        };

        private static readonly int[,] space = new int[,]
        {
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 },
            { 0, 0, 0 }
        };

        private static readonly int[,] forwardSlash = new int[,]
        {
            { 0, 0, 1 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 0, 1, 0 },
            { 1, 0, 0 }
        };

        public static readonly Dictionary<char, int[,]> chars = new()
        {
            { ' ', space },
            { '!', exclamation },
            //{ '"', quote },
            { '#', hash },
            { '$', dollar },
            { '%', percent },
            { '&', ampersand },
            //{ "'", apostrophe },
            { '(', openBracket },
            { ')', closeBracket },
            { '*', asterisk },
            { '+', plus },
            //{ ',', comma },
            { '-', minus },
            { '.', period },
            { '/', forwardSlash },
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
            { ':', colon },
            //{ ';', semicolon },

            //{ '`', backtick },
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
            
            { '~', tilda },
            
            { '@', at },
            
            
            
            { '^', power },
            
            
            
            { '_', underscore },
            
            
            { '=', equals },
            { '?', question }
            
            
            
        };
    }
}
