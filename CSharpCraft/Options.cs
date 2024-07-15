
namespace CSharpCraft
{
    public class Options(Pico8Functions p8) : IGameMode
    {

        public string GameModeName { get => "options"; }

        private readonly Pico8Functions p8 = p8;

        private void Printc(string t, int x, int y, double c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        public void Init()
        {

        }

        public void Update()
        {

        }

        public void Draw()
        {
            p8.Cls();

            Printc("optoins", 64, 2, 8);
            Printc("controls", 40, 9, 6);
            Printc("graphics", 88, 9, 6);
        }


    }
}
