namespace CSharpCraft.MainMenu;

internal class MainMenuObj
{
    internal void Update()
    {
        
    }

    internal void Draw()
    {
        Pico8.Cls(17);

        Pico8.Sspr(0, 0, 19, 17, 39, 11, 19, 17);
        Pico8.Sspr(38, 0, 19, 17, 83, 11, 19, 17);
        Pico8.Sspr(0, 17, 15, 17, 19, 47, 15, 17);
        Pico8.Sspr(30, 17, 23, 17, 59, 47, 23, 17);
        Pico8.Sspr(76, 0, 15, 17, 107, 47, 15, 17);
        Pico8.Sspr(0, 34, 15, 17, 41, 83, 15, 17);
        Pico8.Sspr(30, 34, 15, 17, 85, 83, 15, 17);

        Pico8.Sspr(0, 51, 11, 14, 6, 114, 11, 14);
        Pico8.Sspr(22, 51, 14, 11, 22, 119, 14, 11);
        Pico8.Sspr(50, 51, 13, 13, 43, 121, 13, 13);
        Pico8.Sspr(0, 65, 17, 13, 62, 122, 17, 13);
        Pico8.Sspr(34, 65, 10, 13, 85, 121, 10, 13);
        Pico8.Sspr(54, 65, 14, 11, 101, 119, 14, 11);
        Pico8.Sspr(60, 34, 14, 14, 121, 114, 14, 14);
    }
}