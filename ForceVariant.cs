using Microsoft.Xna.Framework;
using Monocle;
using System;


namespace Celeste.Mod.IsaGrabBag
{
    public class ForceVariantTrigger : Trigger
    {
        public enum Variant
        {
            Hiccups = 0,
            InfiniteStamina = 1,
            Invincible = 2,
            InvisibleMotion = 3,
            LowFriction = 4,
            MirrorMode = 5,
            NoGrabbing = 6,
            PlayAsBadeline = 7,
            SuperDashing = 8,
            ThreeSixtyDashing = 9,
            DashAssist = 10
        }
        public enum VariantState
        {
            Enabled,
            Disabled,
            EnabledPermanent,
            DisabledPermanent,
            Toggle,
            SetToDefault
        }

        public Variant variant;
        public VariantState enable;
        public bool keepEnforced;

        private bool? previous;

        public ForceVariantTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            variant = data.Enum("variantChange", Variant.Hiccups);
            enable = data.Enum("enableStyle", VariantState.Enabled);
            keepEnforced = data.Bool("enforceValue", true);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            bool? currentVal = GrabBagModule.Instance.GrabBagSession.Variants[(int)variant];

            switch (enable)
            {
                case VariantState.Enabled:
                case VariantState.EnabledPermanent:
                    previous = currentVal;
                    SetVariant(variant, true, _keepEnforced: keepEnforced);
                    break;
                case VariantState.Disabled:
                case VariantState.DisabledPermanent:
                    previous = currentVal;
                    SetVariant(variant, false, _keepEnforced: keepEnforced);
                    break;
                case VariantState.SetToDefault:
                    SetVariant(variant, null);
                    break;
                case VariantState.Toggle:
                    previous = currentVal;
                    SetVariant(variant, !previous, _keepEnforced: keepEnforced);
                    break;
            }
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);

