using Celeste.Mod.Entities;
using Monocle;

namespace Celeste.Mod.IsaGrabBag {
    [CustomEntity("isaBag/waterBoost")]
    public class WaterBoostMechanic : Entity {
        public WaterBoostMechanic(EntityData _data) {
            GrabBagModule.GrabBagMeta.WaterBoost = _data.Bool("boostEnabled");
        }
    }
}
