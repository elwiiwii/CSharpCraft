namespace CSharpCraft.Pico8;

public static class Font
{

    private static readonly int[,] space = new int[,]
    {
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 },
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

    private static readonly int[,] quote = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 }
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

    private static readonly int[,] ampersand = new int[,]
    {
        { 1, 1, 0 },
        { 1, 1, 0 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] apostrophe = new int[,]
    {
        { 0, 1, 0 },
        { 1, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 }
    };

    private static readonly int[,] leftRoundBracket = new int[,]
    {
        { 0, 1, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 0, 1, 0 }
    };

    private static readonly int[,] rightRoundBracket = new int[,]
    {
        { 0, 1, 0 },
        { 0, 0, 1 },
        { 0, 0, 1 },
        { 0, 0, 1 },
        { 0, 1, 0 }
    };

    private static readonly int[,] asterisk = new int[,]
    {
        { 1, 0, 1 },
        { 0, 1, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 1, 0, 1 }
    };

    private static readonly int[,] plus = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 0, 0 }
    };

    private static readonly int[,] comma = new int[,]
    {
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 1, 0, 0 }
    };

    private static readonly int[,] dash = new int[,]
    {
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 0, 0 },
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

    private static readonly int[,] forwardSlash = new int[,]
    {
        { 0, 0, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 1, 0, 0 }
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

    private static readonly int[,] colon = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 0 }
    };

    private static readonly int[,] semicolon = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 1, 0, 0 }
    };

    private static readonly int[,] leftAngleBracket = new int[,]
    {
        { 0, 0, 1 },
        { 0, 1, 0 },
        { 1, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 1 }
    };

    private static readonly int[,] equals = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 0, 0 }
    };

    private static readonly int[,] rightAngleBracket = new int[,]
    {
        { 1, 0, 0 },
        { 0, 1, 0 },
        { 0, 0, 1 },
        { 0, 1, 0 },
        { 1, 0, 0 }
    };

    private static readonly int[,] question = new int[,]
    {
        { 1, 1, 1 },
        { 0, 0, 1 },
        { 0, 1, 1 },
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

    private static readonly int[,] a = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] b = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 0 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] c = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 1 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 0, 1, 1 }
    };

    private static readonly int[,] d = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 0 }
    };

    private static readonly int[,] e = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 1, 1, 0 },
        { 1, 0, 0 },
        { 0, 1, 1 }
    };

    private static readonly int[,] f = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 1, 1, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 }
    };

    private static readonly int[,] g = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 1 },
        { 1, 0, 0 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] h = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] i = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 1, 1, 1 }
    };

    private static readonly int[,] j = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 1, 1, 0 }
    };

    private static readonly int[,] k = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] l = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 0, 1, 1 }
    };

    private static readonly int[,] m = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] n = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] o = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 0 }
    };

    private static readonly int[,] p = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 0, 0 }
    };

    private static readonly int[,] q = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 1, 0, 1 },
        { 1, 1, 0 },
        { 0, 1, 1 }
    };

    private static readonly int[,] r = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 1, 0 },
        { 1, 0, 1 }
    };

    private static readonly int[,] s = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 1 },
        { 1, 0, 0 },
        { 0, 0, 1 },
        { 1, 1, 0 }
    };

    private static readonly int[,] t = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 }
    };

    private static readonly int[,] u = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 0, 1, 1 }
    };

    private static readonly int[,] v = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 0, 1, 0 }
    };

    private static readonly int[,] w = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] x = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 1, 0, 1 }
    };

    private static readonly int[,] y = new int[,]
    {
        { 0, 0, 0 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 0, 0, 1 },
        { 1, 1, 0 }
    };

    private static readonly int[,] z = new int[,]
    {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 0, 1 },
        { 1, 0, 0 },
        { 1, 1, 1 }
    };

    private static readonly int[,] leftSquareBracket = new int[,]
    {
        { 1, 1, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 1, 0 }
    };

    private static readonly int[,] backslash = new int[,]
    {
        { 1, 0, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 0, 1 }
    };

    private static readonly int[,] rightSquareBracket = new int[,]
    {
        { 0, 1, 1 },
        { 0, 0, 1 },
        { 0, 0, 1 },
        { 0, 0, 1 },
        { 0, 1, 1 }
    };

    private static readonly int[,] power = new int[,]
    {
        { 0, 1, 0 },
        { 1, 0, 1 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 }
    };

    private static readonly int[,] underscore = new int[,]
    {
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 1, 1, 1 }
    };

    private static readonly int[,] backtick = new int[,]
    {
        { 0, 1, 0 },
        { 0, 0, 1 },
        { 0, 0, 0 },
        { 0, 0, 0 },
        { 0, 0, 0 }
    };

    private static readonly int[,] A = new int[,]
    {
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] B = new int[,]
    {
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] C = new int[,]
    {
        { 0, 1, 1 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 0, 1, 1 }
    };

    private static readonly int[,] D = new int[,]
    {
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] E = new int[,]
    {
        { 1, 1, 1 },
        { 1, 0, 0 },
        { 1, 1, 0 },
        { 1, 0, 0 },
        { 1, 1, 1 }
    };

    private static readonly int[,] F = new int[,]
    {
        { 1, 1, 1 },
        { 1, 0, 0 },
        { 1, 1, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 }
    };

    private static readonly int[,] G = new int[,]
    {
        { 0, 1, 1 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] H = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] I = new int[,]
    {
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 1, 1, 1 }
    };

    private static readonly int[,] J = new int[,]
    {
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 1, 1, 0 }
    };

    private static readonly int[,] K = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] L = new int[,]
    {
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 0, 0 },
        { 1, 1, 1 }
    };

    private static readonly int[,] M = new int[,]
    {
        { 1, 1, 1 },
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] N = new int[,]
    {
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] O = new int[,]
    {
        { 0, 1, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 0 }
    };

    private static readonly int[,] P = new int[,]
    {
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 0, 0 },
        { 1, 0, 0 }
    };

    private static readonly int[,] Q = new int[,]
    {
        { 0, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 0 },
        { 0, 1, 1 }
    };

    private static readonly int[,] R = new int[,]
    {
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] S = new int[,]
    {
        { 0, 1, 1 },
        { 1, 0, 0 },
        { 1, 1, 1 },
        { 0, 0, 1 },
        { 1, 1, 0 }
    };

    private static readonly int[,] T = new int[,]
    {
        { 1, 1, 1 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 }
    };

    private static readonly int[,] U = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 0, 1, 1 }
    };

    private static readonly int[,] V = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 0, 1, 0 }
    };

    private static readonly int[,] W = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] X = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 0, 1, 0 },
        { 1, 0, 1 },
        { 1, 0, 1 }
    };

    private static readonly int[,] Y = new int[,]
    {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 },
        { 0, 0, 1 },
        { 1, 1, 1 }
    };

    private static readonly int[,] Z = new int[,]
    {
        { 1, 1, 1 },
        { 0, 0, 1 },
        { 0, 1, 0 },
        { 1, 0, 0 },
        { 1, 1, 1 }
    };

    private static readonly int[,] leftCurlyBracket = new int[,]
    {
        { 0, 1, 1 },
        { 0, 1, 0 },
        { 1, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 1 }
    };

    private static readonly int[,] pipe = new int[,]
    {
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 }
    };

    private static readonly int[,] rightCurlyBracket = new int[,]
    {
        { 1, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 1 },
        { 0, 1, 0 },
        { 1, 1, 0 }
    };

    private static readonly int[,] tilda = new int[,]
    {
        { 0, 0, 0 },
        { 0, 0, 1 },
        { 1, 1, 1 },
        { 1, 0, 0 },
        { 0, 0, 0 }
    };

    private static readonly int[,] flower = new int[,]
    {
        { 0, 0, 0 },
        { 0, 1, 0 },
        { 1, 0, 1 },
        { 0, 1, 0 },
        { 0, 0, 0 }
    };

    public static readonly Dictionary<char, int[,]> chars = new()
    {
        { ' ', space },
        { '!', exclamation },
        { '"', quote },
        { '#', hash },
        { '$', dollar },
        { '%', percent },
        { '&', ampersand },
        { '\'', apostrophe },
        { '(', leftRoundBracket },
        { ')', rightRoundBracket },
        { '*', asterisk },
        { '+', plus },
        { ',', comma },
        { '-', dash },
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
        { ';', semicolon },
        { '<', leftAngleBracket },
        { '=', equals },
        { '>', rightAngleBracket },
        { '?', question },
        { '@', at },
        { 'A', a },
        { 'B', b },
        { 'C', c },
        { 'D', d },
        { 'E', e },
        { 'F', f },
        { 'G', g },
        { 'H', h },
        { 'I', i },
        { 'J', j },
        { 'K', k },
        { 'L', l },
        { 'M', m },
        { 'N', n },
        { 'O', o },
        { 'P', p },
        { 'Q', q },
        { 'R', r },
        { 'S', s },
        { 'T', t },
        { 'U', u },
        { 'V', v },
        { 'W', w },
        { 'X', x },
        { 'Y', y },
        { 'Z', z },
        { '[', leftSquareBracket },
        { '\\', backslash },
        { ']', rightSquareBracket },
        { '^', power },
        { '_', underscore },
        { '`', backtick },
        { 'a', A },
        { 'b', B },
        { 'c', C },
        { 'd', D },
        { 'e', E },
        { 'f', F },
        { 'g', G },
        { 'h', H },
        { 'i', I },
        { 'j', J },
        { 'k', K },
        { 'l', L },
        { 'm', M },
        { 'n', N },
        { 'o', O },
        { 'p', P },
        { 'q', Q },
        { 'r', R },
        { 's', S },
        { 't', T },
        { 'u', U },
        { 'v', V },
        { 'w', W },
        { 'x', X },
        { 'y', Y },
        { 'z', Z },
        { '{', leftCurlyBracket },
        { '|', pipe },
        { '}', rightCurlyBracket },
        { '~', tilda },
        //{ 'flower', flower }
    };
}
