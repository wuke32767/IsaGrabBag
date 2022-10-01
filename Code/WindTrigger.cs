using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.IsaGrabBag {
    [CustomEntity("isaBag/coreWindTrigger", "CoreHeatWindTrigger")]
    public class CoreWindTrigger : Trigger {
        public WindController.Patterns windHot, windCold;
        private static bool current;

        public CoreWindTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            windHot = data.Enum("patternHot", WindController.Patterns.Up);
            windCold = data.Enum("patternCold", WindController.Patterns.Down);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            WindController.Patterns currentWind = current ? windHot : windCold;
            SetWind(Scene, currentWind);
        }
        public override void OnStay(Player player) {
            base.OnStay(player);

            if (SceneAs<Level>().CoreMode == Session.CoreModes.Hot != current) {
                current = !current;
                WindController.Patterns currentWind = current ? windHot : windCold;
                SetWind(Scene, currentWind);
            }
        }

        public static void SetWind(Monocle.Scene scene, WindController.Patterns currentWind) {
            WindController wind = scene.Entities.FindFirst<WindController>();
            if (wind == null) {
                wind = new WindController(currentWind);
                scene.Add(wind);
                return;
            }

            wind.SetPattern(WindController.Patterns.None);
            wind.SetPattern(currentWind);
        }
    }
}
