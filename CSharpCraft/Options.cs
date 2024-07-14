using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCraft
{
    public class Options(Pico8Functions p8) : IGameMode
    {

        public string GameModeName { get => "options"; }

        private readonly Pico8Functions p8 = p8;

        public void Init()
        {

        }

        public void Update()
        {

        }

        public void Draw()
        {
            p8.Cls();

            p8.Print("optoins", 1, 1, 8);
        }


    }
}
