using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using System.Collections.Generic;


namespace Celeste.Mod.IsaGrabBag {

	public enum Variant {
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
	public enum VariantState {
		Enabled,
		Disabled,
		EnabledTemporary,
		DisabledTemporary,
		EnabledPermanent,
		DisabledPermanent,
		Toggle,
		SetToDefault
	}

	public static class ForceVariants {

		public static void Unload() {
			On.Celeste.Level.AssistMode -= Level_AssistMode;
			On.Celeste.Level.VariantMode -= Level_VariantMode;
		}
		public static void Load() {
			On.Celeste.Level.AssistMode += Level_AssistMode;
			On.Celeste.Level.VariantMode += Level_VariantMode;
		}


		private static void Level_VariantMode(On.Celeste.Level.orig_VariantMode orig, Level self, int returnIndex, bool minimal) {

			orig(self, returnIndex, minimal);

			List<Entity> list = self.Entities.ToAdd;
			OnVariantMenu(list[list.Count - 1] as TextMenu, false);
			

		}

		private static void Level_AssistMode(On.Celeste.Level.orig_AssistMode orig, Level self, int returnIndex, bool minimal) {

			orig(self, returnIndex, minimal);

			List<Entity> list = self.Entities.ToAdd;
			OnVariantMenu(list[list.Count - 1] as TextMenu, true);
		}

		private readonly static List<Variant> menuLayout = new List<Variant>(){
			Variant.MirrorMode,
			Variant.ThreeSixtyDashing,
			Variant.InvisibleMotion,
			Variant.NoGrabbing,
			Variant.LowFriction,
			Variant.SuperDashing,
			Variant.Hiccups,
			Variant.PlayAsBadeline,
			Variant.InfiniteStamina,
			Variant.DashAssist,
			Variant.Invincible,
		};

		private static void OnVariantMenu(TextMenu menu, bool assist) {

			var session = GrabBagModule.Session;

			//Logger.Log("IsaGrabBag", $"Starting {(assist ? "Assist" : "Variant")} Menu of size {menu.Items.Count}");

			int index = assist ? 8 : 0;
			for (int i = 0; i < menu.Items.Count; ++i) {

				var item = menu.Items[i];

				if (!(item is TextMenu.OnOff))
					continue;

				Variant v = menuLayout[index++];

				//Logger.Log("IsaGrabBag", $"Looking at {v}");

				if (session.Variants[(int)v] != null) {
					item.Disabled = true;
				}
			}
		}

		public static void SaveToSession() {

			var session = GrabBagModule.Session;

			for (int i = 0; i < session.Variants.Length; i++) {
				session.Variants_Save[i] = session.Variants[i];
			}
		}
		public static void GetFromSession() {

			var session = GrabBagModule.Session;

			for (int i = 0; i < session.Variants.Length; i++) {
				session.Variants[i] = session.Variants_Save[i];
			}
		}
		public static void ReinforceSession() {
			for (int i = 0; i < 11; i++) {
				if (GrabBagModule.Session.Variants[i] == null)
					continue;

				SetVariant(i, GrabBagModule.Session.Variants[i].Value ? VariantState.EnabledPermanent : VariantState.DisabledPermanent);
			}
		}
		public static void ResetSession() {	

			var session = GrabBagModule.Session;

			for (int i = 0; i < session.Variants.Length; i++) {
				//Logger.Log("IsaGrabBag", session.Variants[i] == null ? "null" : session.Variants[i].ToString());
				if (session.Variants[i] != null)
					SetVariant(i, session.Variants_Default[i] ? VariantState.Enabled : VariantState.Disabled);
			}
		}

		public static void SetVariant(int variant, VariantState state) {

			SetVariant((Variant)variant, state);
		}
		public static void SetVariant(Variant variant, VariantState state) {

			if (state == VariantState.SetToDefault) {

				var session = GrabBagModule.Session;

				SetVariantInGame(variant, session.Variants_Default[(int)variant]);

				session.Variants[(int)variant] = null;

				return;
			}

			bool permanent = state == VariantState.EnabledPermanent || state == VariantState.DisabledPermanent;
			bool value = state == VariantState.Enabled || state == VariantState.EnabledTemporary || state == VariantState.EnabledPermanent || (state == VariantState.Toggle && !GetVariantStatus(variant));

			SetVariantInGame(variant, value);

			if (permanent) {
				var session = GrabBagModule.Session;

				session.Variants[(int)variant] = value;
			}
		}

		public static bool GetVariantStatus(Variant variant) {
			switch (variant) {
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

		private static void SetVariantInGame(Variant variant, bool value) {
			// Set value in game
			switch (variant) {
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
					if (value != originalValue) {
						// apply the effect immediately
						Player entity = Engine.Scene.Tracker.GetEntity<Player>();
						if (entity != null) {
							PlayerSpriteMode mode = SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : entity.DefaultSpriteMode;
							if (entity.Active) {
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
	}
	public class ForceVariantTrigger : Trigger
	{
		public Variant variant;
		public VariantState enable;

		private bool? previous;

		public ForceVariantTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			variant = data.Enum("variantChange", Variant.Hiccups);
			enable = data.Enum("enableStyle", VariantState.Enabled);
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);

			previous = GrabBagModule.Session.Variants[(int)variant];
			ForceVariants.SetVariant(variant, enable);

		}
		public override void OnLeave(Player player)
		{
			base.OnLeave(player);

			switch (enable)
			{
				case VariantState.DisabledTemporary:
				case VariantState.EnabledTemporary:
					ForceVariants.SetVariant(variant, previous.Value ? VariantState.Enabled : VariantState.Disabled);
					break;
			}
		}

	}
	public class VariantEnforcer : Component
	{
		public VariantEnforcer() : base(true, false) { }

		private Player player;

		public override void Update() {
			player = Entity as Player;
			if (player == null) {
				return;
			}

			//ForceVariants.ReinforceSession();
			//ForceVariantTrigger.Reinforce();
			if (GrabBagModule.GrabBagMeta.WaterBoost)
				WaterBoost();
		}

		private void WaterBoost() {

			if (!player.Collidable)
				return;

			Vector2 posOffset = player.Position + player.Speed * Engine.DeltaTime * 2;

			bool isInWater = player.CollideCheck<Water>(posOffset) || player.CollideCheck<Water>(posOffset + Vector2.UnitY * -8f);

			if (!isInWater && player.StateMachine.State == 3 && (player.Speed.Y < 0 || Input.MoveY.Value == -1 || Input.Jump.Check)) {
				player.Speed.Y = (Input.MoveY.Value == -1 || Input.Jump.Check) ? -110 : 0;
				if (player.Speed.Y < -1) {
					player.Speed.X *= 1.1f;
				}
			}
		}
	}

}