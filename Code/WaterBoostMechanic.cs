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

    public class WaterBoostHandler : Component {
        public WaterBoostHandler()
            : base(active: true, visible: false) {
        }

        public override void Update() {
            if (GrabBagModule.GrabBagMeta.WaterBoost && Entity is Player player && player.Collidable) {
                Vector2 posOffset = player.Position + (player.Speed * Engine.DeltaTime * 2);
                bool isInWater = player.CollideCheck<Water>(posOffset) || player.CollideCheck<Water>(posOffset + (Vector2.UnitY * -8f));
                if (!isInWater && player.StateMachine.State == Player.StSwim && (player.Speed.Y < 0 || Input.MoveY.Value == -1 || Input.Jump.Check)) {
                    player.Speed.Y = (Input.MoveY.Value == -1 || Input.Jump.Check) ? -110 : 0;
                    if (player.Speed.Y < -1) {
                        player.Speed.X *= 1.1f;
                    }
                }
            }
        }
    }
}
