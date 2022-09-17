namespace Celeste.Mod.IsaGrabBag
{
    public class IsaSaveData : EverestModuleSaveData
    {
        public bool[] ColorWall { get; set; } = new bool[] { true, true, true };
        public bool[] ColorWallSave { get; set; } = new bool[] { true, true, true };
        public bool?[] Variants { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null };
        public bool?[] Variants_Save { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null };
        public bool[] Variants_Default { get; set; } = new bool[] { false, false, false, false, false, false, false, false, false, false };
    }
}