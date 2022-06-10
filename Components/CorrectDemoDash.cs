using System;
using Celeste.Mod;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Monocle;
using Celeste;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.IsaGrabBag.Components {
	public class CorrectDemoDash : Component {
		public CorrectDemoDash() : base(true, false) {

            onGround = typeof(Player).GetField("onGround", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private const float WAIT_TIMER = -1;

        private static int newTrack;

        private Player maddy;

        private FieldInfo onGround;
        IEnumerator dashinitial;
        bool inDemoSpot, wasDemoSpot;

        public override void Added(Entity entity) {
            maddy = entity as Player;
            base.Added(entity);

            var coroutines = typeof(StateMachine).GetField("coroutines", BindingFlags.Instance | BindingFlags.NonPublic);

            Func<IEnumerator>[] list = coroutines.GetValue(maddy.StateMachine) as Func<IEnumerator>[];

            dashinitial = list[2].Invoke();
            list[2] = new Func<IEnumerator>(ContinueDashing);
        }

        private IEnumerator ContinueDashing() {

            inDemoSpot = false;
            wasDemoSpot = false;

            dashinitial = typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(maddy, null) as IEnumerator;

            while (dashinitial.MoveNext()) {
                if (dashinitial.Current is float) {

                    float value = (float)dashinitial.Current;

                    yield return value;


                    if (wasDemoSpot && maddy.Speed.X != 0) {
                        while (inDemoSpot) {
                            if (Input.DashPressed)
                                break;
                            yield return null;
                        }

                        if (!Input.DashPressed)
                            yield return null;
                    }
                }
                else
                    yield return dashinitial.Current;
            }

            yield break;
        }


        public override void Update() {

            base.Update();

            inDemoSpot = false;

            const int testHeight = 4;

            var top = maddy.Collider.Bounds;
            try {
                if (maddy.StateMachine == 2 && top.Height <= 6) {
                    int height = top.Height;

                    top.Height = testHeight;
                    top.Y = (int)maddy.Top - top.Height;// testHeight;

                    var bottom = top;
                    bottom.Y += height + testHeight;

                    var spikeTest = top;
                    spikeTest.Y -= 8;
                    spikeTest.Height += 8;

                    if ((Scene.CollideCheck<Solid>(top) || Scene.CollideCheck<CrystalStaticSpinner>(spikeTest)) &&
                        (Scene.CollideCheck<Solid>(bottom))) {
                        inDemoSpot = true;
                        wasDemoSpot = true;

                        maddy.StartJumpGraceTime();
                        maddy.RefillDash();
                        maddy.RefillStamina();
                        onGround.SetValue(maddy, true);
                        maddy.Speed.Y = 0f;
                        maddy.Position.Y = Calc.Snap(maddy.Position.Y, 8);
                    }

                }
            }
            catch { }

        }
    }
}
