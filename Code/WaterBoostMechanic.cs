using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IsaGrabBag {
    [CustomEntity("isaBag/waterBoost")]
    public class WaterBoostMechanic : Entity {
        public WaterBoostMechanic(EntityData _data, Vector2 offset)
            : base(_data.Position + offset) {
            GrabBagModule.GrabBagMeta.WaterBoost = _data.Bool("boostEnabled");
        }
    }
}
