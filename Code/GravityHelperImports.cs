using Monocle;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.IsaGrabBag {
    public static class GravityHelperImports {
        [ModImportName("GravityHelper")]
        public static class Interop {
            public static Func<bool> IsPlayerInverted;
            public static Action<int, float> SetPlayerGravity;
            public static Action BeginOverride;
            public static Action EndOverride;
            public static Func<Action<Player, int, float>, Component> CreatePlayerGravityListener;
        }

        public static bool HasInterop() => Interop.IsPlayerInverted != null;
        public static bool IsPlayerInverted() => Interop.IsPlayerInverted?.Invoke() ?? false;
        public static void SetPlayerInverted(bool inverted) => Interop.SetPlayerGravity?.Invoke(inverted ? 1 : 0, 0f);
        public static void BeginOverride() => Interop.BeginOverride?.Invoke();
        public static void EndOverride() => Interop.EndOverride?.Invoke();
        public static Component CreatePlayerGravityListener(Action<Player, int, float> gravityChanged) =>
            Interop.CreatePlayerGravityListener?.Invoke(gravityChanged);
    }
}