using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace Celeste.Mod.IsaGrabBag {
	public static class WallToggleData {

		static bool[] ColorWallEnabled = new bool[] { false, false, false };


		public static bool IsEnabled(int index, bool inverted = false) => ColorWallEnabled[index] != inverted;
		public static void SetEnabled(int index, bool value = false) {

			if (ColorWallEnabled[index] != value) {
				Toggle(index);
			}
		}
		public static void Toggle(int index) {
			ColorWallEnabled[index] = !ColorWallEnabled[index];

			foreach (ToggleBlock block in Engine.Scene.Entities.FindAll<ToggleBlock>()) {
				block.SetState();
			}
		}

		public static void GetDefaults() {

			for (int i = 0; i < ColorWallEnabled.Length; i++) {
				ColorWallEnabled[i] = false;
			}
		}
		public static void SaveToSession() {

			var session = GrabBagModule.Session;

			for (int i = 0; i < ColorWallEnabled.Length; i++) {
				session.ColorWallState[i] = ColorWallEnabled[i];
			}
		}
		public static void GetFromSession() {

			var session = GrabBagModule.Session;

			for (int i = 0; i < ColorWallEnabled.Length; i++) {
				ColorWallEnabled[i] = session.ColorWallState[i];
			}
		}
		public static void ReinforceSession() {
			var session = GrabBagModule.Session;

			for (int i = 0; i < ColorWallEnabled.Length; i++) {
				ColorWallEnabled[i] = session.ColorWallState[i];

			}
		}
		public static void ResetSession() {
			GetDefaults();
		}



		public static Player playerInstance { get; private set; }

	}
	public class ToggleBlock : Solid {

		public static Color[] BOX_COLORS { get; private set; } = new Color[] { new Color(1, 0.2f, 0.2f), new Color(0.2f, 1, 0.2f), new Color(0.2f, 0.2f, 1) };

		public int colorValue;
		public bool inverted;

		List<ToggleBlock> group;
		bool groupLeader;
		private Vector2 groupOrigin;
		private List<Image> pressed, solid, all;
		private Color color;

		public ToggleBlock(Vector2 position, float width, float height, bool safe, int _colorVal, bool _startInverted) : base(position, width, height, safe) {
			colorValue = _colorVal;
			inverted = _startInverted;
			switch (colorValue) {
				default:
					color = Color.Red;
					break;
				case 1:
					color = Color.Green;
					break;
				case 2:
					color = Color.Blue;
					break;
			}
			all = new List<Image>();
			pressed = new List<Image>();
			solid = new List<Image>();
		}
		public ToggleBlock(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, true, data.Int("colorValue"), data.Bool("startInvert")) {

		}

		public override void Awake(Scene scene) {
			base.Awake(scene);
			foreach (StaticMover mover in staticMovers) {
				Spikes spikes = mover.Entity as Spikes;
				if (spikes != null) {
					spikes.VisibleWhenDisabled = true;
					spikes.EnabledColor = BOX_COLORS[colorValue] * 3f;
					spikes.EnabledColor.A = 1;
					spikes.DisabledColor = BOX_COLORS[colorValue] * .5f;
				}
			}
			//WallToggleData.CheckForToggleInstance(scene);

			if (group == null) {
				groupLeader = true;
				group = new List<ToggleBlock>();
				group.Add(this);
				FindInGroup(this);
			}

			for (float num5 = base.Left; num5 < base.Right; num5 += 8f) {
				for (float num6 = base.Top; num6 < base.Bottom; num6 += 8f) {
					bool flag = this.CheckForSame(num5 - 8f, num6);
					bool flag2 = this.CheckForSame(num5 + 8f, num6);
					bool flag3 = this.CheckForSame(num5, num6 - 8f);
					bool flag4 = this.CheckForSame(num5, num6 + 8f);
					if (flag && flag2 && flag3 && flag4) {
						if (!this.CheckForSame(num5 + 8f, num6 - 8f)) {
							this.SetImage(num5, num6, 3, 0);
						}
						else if (!this.CheckForSame(num5 - 8f, num6 - 8f)) {
							this.SetImage(num5, num6, 3, 1);
						}
						else if (!this.CheckForSame(num5 + 8f, num6 + 8f)) {
							this.SetImage(num5, num6, 3, 2);
						}
						else if (!this.CheckForSame(num5 - 8f, num6 + 8f)) {
							this.SetImage(num5, num6, 3, 3);
						}
						else {
							this.SetImage(num5, num6, 1, 1);
						}
					}
					else if (flag && flag2 && !flag3 && flag4) {
						this.SetImage(num5, num6, 1, 0);
					}
					else if (flag && flag2 && flag3 && !flag4) {
						this.SetImage(num5, num6, 1, 2);
					}
					else if (flag && !flag2 && flag3 && flag4) {
						this.SetImage(num5, num6, 2, 1);
					}
					else if (!flag && flag2 && flag3 && flag4) {
						this.SetImage(num5, num6, 0, 1);
					}
					else if (flag && !flag2 && !flag3 && flag4) {
						this.SetImage(num5, num6, 2, 0);
					}
					else if (!flag && flag2 && !flag3 && flag4) {
						this.SetImage(num5, num6, 0, 0);
					}
					else if (flag && !flag2 && flag3 && !flag4) {
						this.SetImage(num5, num6, 2, 2);
					}
					else if (!flag && flag2 && flag3 && !flag4) {
						this.SetImage(num5, num6, 0, 2);
					}
				}
			}

			SetState();
			updatedPortals = false;
		}

		private static bool updatedPortals;

		public override void Update() {
			base.Update();


			if (!updatedPortals) {
				updatedPortals = true;
				//if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "OutbackHelper" }))
				//	WallToggleData.UpdatePortals(SceneAs<Level>());
			}
		}

		private void UpdateVisualState() {
			if (!this.Collidable) {
				base.Depth = 8990;
			}
			else {
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null && entity.Top >= base.Bottom - 1f) {
					base.Depth = 10;
				}
				else {
					base.Depth = -9990;
				}
			}

			foreach (StaticMover staticMover in this.staticMovers) {
				staticMover.Entity.Depth = base.Depth + 1;
			}

			//this.side.Depth = base.Depth + 5;
			//this.side.Visible = (this.blockHeight > 0);
			//this.occluder.Visible = this.Collidable;

			foreach (Image image in this.solid) {
				image.Visible = this.Collidable;
			}
			foreach (Image image2 in this.pressed) {
				image2.Visible = !this.Collidable;
			}

			if (this.groupLeader) {
				Vector2 scale = new Vector2(1f, 1f);
				foreach (ToggleBlock toggleBlock in this.group) {
					foreach (Image image3 in toggleBlock.all) {
						image3.Scale = scale;
					}
					foreach (StaticMover staticMover2 in toggleBlock.staticMovers) {
						Spikes spikes = staticMover2.Entity as Spikes;
						if (spikes != null) {
							foreach (Component component in spikes.Components) {
								Image image4 = component as Image;
								if (image4 != null) {
									image4.Scale = scale;
								}
							}
						}
					}
				}
			}

		}
		private bool CheckForSame(float x, float y) {
			foreach (Entity entity in base.Scene.Entities.FindAll<ToggleBlock>()) {
				ToggleBlock cassetteBlock = (ToggleBlock)entity;
				if (cassetteBlock.colorValue == colorValue && cassetteBlock.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8))) {
					return true;
				}
			}
			return false;
		}
		private void SetImage(float x, float y, int tx, int ty) {
			pressed.Add(CreateImage(x, y, tx, ty, GFX.Game["objects/isatoggleblock/pressed" + colorValue]));
			solid.Add(CreateImage(x, y, tx, ty, GFX.Game["objects/isatoggleblock/solid" + colorValue]));
		}
		private Image CreateImage(float x, float y, int tx, int ty, MTexture tex) {
			Vector2 value = new Vector2(x - base.X, y - base.Y);
			Image image = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8, null));
			Vector2 vector = this.groupOrigin - this.Position;
			image.Origin = vector - value;
			image.Position = vector;
			image.Color = this.color;
			Add(image);
			all.Add(image);
			return image;
		}
		private void FindInGroup(ToggleBlock block) {
			foreach (Entity entity in base.Scene.Entities.FindAll<ToggleBlock>()) {
				ToggleBlock toggleBlock = (ToggleBlock)entity;
				if (toggleBlock != this && toggleBlock != block && toggleBlock.colorValue == this.colorValue && (toggleBlock.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || toggleBlock.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !this.group.Contains(toggleBlock)) {
					this.group.Add(toggleBlock);
					this.FindInGroup(toggleBlock);
					toggleBlock.group = this.group;
				}
			}
		}

		public override void Added(Scene scene) {
			base.Added(scene);
		}
		public void SetState() {
			Collidable = WallToggleData.IsEnabled(colorValue, inverted);

			if (Collidable) {
				EnableStaticMovers();
			}
			else {
				DisableStaticMovers();
			}

			UpdateVisualState();
		}

		//public override void Render()
		//{
		//    Draw.Rect(Collider, BOX_COLORS[colorValue] * (WallToggleData.IsEnabled(colorValue, inverted) ? 1 : .5f));
		//}
	}
	public class ToggleSwitch : CrushBlock {
		public int SwitchIndex;
		bool hitSide, hitTop;

		Sprite animations;
		Image protector, lightImage;

		public ToggleSwitch(Vector2 position, float width, float height, Axes axes, int index, bool chillOut = false) : base(position, width, height, axes, chillOut) {
			SwitchIndex = index;
			OnDashCollide = NewCollision;
			SurfaceSoundIndex = 11;

			switch (SwitchIndex) {
				default:
					Add(animations = GrabBagModule.sprites.Create("blockred"));
					break;
				case 1:
					Add(animations = GrabBagModule.sprites.Create("blockgreen"));
					break;
				case 2:
					Add(animations = GrabBagModule.sprites.Create("blockblue"));
					break;
			}

			switch (axes) {
				default:
					hitSide = hitTop = true;
					break;
				case Axes.Horizontal:
					hitSide = true;
					hitTop = false;
					Add(protector = new Image(GFX.Game["objects/isatoggleblock/solidtop"]));
					break;
				case Axes.Vertical:
					hitSide = false;
					hitTop = true;
					Add(protector = new Image(GFX.Game["objects/isatoggleblock/solidside"]));
					break;
			}
		}

		public ToggleSwitch(EntityData data, Vector2 offset) : this(data.Position + offset, 16, 16, data.Enum("dashAxis", Axes.Both), data.Int("colorValue")) {
		}

		private DashCollisionResults NewCollision(Player player, Vector2 direction) {
			if ((direction.X != 0 && !hitSide) || (direction.Y != 0 && !hitTop)) {
				return DashCollisionResults.NormalCollision;
			}

			StartShaking(.5f);

			animations.Play((direction.X == 0 ? (direction.Y > 0 ? "hittop" : "hitbottom") : (direction.X > 0 ? "hitleft" : "hitright")), true);
			Audio.Play("event:/char/madeline/landing", Center, "surface_index", 12);

			WallToggleData.Toggle(SwitchIndex);
			foreach (ToggleBlock block in Scene.Entities.FindAll<ToggleBlock>()) {
				block.SetState();
			}

			//if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "OutbackHelper" }))
			//	WallToggleData.UpdatePortals(SceneAs<Level>());

#if !USE_REFILLS
			if (!player.Inventory.NoRefills)
#endif
				player.RefillDash();

			return DashCollisionResults.Bounce;
		}
		public override void Added(Scene scene) {
			base.Added(scene);
			//WallToggleData.CheckForToggleInstance(scene);
		}

		public override void Render() {
			animations.Render();
			if (!hitSide || !hitTop)
				protector.Render();
			//Draw.Rect(Collider, Color.Gray);
			//Draw.HollowRect(Collider, ToggleBlock.BOX_COLORS[SwitchIndex]);
		}
	}

	public class ToggleSwitchTrigger : Trigger {
		private int color;
		private bool onlyOnce;
		private bool enable;

		public ToggleSwitchTrigger(EntityData data, Vector2 offset) : base(data, offset) {
			color = data.Int("color");
			onlyOnce = data.Bool("onlyOnce");
			enable = data.Bool("enable");
		}

		public override void OnEnter(Player player) {
			base.OnEnter(player);

			WallToggleData.SetEnabled(color, enable);
			
			if (onlyOnce) {
				RemoveSelf();
			}
		}
	}
}