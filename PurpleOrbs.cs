//using System.Reflection;
//using System.Collections;
//using Microsoft.Xna.Framework;
//using Monocle;

//namespace Celeste.Mod.IsaGrabBag
//{
//    class PurpleOrb : Booster
//    {
//        private const float SLOW_SPEED = 50, FAST_SPEED = 240, DISABLED_WAIT = 0.15f;

//        private bool isMoving;
//        private MethodInfo onPlayerBase = typeof(Booster).GetMethod("OnPlayer", BindingFlags.NonPublic | BindingFlags.Instance),
//            onDashBase = typeof(Booster).GetMethod("OnPlayerDashed", BindingFlags.NonPublic | BindingFlags.Instance);
//        private FieldInfo respawnTimer = typeof(Booster).GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);
//        private Player rider;
//        private Vector2 direction;
//        private Sprite visuals;
//        private float disableTimer = 0;
//        private Rectangle popCollider;

//        public PurpleOrb(EntityData data, Vector2 offset) : this(data== null ? offset : data.Position + offset)
//        {
//        }
//        private PurpleOrb(Vector2 position) : base(position, true)
//        {
//            popCollider = new Rectangle(-9, -9, 18, 18);
//        }

//        public override void Added(Scene scene)
//        {
//            base.Added(scene);
//            PlayerCollider playerInteraction = Get<PlayerCollider>();
//            playerInteraction.OnCollide = OnPlayerNew;
//            DashListener listener = Get<DashListener>();
//            listener.OnDash = OnDashNew;
//            visuals = Get<Sprite>();
            
//        }

//        public override void Update()
//        {
//            base.Update();
//            IsColliding();

//            if (disableTimer > 0)
//            {
//                disableTimer -= Engine.DeltaTime;
//                if (disableTimer <= 0)
//                    Collidable = true;
//            }

//            if (isMoving)
//            {
//                if (visuals.CurrentAnimationID == "pop")
//                {
//                    visuals.Play("loop");
//                }
//                Collider.Position = rider != null ? (rider.Position - Position) + playerOffset : Collider.Position + (direction * SLOW_SPEED * Engine.DeltaTime);

//                if (rider != null)
//                {
//                    if (rider.StateMachine.State == 2)
//                    {
//                        rider = null;
//                        disableTimer = DISABLED_WAIT;
//                        Collidable = false;
//                        visuals.Visible = true;
//                        respawnTimer.SetValue(this, 0);
//                    }
//                    else
//                    {
//                        direction = rider.Speed;
//                        direction.Normalize();
//                    }
//                }
//                if (CollideCheck<Solid>(Position) || (rider != null && rider.StateMachine.State != 5))
//                {
//                    isMoving = false;

//                    Collider.Position = playerOffset;
//                }
//                visuals.Position = Collider.Position + playerOffset;
//            }
//            else
//            {
//                Collider.Position = playerOffset;
//            }
//        }

//        private bool IsColliding()
//        {
//            foreach (Entity e in Scene.Entities.FindAll<SolidTiles>())
//            {
//                if (e.CollideRect(new Rectangle(0, 0, 100, 100), Collider.AbsolutePosition))
//                {
//                    throw new System.Exception();
//                }
//            }
//            return false;
//        }

//        private void OnDashNew(Vector2 _dashDir)
//        {
//            OnPlayerDashed(_dashDir);
//        }

//        private void OnPlayerNew(Player player)
//        {
//            rider = player;
//            onPlayerBase.Invoke(this, new object[] { player });
//            if (!isMoving)
//            {
//                isMoving = true;
//            }
//            visuals.Position = rider.Position - Position;
//        }

//        public override void Render()
//        {
//            base.Render();
//            Draw.Point(Collider.AbsolutePosition, Color.Black);
//            Draw.HollowRect(popCollider.X + Position.X + Collider.Position.X, popCollider.Y + Position.Y + Collider.Position.Y, popCollider.Width, popCollider.Height, Color.Red);
//        }
//    }
//}