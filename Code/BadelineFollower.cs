using Monocle;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System;

namespace Celeste.Mod.IsaGrabBag
{
    public class BadelineFollower : Entity
    {
        public const string SESSION_FLAG = "has_badeline_follower";

        private static BadelineBoost booster;
        private static bool firstBoost = false;
        private static bool boosting = false;

        static Coroutine LookForBubble;

        public static bool CheckBooster(Level lvl, bool onTransition)
        {
            if (lvl == null)
                return false;
            if (!lvl.Session.GetFlag(SESSION_FLAG))
                return false;

            var player = GrabBagModule.playerInstance;

            booster = null;

            List<BadelineBoost> boosters = lvl.Entities.FindAll<BadelineBoost>();

            float distTemp = float.MaxValue;
            if (boosters.Count > 0)
            {
                if (player != null)
                {
                    Vector2 position = lvl.Session.RespawnPoint == null ? player.Position : lvl.Session.RespawnPoint.Value;
                    float min = float.MaxValue;
                    foreach (BadelineBoost b in boosters)
                    {
                        b.Visible = false;
                        distTemp = Vector2.Distance(b.Position, position);
                        if (distTemp < min)
                        {
                            min = distTemp;
                            if (booster != null)
                                booster.RemoveSelf();
                            booster = b;
                        }
                        else
                            b.RemoveSelf();
                    }

                    booster.Collidable = false;
                    booster.Visible = true;

                    if (lvl.Session.GetFlag(SESSION_FLAG))
                    {
                        booster.Get<PlayerCollider>().OnCollide = NewBoostMechanic;
                        booster.Visible = false;

                        firstBoost = true;
                        booster.Position = BadelineFollower.instance.Position;
                        booster.Add(new Coroutine(NewBoost()));

                        if (!lvl.Session.Level.Contains("_sl"))
                            booster.Add(new Coroutine(Skip()));

                        return true;
                    }
                    else
                    {
                        booster.RemoveSelf();
                    }
                }
                else
                {
                    foreach (BadelineBoost b in boosters)
                        b.RemoveSelf();
                }
            }
            return false;
        }
        private static IEnumerator Skip()
        {
            var player = GrabBagModule.playerInstance;

            FieldInfo index = typeof(BadelineBoost).GetField("nodeIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo travelling = typeof(BadelineBoost).GetField("travelling", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo skip = typeof(BadelineBoost).GetMethod("Skip", BindingFlags.Instance | BindingFlags.NonPublic);

            Vector2[] nodes = typeof(BadelineBoost).GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(booster) as Vector2[];

            int value;
            float distNow, distNext;

            yield return 1;

            while ((value = (int)index.GetValue(booster)) < nodes.Length - 1)
            {
                while (boosting)
                {
                    yield return null;
                    continue;
                }

                distNow = Vector2.Distance(nodes[value], player.Position);
                distNext = Vector2.Distance(nodes[value + 1], player.Position);
                if (distNow > distNext * 1.3f)
                {
                    index.SetValue(booster, value);
                    skip.Invoke(booster, new object[0]);
                    boosting = true;
                    do
                    {
                        boosting = (bool)travelling.GetValue(booster);
                        yield return null;
                    }
                    while (boosting);
                }

                yield return null;
            }


            yield break;
        }

        private static IEnumerator SearchForBadeline(Level level)
        {
            bool exit = false;
            while (!exit)
            {
                if (CheckBooster(level, true))
                    exit = true;

                yield return 0.05f;
            }

            yield break;
        }

        private static void NewBoostMechanic(Player obj)
        {
            if (booster == null)
                throw new NullReferenceException("Badeline Booster is null?");

            booster.Collidable = false;
            booster.Add(new Coroutine(NewBoost()));
        }
        private static IEnumerator NewBoost()
        {
            var player = GrabBagModule.playerInstance;

            while (boosting)
                yield return 0;

            boosting = true;
            booster.Visible = true;

            Type type = typeof(BadelineBoost);

            Vector2[] nodes = type.GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(booster) as Vector2[];
            int nodeIndex = (int)type.GetField("nodeIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(booster);
            Sprite sprite = type.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(booster) as Sprite;
            Image stretch = type.GetField("stretch", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(booster) as Image;

            Level level = booster.Scene as Level;

            if (BadelineFollower.instance.dummy.Visible)
            {
                BadelineFollower.instance.dummy.Visible = false;
            }

            if (!firstBoost)
                type.GetField("nodeIndex", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(booster, ++nodeIndex);

            bool finalBoost = nodeIndex >= nodes.Length;

            if (!firstBoost)
            {
                sprite.Visible = false;
                sprite.Position = Vector2.Zero;
                booster.Collidable = false;

                Audio.Play("event:/char/badeline/booster_begin", booster.Position);

                player.StateMachine.State = 11;
                player.DummyAutoAnimate = false;
                player.DummyGravity = false;
                player.Dashes = 1;
                player.RefillStamina();
                player.Speed = Vector2.Zero;
                int num = Math.Sign(player.X - booster.X);
                if (num == 0)
                {
                    num = -1;
                }

                BadelineDummy badeline = new BadelineDummy(booster.Position);
                booster.Scene.Add(badeline);
                player.Facing = (Facings)(-num);
                badeline.Sprite.Scale.X = (float)num;

                Vector2 playerFrom = player.Position;
                Vector2 playerTo = booster.Position + new Vector2((float)(num * 4), -3f);
                Vector2 badelineFrom = badeline.Position;
                Vector2 badelineTo = booster.Position + new Vector2((float)(-(float)num * 4), 3f);


                for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.2f)
                {
                    Vector2 vector = Vector2.Lerp(playerFrom, playerTo, p);
                    if (player.Scene != null)
                    {
                        player.MoveToX(vector.X, null);
                    }
                    if (player.Scene != null)
                    {
                        player.MoveToY(vector.Y, null);
                    }
                    badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, p);
                    yield return null;
                }
                playerFrom = default;
                playerTo = default;
                badelineFrom = default;
                badelineTo = default;

                Audio.Play("event:/char/badeline/booster_throw", booster.Position);

                badeline.Sprite.Play("boost", false, false);

                yield return 0.1f;

                if (!player.Dead)
                {
                    player.MoveV(5f, null, null);
                }

                yield return 0.1f;

                booster.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
                {
                    if (player.Dashes < player.Inventory.Dashes)
                    {
                        player.Dashes++;
                    }
                    booster.Scene.Remove(badeline);
                    (booster.Scene as Level).Displacement.AddBurst(badeline.Position, 0.25f, 8f, 32f, 0.5f, null, null);
                }, 0.15f, true));

                (booster.Scene as Level).Shake(0.3f);

                player.BadelineBoostLaunch(booster.CenterX);
            }

            booster.Visible = true;

            Vector2 from = firstBoost ? BadelineFollower.instance.dummy.Position : booster.Position;
            Vector2 to = finalBoost ? BadelineFollower.instance.dummy.Position : nodes[nodeIndex];

            float duration = Vector2.Distance(from, to) / 320f;

            stretch.Visible = true;
            stretch.Rotation = (to - from).Angle();
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
            tween.OnUpdate = delegate (Tween t)
            {
                if (finalBoost)
                    to = BadelineFollower.instance.dummy.Position;

                booster.Position = Vector2.Lerp(from, to, t.Eased);
                stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
                stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;
                if (t.Eased < 0.9f && booster.Scene.OnInterval(0.03f))
                {
                    TrailManager.Add(booster, Player.TwoDashesHairColor, 0.5f);
                    level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, booster.Center, Vector2.One * 4f);
                }
            };
            tween.OnComplete = delegate (Tween t)
            {
                stretch.Visible = false;
                if (finalBoost)
                {
                    BadelineFollower.instance.dummy.Visible = true;

                }
                else
                {
                    booster.Visible = true;
                    sprite.Visible = true;
                    booster.Collidable = true;
                }
                Audio.Play("event:/char/badeline/booster_reappear", booster.Position);
            };
            booster.Add(tween);
            Audio.Play("event:/char/badeline/booster_relocate", null, 0f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.DirectionalShake(-Vector2.UnitY, 0.3f);
            level.Displacement.AddBurst(booster.Center, 0.4f, 8f, 32f, 0.5f, null, null);

            firstBoost = false;
            boosting = false;
            yield break;
        }

