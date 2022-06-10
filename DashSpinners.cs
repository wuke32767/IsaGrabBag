using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.IsaGrabBag
{
	public class DreamSpinner : Entity
	{
		private static bool dreamdashEnabled = true;
		private DreamBlock block;
		private bool hasCollided, fake;

		public List<DreamSpinner> spinners;
		public List<Vector2> fillOffset;

		public int RNG;

		public bool Fragile;

		private static readonly Color debrisColor = new Color(0xC1, 0x8A, 0x53);

		public DreamSpinner(EntityData data, Vector2 offset, bool _fake)
			: this(data.Position + offset, data.Bool("useOnce", false), _fake)
		{
		}

		public DreamSpinner(Vector2 position, bool _useOnce, bool _fake)
		{
			Position = position;

			spinners = new List<DreamSpinner>();
			fillOffset = new List<Vector2>();

			Collider = new ColliderList(
				new Hitbox(16, 4, -8, -3),
				new Circle(6)
				);

			Add(new PlayerCollider(OnPlayer));
			Add(new LedgeBlocker());

			Fragile = _useOnce;
			fake = _fake;
			Visible = false;
			RNG = Calc.Random.Next(4);
			Depth = -8500;
			Collidable = false;
		}

		private void OnPlayer(Player player) {
			player.Die((player.Center - Center).SafeNormalize());
		}

		public override void Update()
		{
			if (fake)
				return;

			Player player = GrabBagModule.playerInstance;

			foreach (var deb in Scene.CollideAll<Actor>(block.Collider.Bounds))
			{
				if (deb is CrystalDebris)
					deb.RemoveSelf();
			}

			if (player != null)
			{
				dreamdashEnabled = player.Inventory.DreamDash;
				if (Fragile)
				{
					bool isColliding = block.Collidable && player.Collider.Collide(block);

					if (!isColliding && hasCollided)
					{
						RemoveSelf();
						block.RemoveSelf();

						Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
						CrystalDebris.Burst(Center, debrisColor, false, 4); //c18a53
						return;
					}
					hasCollided = isColliding;
				}
				if ((player.DashAttacking || player.StateMachine.State == 9) && dreamdashEnabled)
				{
					block.Collidable = true;
					Collidable = false;
				}
				else
				{
					block.Collidable = false;
					Collidable = true;

				}
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);

			if (!Fragile)
			{
				foreach (var spin in SceneAs<Level>().Entities.FindAll<DreamSpinner>())
				{
					if (spin == this || spin.Fragile || spin.spinners.Contains(this))
						continue;

					if (Vector2.Distance(spin.Position, Position) < 24)
					{
						spinners.Add(spin);
						fillOffset.Add((spin.Center + Center) / 2);
					}
				}
			}

			if (!fake)
			{
				scene.Add(block = new DreamBlock(Center - new Vector2(8, 8), 16, 16, null, false, false));
				block.Visible = false;

				if (GrabBagModule.GrabBagMeta.RoundDreamSpinner)
					block.Collider = new Circle(9f, 8, 8);
			}
		}

		public bool InView()
		{
			Camera camera = (Scene as Level).Camera;
			return X > camera.X - 16f && Y > camera.Y - 16f && X < camera.X + 320f + 16f && Y < camera.Y + 180f + 16f;
		}
	}

	struct Star
	{
		public Star(Vector2 _pos, int _depth, float _animSpeed, float _initAnim, byte _color)
		{
			point = _pos;
			depth = _depth;
			animSpeed = _animSpeed;
			anim = _initAnim;
			color = _color;
		}
		public Vector2 point;
		public int depth;
		public byte color;
		public float anim, animSpeed;
	}
	class DreamSpinnerBorder : Entity
	{
		internal static DreamSpinnerBorder mainInstance;

		public static void ReloadBorder(Scene scene) {

			if (mainInstance != null && scene != mainInstance.Scene) {
				mainInstance.RemoveSelf();
				mainInstance = null;
			}
			if (mainInstance == null) {
				scene.Add(new DreamSpinnerBorder());
			}
		}

		static Image spinnerMainTex, spinnerBGTex;
		static Image[] starTextures;
		static Effect borderEffect, starEffect, spinnerEffect;
		static DepthStencilState spinnerState;
		static DepthStencilState starState;
		static RenderTarget2D mainTarget;


		static bool dreamDashEnabled;

		static GameplayRenderer gameplayInstance;

		static Star[] stars;

		static Color[][] colors = new Color[][]{
			new Color[]{
				Calc.HexToColor("6383b8"),
				Calc.HexToColor("ba4a4a"),
				Calc.HexToColor("f0ff00"),
				Calc.HexToColor("9800b2"),
				Calc.HexToColor("9800b2"),
				Calc.HexToColor("3ca31d"),
				Calc.HexToColor("72e0ea")
			},
			new Color[]{
				Color.Orange,
				Color.Orange,
				Color.Orange,
				Color.Orange,
				Color.Orange,
				Color.Orange,
				Color.Orange,
			},
			new Color[]{
				Calc.HexToColor("666666"),
				Calc.HexToColor("666666"),
				Calc.HexToColor("666666"),
				Calc.HexToColor("666666"),
				Calc.HexToColor("666666"),
				Calc.HexToColor("666666"),
				Calc.HexToColor("666666")
			}

		};

		public const int SPIN_TYPE_COUNT = 4;
		public const int SPRITE_MAIN = 21, SPRITE_FILLER = 14;

		const int starCount = 700;
		const int depthDiv = starCount / 5;

		private const int WIDTH = 320 + 2, HEIGHT = 180 + 2;

		private Vector2 camPos, lastCamPos;

		public static void LoadTextures() {

			spinnerState = new DepthStencilState();
			starState = new DepthStencilState();

			mainTarget = new RenderTarget2D(Draw.SpriteBatch.GraphicsDevice, WIDTH, HEIGHT, false, SurfaceFormat.Color, DepthFormat.Depth16);

			spinnerState.DepthBufferFunction = CompareFunction.Greater;
			spinnerState.DepthBufferEnable = true;
			spinnerState.DepthBufferWriteEnable = true;
			spinnerState.StencilDepthBufferFail = StencilOperation.Keep;
			spinnerState.StencilPass = StencilOperation.Replace;

			starState.DepthBufferFunction = CompareFunction.Equal;
			starState.DepthBufferEnable = true;
			starState.DepthBufferWriteEnable = false;
			starState.StencilDepthBufferFail = StencilOperation.Keep;
			starState.StencilPass = StencilOperation.Keep;

			spinnerMainTex = new Image(GFX.Game["isafriend/danger/crystal/fg_dream"]);
			spinnerBGTex = new Image(GFX.Game["isafriend/danger/crystal/bg_dream"]);

			starTextures = new Image[] {
				new Image(GFX.Game["isafriend/danger/crystal/star0"]),
				new Image(GFX.Game["isafriend/danger/crystal/star1"]),
				new Image(GFX.Game["isafriend/danger/crystal/star2"]),
				null
			};
			starTextures[3] = starTextures[1];

			spinnerMainTex.CenterOrigin();
			spinnerBGTex.CenterOrigin();

			try {
				var asset = Everest.Content.Get("Effects/dream_border.cso");
				borderEffect = new Effect(Draw.SpriteBatch.GraphicsDevice, asset.Data);

				asset = Everest.Content.Get("Effects/dream_stars.cso");
				starEffect = new Effect(Draw.SpriteBatch.GraphicsDevice, asset.Data);

				asset = Everest.Content.Get("Effects/dream_spinners.cso");
				spinnerEffect = new Effect(Draw.SpriteBatch.GraphicsDevice, asset.Data);
			}
			catch (Exception e) {
				Logger.Log(LogLevel.Error, "IsaGrabBag", "Failed to load the shader");
				Logger.Log(LogLevel.Error, "IsaGrabBag", "Exception: \n" + e.ToString());
			}

			stars = new Star[starCount];

			Calc.PushRandom(0x12F3);

			for (int i = 0; i < starCount; ++i) {	
				int depth = i / depthDiv;
				depth = 4 - depth;

				float initAnim = 0;
				if (depth < 2) {
					initAnim = Calc.Random.NextFloat(4f);
				}
				else if (depth < 4) {
					initAnim = Calc.Random.NextFloat(2f);
				}
				stars[i] = new Star(
					new Vector2(Calc.Random.NextFloat(WIDTH), Calc.Random.NextFloat(HEIGHT)),
					depth, Calc.Random.NextFloat(1f) + 5f, initAnim, (byte)Calc.Random.Next(0, 7));
			}

			Calc.PopRandom();
		}
		public static void OnLoad() {	
			On.Celeste.GameplayRenderer.ctor += GameplayRenderer_ctor;
			Everest.Events.Level.OnExit += Level_OnExit;
			On.Celeste.Level.ctor += Level_ctor;

		}
		public static void OnUnload() {
			On.Celeste.GameplayRenderer.ctor -= GameplayRenderer_ctor;
			Everest.Events.Level.OnExit -= Level_OnExit;
			On.Celeste.Level.ctor -= Level_ctor;

		}

		private static void Level_ctor(On.Celeste.Level.orig_ctor orig, Level self) {
			orig(self);
			ReloadBorder(self);
		}

		private static void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
			if (mainInstance != null) {

				mainInstance.RemoveSelf();
				mainInstance = null;
			}
		}
		private static void GameplayRenderer_ctor(On.Celeste.GameplayRenderer.orig_ctor orig, GameplayRenderer self) {
			orig(self);
			gameplayInstance = self;
		}



		public DreamSpinnerBorder() {
			Depth = -8500;

			mainInstance = this;
			AddTag(Tags.Global);
		}

		public override void Awake(Scene scene)
		{
			Update();
			base.Awake(scene);
		}
		public override void Added(Scene scene)
		{
			base.Added(scene);

			camPos = SceneAs<Level>().Camera.Position;
			camPos.X = (int)camPos.X;
			camPos.Y = (int)camPos.Y;
			lastCamPos = camPos;
		}

		public override void Update()
		{
			base.Update();

			if (GrabBagModule.playerInstance != null && !GrabBagModule.playerInstance.Dead) {
				dreamDashEnabled = GrabBagModule.playerInstance.Inventory.DreamDash;
			}

			if (dreamDashEnabled && mainInstance == this) {

				for (int i = 0; i < starCount; ++i) {

					if (stars[i].depth < 2) {
						stars[i].anim += stars[i].animSpeed * Engine.DeltaTime;
						if (stars[i].anim >= 4)
							stars[i].anim -= 4;
					}
					else if (stars[i].depth < 4) {
						stars[i].anim += stars[i].animSpeed * Engine.DeltaTime;
						if (stars[i].anim >= 2)
							stars[i].anim -= 2;
					}
				}

			}

			base.Update();
		}

		public override void Render()
		{
			base.Render();

			if (Scene.Entities.FindFirst<DreamSpinner>() == null) {
				return;
			}

			camPos = SceneAs<Level>().Camera.Position;
			camPos.X = (int)camPos.X;
			camPos.Y = (int)camPos.Y;

			for (int i = 0; i < starCount; ++i) {

				Vector2 point = stars[i].point;
				point -= (camPos - lastCamPos) * (5 - stars[i].depth) * 0.15f;
				if (point.X < -4)
					point.X += WIDTH + 4;
				if (point.X >= WIDTH)
					point.X -= WIDTH + 4;
				if (point.Y < -4)
					point.Y += HEIGHT + 4;
				if (point.Y >= HEIGHT)
					point.Y -= HEIGHT + 4;

				stars[i].point = point;

			}

			lastCamPos = camPos;

			

			GameplayRenderer.End();
			DrawTexture(false);
			DrawTexture(true);
			GameplayRenderer.Begin();
		}

		private void DrawTexture(bool isFragile) {

			if (GrabBagModule.playerInstance != null && !GrabBagModule.playerInstance.Dead) {
				dreamDashEnabled = GrabBagModule.playerInstance.Inventory.DreamDash;
			}

			var gd = Draw.SpriteBatch.GraphicsDevice;

			gd.SetRenderTarget(mainTarget);
			gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 0f, 0);

			// Start rendering dream spinners
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, spinnerState, RasterizerState.CullNone,
				spinnerEffect, gameplayInstance.Camera.Matrix);

			if (!dreamDashEnabled) {
				spinnerMainTex.Color = new Color(25, 25, 25);
			}
			else if (isFragile) {
				spinnerMainTex.Color = new Color(30, 22, 10);
			}
			else {
				spinnerMainTex.Color = Color.Black;
			}
			spinnerBGTex.Color = spinnerMainTex.Color;

			foreach (var spin in Scene.Entities.FindAll<DreamSpinner>()) {
				if (spin.Fragile != isFragile || !spin.InView())
					continue;

				int type = spin.RNG;

				spinnerMainTex.RenderPosition = spin.Position;
				spinnerMainTex.Rotation = (type & 0x3) * MathHelper.PiOver2;

				spinnerMainTex.Render();

				foreach (Vector2 pos in spin.fillOffset) {
					spinnerBGTex.RenderPosition = pos;
					spinnerBGTex.Render();
				}
			}

			Draw.SpriteBatch.End();


			// Draw stars on spinner
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, starState, RasterizerState.CullNone,
				starEffect, Matrix.Identity);

			Color[] array;

			if (!dreamDashEnabled) {
				array = colors[2];
			}
			else if (isFragile) {
				array = colors[1];
			}
			else {
				array = colors[0];
			}


			for (int i = 0; i < starCount; ++i) {

				var image = starTextures[(int)stars[i].anim];
				if (isFragile && stars[i].depth < 2)
					image = starTextures[0];
				image.Color = array[stars[i].color];
				image.RenderPosition = stars[i].point;
				image.Render();
			}

			Draw.SpriteBatch.End();


			gd.SetRenderTarget(GameplayBuffers.Gameplay);

			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
				borderEffect, gameplayInstance.Camera.Matrix);

			if (!dreamDashEnabled) {
				Draw.SpriteBatch.Draw(mainTarget, gameplayInstance.Camera.Position, Color.Gray);
			}
			else if (isFragile) {
				Draw.SpriteBatch.Draw(mainTarget, gameplayInstance.Camera.Position, Color.Orange * 0.9f);
			}
			else {
				Draw.SpriteBatch.Draw(mainTarget, gameplayInstance.Camera.Position, Color.White);
			}

			Draw.SpriteBatch.End();

		}
	}
}