using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using Celeste.Mod;
using System.Reflection;

namespace Celeste.Mod.IsaGrabBag {
	[Entities.CustomEntity("isaBag/cornerBlock")]
	[Tracked(false)]
	public class CornerBoostBlock : Solid {



		private static FieldInfo retentionTimer, retentionSpeed, moveX;

		static CornerBoostBlock() {
			retentionTimer = typeof(Player).GetField("wallSpeedRetentionTimer", BindingFlags.NonPublic | BindingFlags.Instance);
			retentionSpeed = typeof(Player).GetField("wallSpeedRetained", BindingFlags.NonPublic | BindingFlags.Instance);
			moveX = typeof(Player).GetField("moveX", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public bool CustomTile => _tileset != '\0';

		char _tileset;
		TileGrid tiles;
		List<CornerBoostBlock> Group = new List<CornerBoostBlock>();

		bool HasGroup;

		Point GroupBoundsMin, GroupBoundsMax;

		public CornerBoostBlock(Vector2 position, int width, int height, char tile, bool useTileset) : base(position, width, height, true) {
			if (useTileset) {
				_tileset = tile;
			}
			else {
				_tileset = '\0';
			}

			OnCollide = OnCollision;
		}
		public CornerBoostBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("useTileset", false)) { }

		private void OnCollision(Vector2 dir) {

			var player = GrabBagModule.playerInstance;
			if (player == null)
				player = Scene.Entities.FindFirst<Player>();

			if (dir.X == 0 || player == null)
				return;

			if ((float)retentionTimer.GetValue(player) == 0.06f) {
				retentionTimer.SetValue(player, 0.12f);
			}
		}

		public override void Awake(Scene scene) {

			if (!HasGroup) {

				GroupBoundsMin = new Point((int)X, (int)Y);
				GroupBoundsMax = new Point((int)Right, (int)Bottom);

				AddToGroupAndFindChildren(this);

				Rectangle rectangle = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8);
				VirtualMap<char> virtualMap = new VirtualMap<char>(rectangle.Width, rectangle.Height, '0');


				foreach (CornerBoostBlock block in Group) {
					int left = (int)(block.X / 8f) - rectangle.X;
					int top = (int)(block.Y / 8f) - rectangle.Y;
					int right = (int)(block.Width / 8f);
					int bottom = (int)(block.Height / 8f);

					for (int x = left; x < left + right; x++) {
						for (int y = top; y < top + bottom; y++) {

							virtualMap[x, y] = CustomTile ? _tileset : '3';
						}
					}
				}

				tiles = GFX.FGAutotiler.GenerateMap(virtualMap, new Autotiler.Behaviour {
					EdgesExtend = false,
					EdgesIgnoreOutOfLevel = false,
					PaddingIgnoreOutOfLevel = false
				}).TileGrid;

				if (!CustomTile) {
					var template = tiles;

					tiles = new TileGrid(8, 8, rectangle.Width, rectangle.Height);

					var tex = GFX.Game["isafriend/tilesets/isafriend/boost_block"];

					Vector2 texOffset = new Vector2(0.255127f, 0.4956055f);
					Vector2 texSize = new Vector2(0.01164f, 0.0291f);

					Tileset tileset = new Tileset(tex, 8, 8);


					const int width = 6, height = 15;

					for (int y = 0; y < rectangle.Height; ++y) {
						for (int x = 0; x < rectangle.Width; ++x) {

							var fromTemplate = template.Tiles[x, y];

							if (fromTemplate == null)
								continue;

							float u = (fromTemplate.LeftUV - texOffset.X) / texSize.X;
							float v = (fromTemplate.TopUV  - texOffset.Y) / texSize.Y;

							if (u < 0 && u > -0.0001f)
								u = 0;
							if (v < 0 && v > -0.0001f)
								v = 0;

							if (u < 0 || v < 0 || u >= 1 || v >= 1)
								break;

							tiles.Tiles[x, y] = tileset[(int)(u * width), (int)(v * height)];
						}
					}
				}

				tiles.Position = new Vector2(GroupBoundsMin.X - X, GroupBoundsMin.Y - Y);

				Add(tiles);
			}

			base.Awake(scene);
		}

		private void AddToGroupAndFindChildren(CornerBoostBlock from) {

			if (from.X < GroupBoundsMin.X) {
				GroupBoundsMin.X = (int)from.X;
			}
			if (from.Y < GroupBoundsMin.Y) {
				GroupBoundsMin.Y = (int)from.Y;
			}
			if (from.Right > GroupBoundsMax.X) {
				GroupBoundsMax.X = (int)from.Right;
			}
			if (from.Bottom > GroupBoundsMax.Y) {
				GroupBoundsMax.Y = (int)from.Bottom;
			}
			from.HasGroup = true;
			Group.Add(from);
			if (from != this) {
				//from.master = this;
			}

			foreach (Entity entity in Scene.Tracker.GetEntities<CornerBoostBlock>()) {
				CornerBoostBlock block = (CornerBoostBlock)entity;

				if (!block.HasGroup && block._tileset == _tileset && (Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), block) || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), block))) {
					AddToGroupAndFindChildren(block);
				}
			}
		}

		public static void Load() {
			On.Celeste.Player.ClimbJump += OnClimbJumped;
			On.Celeste.Player.ClimbEnd += Player_ClimbEnd;
			On.Celeste.Player.NormalEnd += Player_NormalEnd;
		}


		public static void Unload() {
			On.Celeste.Player.ClimbJump -= OnClimbJumped;
			On.Celeste.Player.ClimbEnd -= Player_ClimbEnd;
			On.Celeste.Player.NormalEnd -= Player_NormalEnd;
		}

		private static Rectangle GetFacingHitbox(Player player) {

			Rectangle hitbox = player.Collider.Bounds;

			hitbox.Width = 1;

			if (player.Facing == Facings.Left) {
				hitbox.X -= 1;
			}
			else {
				hitbox.X += (int)player.Width;
			}
			hitbox.Y -= 1;
			hitbox.Height += 1;

			return hitbox;
		}

		private static void Player_ClimbEnd(On.Celeste.Player.orig_ClimbEnd orig, Player self) {
			float timer = (float)retentionTimer.GetValue(self);

			orig(self);

			if (self.Scene.CollideCheck<CornerBoostBlock>(GetFacingHitbox(self)))
				retentionTimer.SetValue(self, timer);
		}

		private static void Player_NormalEnd(On.Celeste.Player.orig_NormalEnd orig, Player self) {
			float timer = (float)retentionTimer.GetValue(self);

			orig(self);

			if (self.StateMachine.State == 1 && self.Scene.CollideCheck<CornerBoostBlock>(GetFacingHitbox(self))) {

				retentionTimer.SetValue(self, timer);

			}
		}

		private static void OnClimbJumped(On.Celeste.Player.orig_ClimbJump orig, Player self) {
			orig(self);

			if (self.Scene.CollideCheck<CornerBoostBlock>(GetFacingHitbox(self)) && Math.Abs(self.Speed.X) <= 51f && (float)retentionTimer.GetValue(self) > 0 && (float)retentionSpeed.GetValue(self) != 0) {

				retentionTimer.SetValue(self, Math.Max((float)retentionTimer.GetValue(self), 0.06f));

				float speed = (float)retentionSpeed.GetValue(self);
				retentionSpeed.SetValue(self, speed + (40 * (int)moveX.GetValue(self)));

			}
		}
	}
}