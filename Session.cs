using YamlDotNet.Serialization;

namespace Celeste.Mod.IsaGrabBag
{
    public class IsaSession : EverestModuleSession
    {
        [YamlIgnore]
        public bool[] ColorWall { get; set; } = new bool[] { true, true, true };
        public bool[] ColorWallSave { get; set; } = new bool[] { true, true, true };

        [YamlIgnore]
        public bool?[] Variants { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null, null };
        [YamlIgnore]
        public bool?[] Variants_Default { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null, null };
        public bool?[] Variants_Save { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null, null };
    }
}