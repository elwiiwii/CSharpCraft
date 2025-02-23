using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCraft.Pico8;

namespace CSharpCraft.Pcraft
{
    public class PcraftSingleplayer : PcraftBase
    {
        public override string SceneName => "pcraft";

        public override void Init(Pico8Functions pico8)
        {
            base.Init(pico8);
        }
    }

    public class PcraftRaceplayer(Pico8Functions p8) : PcraftBase
    {
        public override string SceneName => "race";

        public override void Init(Pico8Functions pico8)
        {
            base.Init(pico8);
        }
    }
}