            switch (enable)
            {
                case VariantState.Disabled:
                case VariantState.Enabled:
                case VariantState.Toggle:
                    SetVariant(variant, previous, false, _keepEnforced: keepEnforced);
                    break;
            }
        }

        public static void Reinforce()
        {
            for (int i = 0; i < 11; i++)
            {
                if (GrabBagModule.Instance.GrabBagSession.Variants[i] == null)
                    GrabBagModule.Instance.GrabBagSession.Variants_Default[i] = GetVariantStatus((Variant)i);
                else
                    SetVariant((Variant)i, GrabBagModule.Instance.GrabBagSession.Variants[i], true);
            }
        }
        public static void OnRespawn()
        {
            for (int i = 0; i < 11; i++)
            {
                if (GrabBagModule.Instance.GrabBagSession.Variants[i] == null && GrabBagModule.Instance.GrabBagSession.Variants_Default[i].HasValue)
                    SetVariantInGame((Variant)i, GrabBagModule.Instance.GrabBagSession.Variants_Default[i].Value);
            }
        }
        public static void SetVariantsToDefault()
        {
            for (int i = 0; i < 11; i++)
            {
                SetVariant((Variant)i, null, false);
            }
        }
        public static void SetVariant(Variant variant, bool? _value, bool _ignoreNull = true, bool _keepEnforced = true)
        {
            // Don't do anything if value is null and ignoring null
            if (_ignoreNull && _value == null)
                return;

            IsaSession saveData = GrabBagModule.Instance.GrabBagSession;

            // Get the value needed to set to the vanilla game.  If _value is null, reset vanilla to game.  Otherwise, set value;
            bool value = _value == null ? GetVariantDefaultValue(variant) : _value.Value;

            if (_keepEnforced && saveData.Variants[(int)variant] == null)
            {
                saveData.Variants_Default[(int)variant] = GetVariantStatus(variant);
            }
            // If not enforcing values, set variant null so player can change if desired
            saveData.Variants[(int)variant] = _keepEnforced ? _value : null;

            SetVariantInGame(variant, value);
        }
        private static void SetVariantInGame(Variant variant, bool value)
        {
            // Set value in game
            switch (variant)
            {
                case Variant.Hiccups:
                    SaveData.Instance.Assists.Hiccups = value;
                    break;
                case Variant.InfiniteStamina:
                    SaveData.Instance.Assists.InfiniteStamina = value;
                    break;
                case Variant.Invincible:
                    SaveData.Instance.Assists.Invincible = value;
                    break;
                case Variant.InvisibleMotion:
                    SaveData.Instance.Assists.InvisibleMotion = value;
                    break;
                case Variant.LowFriction:
                    SaveData.Instance.Assists.LowFriction = value;
                    break;
                case Variant.MirrorMode:
                    SaveData.Instance.Assists.MirrorMode = Input.Aim.InvertedX = Input.MoveX.Inverted = value;
                    break;
                case Variant.NoGrabbing:
                    SaveData.Instance.Assists.NoGrabbing = value;
                    break;
                case Variant.PlayAsBadeline:
                    bool originalValue = SaveData.Instance.Assists.PlayAsBadeline;
                    SaveData.Instance.Assists.PlayAsBadeline = value;
                    if (value != originalValue)
                    {
                        // apply the effect immediately
                        Player entity = Engine.Scene.Tracker.GetEntity<Player>();
                        if (entity != null)
                        {
                            PlayerSpriteMode mode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : entity.DefaultSpriteMode;
                            if (entity.Active)
                            {
                                entity.ResetSpriteNextFrame(mode);
                                return;
                            }
                            entity.ResetSprite(mode);
                        }
                    }
                    break;
                case Variant.SuperDashing:
                    SaveData.Instance.Assists.SuperDashing = value;
                    break;
                case Variant.ThreeSixtyDashing:
                    SaveData.Instance.Assists.ThreeSixtyDashing = value;
                    break;
                case Variant.DashAssist:
                    SaveData.Instance.Assists.DashAssist = value;
                    break;
            }
        }
        public static bool GetVariantDefaultValue(Variant variant)
        {
            if (GrabBagModule.Instance.GrabBagSession.Variants[(int)variant] == null || !GrabBagModule.Instance.GrabBagSession.Variants_Default[(int)variant].HasValue)
            {
                return GetVariantStatus(variant);
            }
            return GrabBagModule.Instance.GrabBagSession.Variants_Default[(int)variant].Value;
        }
        public static bool GetVariantStatus(Variant variant)
        {
            switch (variant)
            {
                case Variant.Hiccups:
                    return SaveData.Instance.Assists.Hiccups;
                case Variant.InfiniteStamina:
                    return SaveData.Instance.Assists.InfiniteStamina;
                case Variant.Invincible:
                    return SaveData.Instance.Assists.Invincible;
                case Variant.InvisibleMotion:
                    return SaveData.Instance.Assists.InvisibleMotion;
                case Variant.LowFriction:
                    return SaveData.Instance.Assists.LowFriction;
                case Variant.MirrorMode:
                    return SaveData.Instance.Assists.MirrorMode;
                case Variant.NoGrabbing:
                    return SaveData.Instance.Assists.NoGrabbing;
                case Variant.PlayAsBadeline:
                    return SaveData.Instance.Assists.PlayAsBadeline;
                case Variant.SuperDashing:
                    return SaveData.Instance.Assists.SuperDashing;
                case Variant.ThreeSixtyDashing:
                    return SaveData.Instance.Assists.ThreeSixtyDashing;
                case Variant.DashAssist:
                    return SaveData.Instance.Assists.DashAssist;
            }
            return false;
        }
    }
    public class VariantEnforcer : Component
    {
        public VariantEnforcer(bool active, bool visible) : base(active, visible) { }

        public override void Update()
        {
            ForceVariantTrigger.Reinforce();
        }
    }

    public class WaterFix : Component
    {
        public WaterFix(bool active, bool visible) : base(active, visible) { }

        private Player player;

        public override void Update()
        {
            if (!WaterBoostMechanic.WaterBoost)
                return;

            player = Entity as Player;
            if (player == null || !player.Collidable)
            {
                return;
            }

            Vector2 posOffset = player.Position + player.Speed * Engine.DeltaTime * 2;

            bool isInWater = player.CollideCheck<Water>(posOffset) || player.CollideCheck<Water>(posOffset + Vector2.UnitY * -8f);

            if (!isInWater && player.StateMachine.State == 3 && (player.Speed.Y < 0 || Input.MoveY.Value == -1 || Input.Jump.Check))
            {
                player.Speed.Y = (Input.MoveY.Value == -1 || Input.Jump.Check) ? -110 : 0;
                if (player.Speed.Y < -1)
                {
                    player.Speed.X *= 1.1f;
                }
            }
        }
    }

}