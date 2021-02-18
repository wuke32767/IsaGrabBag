using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.IsaGrabBag
{
    public class GrabBagModule : EverestModule
    {
        public static int ZipLineState { get; private set; }

        public override Type SessionType => typeof(IsaSession);
        public IsaSession GrabBagSession => (IsaSession)base._Session;

        public override Type SettingsType => typeof(IsaSettings);
        public static IsaSettings Settings => (IsaSettings)Instance._Settings;

        public static GrabBagModule Instance { get; private set; }

        public static Player playerInstance { get; private set; }

        public static SpriteBank sprites { get; private set; }

        public static bool CheckGrab
        {
            get
            {
                if (Everest.VersionCelesteString.Contains("1.3.1.2"))
                {
                    return NotBetaGrab;
                }
                else
                {
                    return BetaGrab;
                }
            }
        }

        

        private static bool BetaGrab
        {
            get { return Input.GrabCheck; }
        }
        private static bool NotBetaGrab
        {
            get{ return Input.Grab.Check; }
        }

        public GrabBagModule()
        {
            Instance = this;
        }

        public override void Initialize()
        {
            
            base.Initialize();
        }
        public override void Load()
        {
            Everest.Events.Level.OnEnter += Level_OnEnter;
            Everest.Events.Level.OnExit += Level_OnExit;
            Everest.Events.Level.OnLoadLevel += LoadLevel;
            Everest.Events.Level.OnLoadEntity += Level_OnLoadEntity;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;

            On.Celeste.Player.ctor += PlayerInit;
            On.Celeste.Player.UpdateSprite += UpdatePlayerVisuals;
            On.Celeste.Player.Update += ZipLine.OnPlayerUpdate;

            //On.Celeste.Player.Added += OnPlayerAdded;

            On.Celeste.BadelineBoost.Awake += BadelineBoostAwake;

            On.Celeste.ChangeRespawnTrigger.OnEnter += OnChangeRespawn;
        }
        public override void Unload()
        {
            Everest.Events.Level.OnEnter -= Level_OnEnter;
            Everest.Events.Level.OnExit -= Level_OnExit;
            Everest.Events.Level.OnLoadLevel -= LoadLevel;
            Everest.Events.Level.OnLoadEntity -= Level_OnLoadEntity;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;

            On.Celeste.Player.ctor -= PlayerInit;
            On.Celeste.Player.UpdateSprite -= UpdatePlayerVisuals;
            On.Celeste.Player.Update -= ZipLine.OnPlayerUpdate;

            //On.Celeste.Player.Added -= OnPlayerAdded;

            On.Celeste.BadelineBoost.Awake -= BadelineBoostAwake;

            On.Celeste.ChangeRespawnTrigger.OnEnter -= OnChangeRespawn;
        }

        private void OnPlayerAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            if (Settings.CustomCharacter)
            {
                scene.Add(new CustomPlayer(self));
            }
            else
            {
                orig(self, scene);
            }
        }

        private void BadelineBoostAwake(On.Celeste.BadelineBoost.orig_Awake orig, BadelineBoost self, Scene scene)
        {
            orig(self, scene);

            if (BadelineFollower.HasBadeline(scene as Level))
                self.Visible = false;
        }


        private void UpdatePlayerVisuals(On.Celeste.Player.orig_UpdateSprite orig, Player self)
        {
            if (self.StateMachine == ZipLineState)
            {
                self.Sprite.Scale.X = Calc.Approach(self.Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
                self.Sprite.Scale.Y = Calc.Approach(self.Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);

                if (ZipLine.GrabbingCoroutine)
                    return;

                self.Sprite.PlayOffset("fallSlow_carry", .5f, false);
                self.Sprite.Rate = 0.0f;

            }
            else
            {
                orig(self);
            }
        }

        private void OnChangeRespawn(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player)
        {
            orig(self, player);

            for (int i = 0; i < GrabBagSession.ColorWall.Length; i++)
            {
                GrabBagSession.ColorWallSave[i] = GrabBagSession.ColorWall[i];
            }
            for (int i = 0; i < GrabBagSession.Variants.Length; i++)
            {
                GrabBagSession.Variants_Save[i] = GrabBagSession.Variants[i];
            }
        }

        private void PlayerInit(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);

            ZipLineState = self.StateMachine.AddState(ZipLine.ZipLineUpdate, begin: ZipLine.ZipLineBegin, end:ZipLine.ZipLineEnd, coroutine:ZipLine.ZipLineCoroutine);
        }

        public override void LoadContent(bool firstLoad)
        {
            sprites = new SpriteBank(GFX.Game, "Graphics/IsaGrabBag.xml");

            Sprite textures = sprites.Create("dreamSpin");

            Texture2D tex;

            Color[] colors;
            for (int i = 0; i < 4; ++i)
            {
                tex = textures.GetFrame("fg", i).Texture.Texture;
                colors = new Color[tex.Width * tex.Height];

                DreamSpinnerBorder.spinners[i] = new byte[tex.Width * tex.Height];
                tex.GetData(colors);

                for (int uv = 0; uv < tex.Width * tex.Height; ++uv)
                {
                    if (colors[uv].A == 0)
                        DreamSpinnerBorder.spinners[i][uv] = 0;
                    else if (colors[uv].R == 0)
                        DreamSpinnerBorder.spinners[i][uv] = 2;
                    else
                        DreamSpinnerBorder.spinners[i][uv] = 1;
                }
            }
            tex = textures.GetFrame("bg", 0).Texture.Texture;
            colors = new Color[tex.Width * tex.Height];

            DreamSpinnerBorder.fillers = new byte[tex.Width * tex.Height];
            tex.GetData(colors);

            for (int uv = 0; uv < tex.Width * tex.Height; ++uv)
            {
                if (colors[uv].A == 0)
                    DreamSpinnerBorder.fillers[uv] = 0;
                else if (colors[uv].R == 0)
                    DreamSpinnerBorder.fillers[uv] = 2;
                else
                    DreamSpinnerBorder.fillers[uv] = 1;
            }
        }

        private void Player_OnSpawn(Player obj)
        {
            Level lvl = obj.SceneAs<Level>();

            playerInstance = obj;
            for (int i = 0; i < GrabBagSession.ColorWall.Length; i++)
            {
                GrabBagSession.ColorWall[i] = GrabBagSession.ColorWallSave[i];
            }
            for (int i = 0; i < GrabBagSession.Variants.Length; i++)
            {
                GrabBagSession.Variants[i] = GrabBagSession.Variants_Save[i];
            }
            foreach (ToggleBlock block in obj.Scene.Entities.FindAll<ToggleBlock>())
            {
                block.SetState();
            }
            ForceVariantTrigger.OnRespawn();

            if (obj.Get<VariantEnforcer>() == null)
            {
                obj.Add(new VariantEnforcer(true, false));
            }

            if (obj.Get<WaterFix>() == null)
            {
                obj.Add(new WaterFix(true, false));
            }

            if (BadelineFollower.HasBadeline(lvl))
            {
                foreach (BadelineBoost boost in lvl.Entities.FindAll<BadelineBoost>())
                {
                    boost.Visible = false;
                    boost.Collidable = false;
                }

                if (lvl.Entities.FindFirst<BadelineFollower>() == null)
                {
                    if (BadelineFollower.instance == null)
                    {
                        BadelineFollower follower = new BadelineFollower(lvl, obj.Position + new Vector2((int)playerInstance.Facing * -12, -20));
                        lvl.Add(follower);
                        obj.Leader.GainFollower(follower.follower);
                    }
                    else
                    {
                        BadelineFollower.instance.Readd(lvl, obj);
                    }
                }
            }
            if (BadelineFollower.HasBadeline(lvl))
                BadelineFollower.instance.dummy.Visible = true;

            BadelineFollower.CheckBooster(lvl, false);
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            ForceVariantTrigger.SetVariantsToDefault();

            if (BadelineFollower.HasBadeline(level))
                BadelineFollower.instance.RemoveSelf();

            BadelineFollower.instance = null;
        }

        private void Level_OnEnter(Session session, bool fromSaveData)
        {
            Logger.Log("IsaGrabBag", "Entering Level");

            for (int i = 0; i < GrabBagSession.Variants.Length; i++)
            {
                GrabBagSession.Variants_Default[i] = ForceVariantTrigger.GetVariantStatus((ForceVariantTrigger.Variant)i);
            }
            ForceVariantTrigger.Reinforce();

            if (session.Area.LevelSet.StartsWith("SpringCollab2020"))
            {
                WaterBoostMechanic.WaterBoost = true;
            }
        }
        
        private void LoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            switch (playerIntro)
            {
                default:
                    break;
                case Player.IntroTypes.Respawn:
                    for (int i = 0; i < GrabBagSession.Variants.Length; i++)
                    {
                        GrabBagSession.Variants[i] = GrabBagSession.Variants_Save[i];
                    }
                    for (int i = 0; i < GrabBagSession.ColorWall.Length; i++)
                    {
                        GrabBagSession.ColorWall[i] = GrabBagSession.ColorWallSave[i];
                    }
                    break;
                case Player.IntroTypes.TempleMirrorVoid:
                case Player.IntroTypes.WakeUp:
                case Player.IntroTypes.Transition:
                    for (int i = 0; i < GrabBagSession.ColorWall.Length; i++)
                    {
                        GrabBagSession.ColorWallSave[i] = GrabBagSession.ColorWall[i];
                    }
                    for (int i = 0; i < GrabBagSession.Variants.Length; i++)
                    {
                        GrabBagSession.Variants_Save[i] = GrabBagSession.Variants[i];
                    }
                    break;
            }

            foreach (ToggleBlock block in level.Entities.FindAll<ToggleBlock>())
            {
                block.SetState();
            }

            if (level.Session.GetFlag("has_badeline_follow"))
            {
                if (BadelineFollower.instance == null)
                {
                    BadelineFollower follower = new BadelineFollower(level, playerInstance.Position);
                    level.Add(follower);
                    playerInstance.Leader.GainFollower(follower.follower);

                    BadelineFollower.CheckBooster(level, false);
                }
                BadelineFollower.Search();

                if (BadelineFollower.HasBadeline(level.Session))
                    BadelineFollower.instance.dummy.Visible = true;
            }
        }


        private bool Level_OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData)
        {
            switch (entityData.Name)
            {
                case "CoreHeatWindTrigger":
                    level.Add(new HotColdWind(entityData, offset));
                    return true;
                case "isaBag/colorSwitch":
                    level.Add(new ToggleSwitch(entityData, offset));
                    return true;
                case "isaBag/colorBlock":
                    level.Add(new ToggleBlock(entityData, offset));
                    return true;
                case "isaBag/dreamSpinner":
                    level.Add(new DreamSpinner(entityData, offset, false));
                    return true;
                case "isaBag/dreamSpinFake":
                    level.Add(new DreamSpinner(entityData, offset, true));
                    return true;
                case "ForceVariantTrigger":
                    level.Add(new ForceVariantTrigger(entityData, offset));
                    return true;
                case "isaBag/colorSwitchTrigger":
                    level.Add(new ToggleSwitchTrigger(entityData, offset));
                    return true;
                case "isaBag/zipline":
                    level.Add(new ZipLine(entityData, offset));
                    break;
                case "isaBag/waterBoost":
                    level.Add(new WaterBoostMechanic(entityData));
                    return true;
                case "isaBag/baddyFollow":
                    if (!BadelineFollower.HasBadeline(level))
                        BadelineFollower.SpawnBadelineFriendo(level);
                    return true;
            }
            return false;
        }
    }
}