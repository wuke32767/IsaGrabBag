namespace Celeste.Mod.IsaGrabBag {
    public static class Utils {
        public static float Mod(float x, float m) {
            return ((x % m) + m) % m;
        }
    }
}
