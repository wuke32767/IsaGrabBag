using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using System.Collections.Generic;
using System;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;


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


		static bool[] Variants_Default { get; set; } = new bool[] { false, false, false, false, false, false, false, false, false, false, false };
		static bool?[] Variants { get; set; } = new bool?[] { null, null, null, null, null, null, null, null, null, null, null };

		static Dictionary<TextMenu.Item, int> itemList = new Dictionary<TextMenu.Item, int>();

		static Hook enableHook, disableHook, aPressHook;

		static TextMenu variantMenu;

		public static void Unload() {
			On.Celeste.Level.AssistMode -= Level_AssistMode;
			On.Celeste.Level.VariantMode -= Level_VariantMode;

			On.Celeste.TextMenu.Close -= TextMenu_Close;
			enableHook?.Dispose();
			disableHook?.Dispose();
			aPressHook?.Dispose();
		}
		public static void Load() {
			On.Celeste.Level.AssistMode += Level_AssistMode;
			On.Celeste.Level.VariantMode += Level_VariantMode;

			disableHook = new Hook(
				typeof(TextMenu.Option<bool>).GetMethod("LeftPressed", BindingFlags.Instance | BindingFlags.Public), 
				(Action<OptionChanged, TextMenu.Option<bool>>)OnChange);

			enableHook = new Hook(
				typeof(TextMenu.Option<bool>).GetMethod("RightPressed", BindingFlags.Instance | BindingFlags.Public), 
				(Action<OptionChanged, TextMenu.Option<bool>>)OnChange);

			aPressHook = new Hook(
				typeof(TextMenu.Option<bool>).GetMethod("ConfirmPressed", BindingFlags.Instance | BindingFlags.Public),
				(Action<OptionChanged, TextMenu.Option<bool>>)OnChange);
		}

		delegate void OptionChanged(TextMenu.Option<bool> self);
		private static void OnChange(OptionChanged orig, TextMenu.Option<bool> self) {
			orig(self);

			if (itemList.ContainsKey(self)) {

				bool value = self.Index >= 1;

				int index = itemList[self];

				Variants_Default[index] = value;

			}

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

			variantMenu = menu;

			var session = GrabBagModule.Session;

			itemList = new Dictionary<TextMenu.Item, int>();

			On.Celeste.TextMenu.Close += TextMenu_Close;

			int index = assist ? 8 : 0;
			for (int i = 0; i < menu.Items.Count; ++i) {

				var item = menu.Items[i];

				if (!(item is TextMenu.OnOff))
					continue;

				Variant v = menuLayout[index++];

				itemList.Add(item, (int)v);

				if (Variants[(int)v] != null) {
					item.Disabled = true;
				}
			}
		}


		private static void TextMenu_Close(On.Celeste.TextMenu.orig_Close orig, TextMenu self) {
			orig(self);
			if (variantMenu != self)
				return;

			On.Celeste.TextMenu.Close -= TextMenu_Close;
		}

		public static void GetDefaults() {

			for (int i = 0; i < Variants_Default.Length; i++) {
				Variants_Default[i] = GetVariantStatus((Variant)i);
			}
			WallToggleData.GetDefaults();
		}
		public static void SaveToSession() {

			var session = GrabBagModule.Session;

			for (int i = 0; i < Variants.Length; i++) {
				session.Variants_Save[i] = Variants[i];
			}
			WallToggleData.SaveToSession();
		}
		public static void GetFromSession() {

			var session = GrabBagModule.Session;

			for (int i = 0; i < Variants.Length; i++) {
				if (Variants[i] != null && session.Variants_Save[i] == null) {
					SetVariant(i, VariantState.SetToDefault);
				}
				Variants[i] = session.Variants_Save[i];
			}

			WallToggleData.ResetSession();
		}
		public static void ReinforceSession() {
			var session = GrabBagModule.Session;

			for (int i = 0; i < 11; i++) {
				Variants[i] = session.Variants_Save[i];

				if (session.Variants_Save[i] == null)
					continue;

				SetVariant(i, session.Variants_Save[i].Value ? VariantState.EnabledPermanent : VariantState.DisabledPermanent);
			}

			WallToggleData.ReinforceSession();
		}
		public static void ResetSession() {	

			for (int i = 0; i < Variants_Default.Length; i++) {
				SetVariant(i, VariantState.SetToDefault);
			}

			WallToggleData.ResetSession();
		}

		public static void SetVariant(int variant, VariantState state) {

			SetVariant((Variant)variant, state);
		}
		public static void SetVariant(Variant variant, VariantState state) {

			if (state == VariantState.SetToDefault) {

				var session = GrabBagModule.Session;

				Variants[(int)variant] = null;
				SetVariantInGame(variant, Variants_Default[(int)variant]);

				return;
			}

			bool permanent = state == VariantState.EnabledPermanent || state == VariantState.DisabledPermanent;
			bool value = state == VariantState.Enabled || state == VariantState.EnabledTemporary || state == VariantState.EnabledPermanent || (state == VariantState.Toggle && !GetVariantStatus(variant));

			SetVariantInGame(variant, value);

			if (permanent) {
				var session = GrabBagModule.Session;

				Variants[(int)variant] = value;
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
		public static bool? GetVariantModdedValue(Variant variant) {

			return Variants[(int)variant];
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

		private bool previous;

		public ForceVariantTrigger(EntityData data, Vector2 offset) : base(data, offset)
		{
			variant = data.Enum("variantChange", Variant.Hiccups);
			enable = data.Enum("enableStyle", VariantState.Enabled);

			if (data.Has("enforceValue")) {
				bool enforce = data.Bool("enforceValue");
				if (enable == VariantState.Enabled) {
					enable = VariantState.EnabledTemporary;
				}
				else if (enable == VariantState.Disabled) {
					enable = VariantState.DisabledTemporary;
				}
				else if (enable == VariantState.EnabledPermanent && !enforce) {
					enable = VariantState.Enabled;
				}
				else if (enable == VariantState.DisabledPermanent && !enforce) {
					enable = VariantState.Disabled;
				}
			}
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);

			previous = ForceVariants.GetVariantStatus(variant);
			ForceVariants.SetVariant(variant, enable);

		}
		public override void OnLeave(Player player)
		{
			base.OnLeave(player);

			switch (enable)
			{
				case VariantState.DisabledTemporary:
				case VariantState.EnabledTemporary:
					ForceVariants.SetVariant(variant, previous ? VariantState.Enabled : VariantState.Disabled);
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