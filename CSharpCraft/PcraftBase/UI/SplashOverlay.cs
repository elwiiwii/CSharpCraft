namespace CSharpCraft.PcraftBase.UI;

internal sealed class SplashOverlay(int spr, string text, string text2)
{
    internal int    Spr   { get; } = spr;
    internal string Text  { get; } = text;
    internal string Text2 { get; } = text2;

    internal static SplashOverlay Main  { get; } = new(spr: 128, text: "by nusan",               text2: "2016");
    internal static SplashOverlay Intro { get; } = new(spr: 136, text: "a storm leaved you",     text2: "on a deserted island");
    internal static SplashOverlay Death { get; } = new(spr: 128, text: "you died",               text2: "alone ...");
    internal static SplashOverlay Win   { get; } = new(spr: 136, text: "you successfully escaped", text2: "from the island");
}
