using System;
using System.Reflection;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.IsaGrabBag
{
    public class ZipLineRender : Entity
    {
        private readonly ZipLine zipInst;

        private Sprite sprite;

        public ZipLineRender(ZipLine instance)
        {
            zipInst = instance;

            sprite = GrabBagModule.sprites.Create("zipline");
            sprite.Play("idle");

            sprite.JustifyOrigin(new Vector2(0.5f, 0.25f));

            Add(sprite);

            Depth = 500;
        }
        public override void Update()
        {
            base.Update();
        }

        List<Rectangle> renderRects = new List<Rectangle>();

        public override void Render()
        {
            renderRects.Clear();

            Position = zipInst.Position;

            Rectangle tempRect = new Rectangle((int)zipInst.Left, (int)zipInst.Y + 1, (int)(zipInst.Right - zipInst.Left), 0);
            tempRect.Inflate(8, 1);


            renderRects.Add(tempRect);

            int left = tempRect.Left, right = tempRect.Right;

            renderRects.Add(new Rectangle(left - 2, (int)Y - 2, 2, 6));
            renderRects.Add(new Rectangle(right, (int)Y - 2, 2, 6));

            foreach (var rect in renderRects)
            {
                Rectangle r = rect;
                r.Inflate(1, 0);
                Draw.Rect(r, Color.Black);
                r.Inflate(-1, 1);
                Draw.Rect(r, Color.Black);
            }
            foreach (var rect in renderRects)
            {
                Draw.Rect(rect, Color.SlateGray);
            }

            base.Render();

        }
    }
    public class ZipLine : Entity
    {
        private static void MoveEntityTo(Actor ent, Vector2 position)
        {
            ent.MoveToX(position.X);
            ent.MoveToY(position.Y);
        }

        private const int STATE_NORMAL = 0;
        private const float ZIP_SPEED = 120f;
        private const float ZIP_ACCEL = 190f;
        private const float ZIP_TURN = 250f;

        private static ZipLine currentGrabbed;

        private float left, right, height;

        public float Left { get { return left; } }
        public float Right { get { return right; } }

        private float speed;
        private bool grabbed;

        private static float ziplineBuffer;

        private Sprite sprite;

        public static bool GrabbingCoroutine { get { return currentGrabbed != null && !currentGrabbed.grabbed; } }

        public static void OnPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            ziplineBuffer = Calc.Approach(ziplineBuffer, 0, Engine.DeltaTime);

            if (!Input.GrabCheck)
                ziplineBuffer = 0;
        }

        public static void ZipLineBegin()
        {
            var self = GrabBagModule.playerInstance;
            self.Ducking = false;

            self.Speed.Y = 0;

        }
        public static void ZipLineEnd()
        {
            currentGrabbed.grabbed = false;
            currentGrabbed = null;
            ziplineBuffer = 0.35f;
        }
        public static int ZipLineUpdate()
        {
            var self = GrabBagModule.playerInstance;

            if (!currentGrabbed.grabbed)
                return GrabBagModule.ZipLineState;

            currentGrabbed.speed = self.Speed.X;

            if (Math.Abs(self.LiftSpeed.X) <= Math.Abs(self.Speed.X))
            {
                self.LiftSpeed = self.Speed;
                self.LiftSpeedGraceTime = 0.15f;
            }

            if (Math.Sign(Input.Aim.Value.X) == -Math.Sign(self.Speed.X))
                self.Speed.X = Calc.Approach(self.Speed.X, Input.Aim.Value.X * ZIP_SPEED, ZIP_TURN * Engine.DeltaTime);
            else if (Math.Abs(self.Speed.X) <= ZIP_SPEED || Math.Sign(Input.Aim.Value.X) != Math.Sign(self.Speed.X))
                self.Speed.X = Calc.Approach(self.Speed.X, Input.Aim.Value.X * ZIP_SPEED, ZIP_ACCEL * Engine.DeltaTime);

            if (!Input.GrabCheck || self.Stamina <= 0)
            {
                return STATE_NORMAL;
            }
            if (Input.Jump.Pressed)
            {
                Input.Jump.ConsumePress();

                self.Stamina -= 110f / 8f;

                self.Speed.X *= 0.1f;

                self.Jump(false, true);

                self.LiftSpeed *= 0.4f;
                //self.ResetLiftSpeed();


                currentGrabbed.speed = Calc.Approach(currentGrabbed.speed, 0, 20);

                return STATE_NORMAL;
            }
            if (self.CanDash)
            {

                self.StartDash();
                return 2;
            }

            self.Stamina -= 5 * Engine.DeltaTime;

            return GrabBagModule.ZipLineState;
        }
        public static IEnumerator ZipLineCoroutine()
        {
            var self = GrabBagModule.playerInstance;
            var speed = self.Speed;
            self.Speed = Vector2.Zero;
            currentGrabbed.speed = 0;

            self.Sprite.Play("pickup");

            self.Play("event:/char/madeline/crystaltheo_lift", null, 0f);

            Vector2 playerLerp = new Vector2((self.X + currentGrabbed.X) / 2f, currentGrabbed.Y + 22);

            playerLerp.X = MathHelper.Clamp(playerLerp.X, currentGrabbed.left, currentGrabbed.right);
            Vector2 zipLerp = new Vector2(playerLerp.X, currentGrabbed.Y);

            Vector2 playerInit = self.Position,
                zipInit = currentGrabbed.Position;

            var tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.07f, true);

            while (tween.Active)
            {
                tween.Update();

                MoveEntityTo(self, Vector2.Lerp(playerInit, playerLerp, tween.Percent));
                currentGrabbed.Position = Vector2.Lerp(zipInit, zipLerp, tween.Percent);

                yield return null;
            }

            currentGrabbed.grabbed = true;

            self.Speed = speed;

            MoveEntityTo(self, playerLerp);
            currentGrabbed.Position = zipLerp;

            yield break;
        }

        public ZipLine(EntityData _data, Vector2 offset) : base(_data.Position + offset)
        {
            left = X;
            right = X;
            foreach (var node in _data.Nodes)
            {
                left = Math.Min(node.X + offset.X, left);
                right = Math.Max(node.X + offset.X, right);
            }

            height = (_data.Position + offset).Y;

            Collider = new Hitbox(20, 16, -10, 1);

            currentGrabbed = null;

            Depth = -500;

            sprite = GrabBagModule.sprites.Create("zipline");
            sprite.Play("idle");

            sprite.JustifyOrigin(new Vector2(0.5f, 0.25f));

            Add(sprite);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(new ZipLineRender(this));
        }

        public override void Update()
        {
            base.Update();
            var player = GrabBagModule.playerInstance;

            if (player == null || player.Dead)
                return;

            if (grabbed)
            {
                if (player.Speed.X > 20)
                {
                    player.LiftSpeed = player.Speed;
                    player.LiftSpeedGraceTime = 0.2f;
                }
                if ((player.CenterX > right || player.CenterX < left))
                {

                    player.Speed.X = 0;
                }
                player.CenterX = MathHelper.Clamp(player.CenterX, left, right);
                Position.X = player.CenterX;

                Position.Y = height;
            }
            else
            {

                if (currentGrabbed == null && player != null && !player.Dead && player.CanUnDuck && Input.GrabCheck && ziplineBuffer == 0)
                {
                    PropertyInfo info = typeof(Player).GetProperty("IsTired", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (player.CollideCheck(this) && (!(bool)info.GetValue(player)))
                    {
                        currentGrabbed = this;

                        player.StateMachine.State = GrabBagModule.ZipLineState;

                    }
                }

                Position.X += speed * Engine.DeltaTime;

                Position.X = MathHelper.Clamp(Position.X, left, right);
                Position.Y = height;
            }


        }



        public override void Render()
        {
            if (grabbed)
            {
                sprite.Visible = true;
                sprite.Play(GrabBagModule.playerInstance.Facing == Facings.Left ? "held_l" : "held_r");
            }
            else
            {
                sprite.Visible = false;
            }

            base.Render();
        }
    }
}