        public static void Search()
        {
            if (LookForBubble != null && LookForBubble.Active)
                LookForBubble.Cancel();

            GrabBagModule.playerInstance.Add(LookForBubble = new Coroutine(SearchForBadeline(Engine.Scene as Level)));
        }

        [Command("spawn_follower", "spawn badeline follower")]
        public static void CmdSpawnBadeline()
        {
            Level level = Engine.Scene as Level;
            SpawnBadelineFriendo(level);
        }
        public static void SpawnBadelineFriendo(Level _level)
        {
            var player = GrabBagModule.playerInstance;

            if (_level == null)
                return;

            _level.Session.SetFlag(SESSION_FLAG, true);

            if (player == null)
                return;

            BadelineFollower follower = new BadelineFollower(_level, player.Position);
            _level.Add(follower);
            player.Leader.GainFollower(follower.follower);
        }

        public Follower follower { get; private set; }
        public static BadelineFollower instance { get; set; }
        public BadelineDummy dummy { get; private set; }
        private float previousPosition;

        public override void SceneBegin(Scene scene)
        {
            boosting = false;
            base.SceneBegin(scene);
        }

        public void Readd(Level lvl, Player obj)
        {
            lvl.Add(this);
            lvl.Add(dummy);
            obj.Leader.GainFollower(follower);
            dummy.Position = obj.Position - new Vector2(obj.Facing == Facings.Left ? -5 : 5, 16);
        }

        public BadelineFollower(Level level, Vector2 position) : this(level, new BadelineDummy(position), position)
        {
        }
        public BadelineFollower(Level level, BadelineDummy _dummy, Vector2 position) : base(position)
        {
            level.Session.SetFlag(SESSION_FLAG, true);

            level.Add(dummy = _dummy);
            dummy.Add(follower = new Follower());
            follower.PersistentFollow = true;
            follower.Added(dummy);

            AddTag(Tags.Persistent);
            dummy.AddTag(Tags.Persistent);

            instance = this;
        }

        public override void Update()
        {
            if (GrabBagModule.playerInstance != null && (follower.Leader == null || follower.Leader.Entity != GrabBagModule.playerInstance))
                GrabBagModule.playerInstance.Leader.GainFollower(follower);

            if ((previousPosition - dummy.Position.X) * dummy.Sprite.Scale.X > 0)
            {
                dummy.Sprite.Scale.X *= -1;
            }
            previousPosition = dummy.Position.X;

            base.Update();
        }

    }
}
