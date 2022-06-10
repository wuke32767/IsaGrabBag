using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IsaGrabBag {
	public class ArrowBubble : Booster {

		static Vector2 GravityDir;
		static FieldInfo sprite = typeof(Booster).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance),
			cannotUseTimer = typeof(Booster).GetField("cannotUseTimer", BindingFlags.NonPublic | BindingFlags.Instance),
			respawnTimer = typeof(Booster).GetField("respawnTimer", BindingFlags.NonPublic | BindingFlags.Instance);

		private Sprite sprites;

		public static void Load() {

			On.Celeste.Player.RedDashEnd += Player_RedDashEnd;
			On.Celeste.Player.RedDashUpdate += Player_RedDashUpdate;
		}

		public static void Unload() {
			
			On.Celeste.Player.RedDashEnd -= Player_RedDashEnd;
			On.Celeste.Player.RedDashUpdate -= Player_RedDashUpdate;
		}

		private static void Player_RedDashEnd(On.Celeste.Player.orig_RedDashEnd orig, Player self) {
			orig(self);
			if (GravityDir != Vector2.Zero) {
				self.UseRefill(false);
			}
			GravityDir = Vector2.Zero;
		}

		private static int Player_RedDashUpdate(On.Celeste.Player.orig_RedDashUpdate orig, Player self) {

			if (self.CanDash) {
				respawnTimer.SetValue(self.LastBooster, 1);
				cannotUseTimer.SetValue(self.LastBooster, 0);
			}

			int value = orig(self);

			if (GravityDir != Vector2.Zero) {

				float approachSpeed = 350 * Engine.DeltaTime;

				void changeValue(ref float val, float dir) {
					val = Calc.Approach(val, (val * dir) >= 0 ? (360 * dir) : 0, approachSpeed);
				}

				if (GravityDir.X != 0)
					changeValue(ref self.Speed.X, GravityDir.X);
				if (GravityDir.Y != 0)
					changeValue(ref self.Speed.Y, GravityDir.Y);

				self.DashDir = self.Speed.SafeNormalize();
			}

			return value;
		}

		public Vector2 gravityDirection;

		public ArrowBubble(EntityData data, Vector2 offset) : base(data.Position + offset, true) {

			string dir = data.Attr("direction", "down");

			Add(new PlayerCollider(OnPlayer));

			switch (dir) {
				default:
					dir = "down";
					gravityDirection = Vector2.UnitY;
					break;
				case "up":
					gravityDirection = -Vector2.UnitY;
					break;
				case "left":
					gravityDirection = -Vector2.UnitX;
					break;
				case "right":
					gravityDirection = Vector2.UnitX;
					break;
			}

			Remove(Get<Sprite>());

			sprites = GrabBagModule.sprites.Create($"booster_{dir}");
			Add(sprites);
			sprite.SetValue(this, sprites);
		}

		void OnPlayer(Player player) {
			if (player.StateMachine != Player.StRedDash) {
				GravityDir = gravityDirection;
			}
			sprites.FlipX = false;
		}
	}
}
