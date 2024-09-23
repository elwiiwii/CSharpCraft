using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCraft.Pico8;

namespace CSharpCraft.Pcraft
{
    public class PcraftSingleplayer(Pico8Functions p8) : PcraftBase(p8)
    {
        public override string GameModeName => "pcraft";

        public override void Init()
        {
            base.Init();
        }
    }

    public class PcraftRaceplayer(Pico8Functions p8) : PcraftBase(p8)
    {
        public override string GameModeName => "race";

        public override void Init()
        {
            base.Init();
        }
    }
}
