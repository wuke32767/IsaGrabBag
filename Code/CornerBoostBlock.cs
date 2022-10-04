using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.IsaGrabBag {
    [CustomEntity("isaBag/cornerBlock")]
    [Tracked(false)]
    public class CornerBoostBlock : Solid {
        private readonly char _tileset;
        private readonly List<CornerBoostBlock> Group = new();
        private TileGrid tiles;
        private bool HasGroup;
        private Point GroupBoundsMin, GroupBoundsMax;

        public CornerBoostBlock(Vector2 position, int width, int height, char tile, bool useTileset)
            : base(position, width, height, true) {
            _tileset = useTileset ? tile : '\0';
            OnCollide = OnCollision;
        }

        public CornerBoostBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("useTileset", false)) {
        }

        public bool CustomTile => _tileset != '\0';

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

        public override void Awake(Scene scene) {
            if (!HasGroup) {
                GroupBoundsMin = new Point((int)X, (int)Y);
                GroupBoundsMax = new Point((int)Right, (int)Bottom);

                AddToGroupAndFindChildren(this);

                Rectangle rectangle = new(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8);
                VirtualMap<char> virtualMap = new(rectangle.Width, rectangle.Height, '0');

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
                    Vector2 texOffset = new(0.255127f, 0.4956055f);
                    Vector2 texSize = new(0.01164f, 0.0291f);
                    const int width = 6, height = 15;

                    TileGrid template = tiles;
                    tiles = new TileGrid(8, 8, rectangle.Width, rectangle.Height);

                    MTexture tex = GFX.Game["isafriend/tilesets/boost_block"];
                    Tileset tileset = new(tex, 8, 8);

                    for (int y = 0; y < rectangle.Height; ++y) {
                        for (int x = 0; x < rectangle.Width; ++x) {

                            MTexture fromTemplate = template.Tiles[x, y];

                            if (fromTemplate == null) {
                                continue;
                            }

                            float u = (fromTemplate.LeftUV - texOffset.X) / texSize.X;
                            float v = (fromTemplate.TopUV - texOffset.Y) / texSize.Y;

                            if (u is < 0 and > (-0.0001f)) {
                                u = 0;
                            }

                            if (v is < 0 and > (-0.0001f)) {
                                v = 0;
                            }

                            if (u < 0 || v < 0 || u >= 1 || v >= 1) {
                                break;
                            }

                            tiles.Tiles[x, y] = tileset[(int)(u * width), (int)(v * height)];
                        }
                    }
                }

                tiles.Position = new Vector2(GroupBoundsMin.X - X, GroupBoundsMin.Y - Y);

                Add(tiles);
            }

            base.Awake(scene);
        }

        private static Rectangle GetFacingHitbox(Player player) {
            Rectangle hitbox = player.Collider.Bounds;
            hitbox.Width = 1;

            if (player.Facing == Facings.Left) {
                hitbox.X -= 1;
            } else {
                hitbox.X += (int)player.Width;
            }

            hitbox.Y -= 1;
            hitbox.Height += 1;

            return hitbox;
        }

        private static void Player_ClimbEnd(On.Celeste.Player.orig_ClimbEnd orig, Player self) {
            float timer = DynamicData.For(self).Get<float>("wallSpeedRetentionTimer");

            orig(self);

            if (self.Scene.CollideCheck<CornerBoostBlock>(GetFacingHitbox(self))) {
                DynamicData.For(self).Set("wallSpeedRetentionTimer", timer);
            }
        }

        private static void Player_NormalEnd(On.Celeste.Player.orig_NormalEnd orig, Player self) {
            float timer = DynamicData.For(self).Get<float>("wallSpeedRetentionTimer");

            orig(self);

            if (self.StateMachine.State == Player.StClimb && self.Scene.CollideCheck<CornerBoostBlock>(GetFacingHitbox(self))) {
                DynamicData.For(self).Set("wallSpeedRetentionTimer", timer);
            }
        }

        private static void OnClimbJumped(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            orig(self);

            float timer = DynamicData.For(self).Get<float>("wallSpeedRetentionTimer");
            if (self.Scene.CollideCheck<CornerBoostBlock>(GetFacingHitbox(self)) && timer > 0f) {
                DynamicData.For(self).Set("wallSpeedRetentionTimer", Math.Max(timer, 0.06f));

                float speed = DynamicData.For(self).Get<float>("wallSpeedRetained");
                int moveX = DynamicData.For(self).Get<int>("moveX");
                DynamicData.For(self).Set("retentionSpeed", speed + (40 * moveX));
            }
        }

        private void OnCollision(Vector2 dir) {

            Player player = GrabBagModule.playerInstance;
            player ??= Scene.Entities.FindFirst<Player>();

            if (dir.X == 0 || player == null) {
                return;
            }

            if (DynamicData.For(player).Get<float>("wallSpeedRetentionTimer") == 0.06f) {
                DynamicData.For(player).Set("wallSpeedRetentionTimer", 0.12f);
            }
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

                if (!block.HasGroup && block._tileset == _tileset &&
                    (Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), block) ||
                    Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), block))) {
                    AddToGroupAndFindChildren(block);
                }
            }
        }
    }
}