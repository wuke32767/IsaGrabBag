using Celeste.Mod.Meta;

namespace Celeste.Mod.IsaGrabBag {
    public class GrabBagWrapperMeta : IMeta {
        public GrabBagMeta IsaGrabBag { get; set; }
    }

    public class GrabBagMeta {
        public bool WaterBoost { get; set; }
        public bool RoundDreamSpinner { get; set; }

        public static GrabBagMeta Default(AreaKey area) {
            return new GrabBagMeta() {
                WaterBoost = area.LevelSet.StartsWith("SpringCollab2020"),
                RoundDreamSpinner = false
            };
        }
    }
}
