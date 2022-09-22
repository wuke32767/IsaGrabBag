using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.IsaGrabBag {
    public class GrabBagModule : EverestModule {
        private static GrabBagMeta gbMeta;

        public GrabBagModule() {
            Instance = this;
        }

        public static GrabBagMeta GrabBagMeta {
            get {
                if (gbMeta == null) {

                    AreaKey key;
                    Level level = Engine.Scene as Level;
                    level ??= Engine.NextScene as Level;

                    if (level != null) {
                        key = level.Session.Area;
                    } else {
                        LevelLoader loader = Engine.Scene as LevelLoader;
                        key = loader.Level.Session.Area;
                    }

                    gbMeta = GrabBagMeta.Default(key);
                };
                return gbMeta;
            }
        }


        public static int ZipLineState { get; private set; }
        public static int ArrowBlockState { get; private set; }

        public override Type SessionType => typeof(IsaSession);
        public static IsaSession Session => (IsaSession)Instance._Session;

        public static GrabBagModule Instance { get; private set; }

        public static Player playerInstance { get; private set; }

        public static SpriteBank sprites { get; private set; }

        public override void Load() {
            ArrowBubble.Load();
            DreamSpinnerBorder.Load();
            ForceVariants.Load();
            RewindCrystal.Load();

            Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            Everest.Events.Level.OnEnter += Level_OnEnter;
            Everest.Events.Level.OnExit += Level_OnExit;
            Everest.Events.Level.OnLoadEntity += Level_OnLoadEntity;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;

            On.Celeste.Player.ctor += PlayerInit;
            On.Celeste.Player.UpdateSprite += UpdatePlayerVisuals;
            On.Celeste.Player.Update += ZipLine.OnPlayerUpdate;
            On.Celeste.BadelineBoost.Awake += BadelineBoostAwake;
            On.Celeste.ChangeRespawnTrigger.OnEnter += OnChangeRespawn;
        }

        public override void Unload() {
            ArrowBubble.Unload();
            DreamSpinnerBorder.Unload();
            ForceVariants.Unload();
            RewindCrystal.Unload();

            Everest.Events.Level.OnEnter -= Level_OnEnter;
            Everest.Events.Level.OnExit -= Level_OnExit;
            Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;

            On.Celeste.Player.ctor -= PlayerInit;
            On.Celeste.Player.UpdateSprite -= UpdatePlayerVisuals;
            On.Celeste.Player.Update -= ZipLine.OnPlayerUpdate;
            On.Celeste.BadelineBoost.Awake -= BadelineBoostAwake;
            On.Celeste.ChangeRespawnTrigger.OnEnter -= OnChangeRespawn;
        }

        private void BadelineBoostAwake(On.Celeste.BadelineBoost.orig_Awake orig, BadelineBoost self, Scene scene) {
            orig(self, scene);
            if ((scene as Level).Session.GetFlag(BadelineFollower.SESSION_FLAG)) {
                self.Visible = false;
            }
        }

        private void UpdatePlayerVisuals(On.Celeste.Player.orig_UpdateSprite orig, Player self) {
            if (self.StateMachine == ZipLineState) {
                self.Sprite.Scale.X = Calc.Approach(self.Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
                self.Sprite.Scale.Y = Calc.Approach(self.Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);

                if (ZipLine.GrabbingCoroutine) {
                    return;
                }

                self.Sprite.PlayOffset("fallSlow_carry", .5f, false);
                self.Sprite.Rate = 0.0f;

            } else {
                orig(self);
            }
        }

        private void OnChangeRespawn(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player) {
            orig(self, player);

            ForceVariants.SaveToSession();
        }

        private void PlayerInit(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            ZipLineState = self.StateMachine.AddState(ZipLine.ZipLineUpdate, begin: ZipLine.ZipLineBegin, end: ZipLine.ZipLineEnd, coroutine: ZipLine.ZipLineCoroutine);
        }

        public override void LoadContent(bool firstLoad) {
            DreamSpinnerBorder.LoadTextures();
            RewindCrystal.LoadGraphics();
            sprites = new SpriteBank(GFX.Game, "Graphics/IsaGrabBag.xml");

        }

        private void Player_OnSpawn(Player obj) {
            Level lvl = obj.SceneAs<Level>();

            playerInstance = obj;

            ForceVariants.GetFromSession();

            if (obj.Get<VariantEnforcer>() == null) {
                obj.Add(new VariantEnforcer());
            }

            if (lvl.Session.GetFlag(BadelineFollower.SESSION_FLAG)) {
                foreach (BadelineBoost boost in lvl.Entities.FindAll<BadelineBoost>()) {
                    boost.Visible = false;
                    boost.Collidable = false;
                }

                if (lvl.Entities.FindFirst<BadelineFollower>() == null) {
                    if (BadelineFollower.instance == null) {
                        BadelineFollower follower = new(lvl, obj.Position + new Vector2((int)playerInstance.Facing * -12, -20));
                        lvl.Add(follower);
                        obj.Leader.GainFollower(follower.follower);
                    } else {
                        BadelineFollower.instance.Readd(lvl, obj);
                    }
                }
            }

            if (lvl.Session.GetFlag(BadelineFollower.SESSION_FLAG)) {
                BadelineFollower.instance.dummy.Visible = true;
            }

            BadelineFollower.CheckBooster(lvl, false);
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            gbMeta = null;

            ForceVariants.ResetSession();

            if (BadelineFollower.instance != null) {
                BadelineFollower.instance.RemoveSelf();
            }

            BadelineFollower.instance = null;
        }

        private void Level_OnEnter(Session session, bool fromSaveData) {
            try {
                gbMeta = null;

                //string s = session.Area.GetSID();
                if (session != null && session.Area != null && session.Area.SID != null) {

                    ModAsset get = Everest.Content.Get(session.Area.SID);
                    if (get != null && get.TryGetMeta(out GrabBagWrapperMeta parsed)) {
                        gbMeta = parsed.IsaGrabBag;
                    }
                }
            } catch (Exception e) {
                Logger.Log(LogLevel.Warn, "IsaGrabBag", "Unable to properly get metadata");
                Logger.LogDetailed(e);
            }

            gbMeta ??= GrabBagMeta.Default(session.Area);

            if (session.Area.LevelSet.StartsWith("SpringCollab2020")) {
                GrabBagMeta.WaterBoost = true;
            }

            ForceVariants.GetDefaults();
            ForceVariants.ReinforceSession();

        }

        private void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction) {
            ForceVariants.SaveToSession();

            if (GrabBagMeta == null) {
                gbMeta = GrabBagMeta.Default(level.Session.Area);
            }

            if (level.Session.GetFlag(BadelineFollower.SESSION_FLAG)) {
                if (BadelineFollower.instance == null) {
                    BadelineFollower follower = new(level, playerInstance.Position);
                    level.Add(follower);
                    playerInstance.Leader.GainFollower(follower.follower);

                    BadelineFollower.CheckBooster(level, false);
                }

                BadelineFollower.Search();

                if (level.Session.GetFlag(BadelineFollower.SESSION_FLAG)) {
                    BadelineFollower.instance.dummy.Visible = true;
                }
            }
        }

        private bool Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            switch (entityData.Name) {
                case "isaBag/baddyFollow":
                    if (!level.Session.GetFlag(BadelineFollower.SESSION_FLAG)) {
                        BadelineFollower.SpawnBadelineFriendo(level);
                    }

                    return true;
            }

            return false;
        }
    }
}