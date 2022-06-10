using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.IsaGrabBag
{
    public class WaterBoostMechanic : Entity
    {
        public WaterBoostMechanic(EntityData _data)
        {
            GrabBagModule.GrabBagMeta.WaterBoost = _data.Bool("boostEnabled");
        }
    }
}
