using YamlDotNet.Serialization;

namespace Celeste.Mod.IsaGrabBag
{
    public class IsaSession : EverestModuleSession
    {
        public bool?[] Variants_Save { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null, null };
        public bool[] ColorWallState { get; set; } = new bool[] { false, false, false };
    }
}