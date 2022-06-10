using YamlDotNet.Serialization;

namespace Celeste.Mod.IsaGrabBag
{
    public class IsaSession : EverestModuleSession
    {
        [YamlIgnore]
        public bool?[] Variants { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null, null };
        [YamlIgnore]
        public bool[] Variants_Default { get; set; } = new bool[] { false, false, false, false, false, false, false, false, false, false, false };
        public bool?[] Variants_Save { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null, null };
    }
}