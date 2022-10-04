using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IsaGrabBag {
    public static class Utils {
        public static Image SetRenderPosition(this Image image, Vector2 position) {
            image.RenderPosition = position;
            return image;
        }

        public static Image SetRotation(this Image image, float rotation) {
            image.Rotation = rotation;
            return image;
        }

        public static float Mod(float x, float m) {
            return ((x % m) + m) % m;
        }
    }
}
