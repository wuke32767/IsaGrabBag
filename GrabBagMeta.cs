using Celeste.Mod.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.IsaGrabBag {
	public class GrabBagWrapperMeta : IMeta {

		public GrabBagMeta IsaGrabBag { get; set; }
	}

	public class GrabBagMeta {

		public static GrabBagMeta Default(AreaKey area) {
			return new GrabBagMeta() {
				WaterBoost = area.LevelSet.StartsWith("SpringCollab2020"),
				RoundDreamSpinner = !area.LevelSet.StartsWith("SpringCollab2020"),
				ReplaceDashWith = "nothing",
			};
		}

		public bool WaterBoost { get; set; }
		public bool RoundDreamSpinner { get; set; }
		public string ReplaceDashWith { get; set; }
	}
}
