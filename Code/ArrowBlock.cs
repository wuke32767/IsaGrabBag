using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.IsaGrabBag {
    [CustomEntity("isaBag/arrowBlock")]
    public class ArrowBlock : Solid {
        private const float MoveSpeed = 500;
        private const float TurnSpeed = 750;

        private readonly ArrowDirection limitation;
        private readonly List<Image> idleImages = new();
        private readonly List<Image> activeTopImages = new();
        private readonly List<Image> activeRightImages = new();
        private readonly List<Image> activeLeftImages = new();
        private readonly List<Image> activeBottomImages = new();
        private readonly Image face;
        private Vector2 moveDir, localPos, previousDirection;
        private Vector2 originalPosition;
        private Vector2 glowDirection;
        private Image topleftCorner;
        private Image toprightCorner;
        private Image bottomleftCorner;
        private Image bottomrightCorner;

        public ArrowBlock(EntityData _data, Vector2 _offset)
            : base(_data.Position + _offset, _data.Width, _data.Height, false) {
            Distance = _data.Int("distance", 16);
            Inverted = _data.Bool("inverted", false);
            originalPosition = _data.Position + _offset;

            limitation = _data.Enum("movementRestriction", ArrowDirection.no_limit);

            int num = (int)(Width / 8f) - 1;
            int num2 = (int)(Height / 8f) - 1;

            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("isafriend/objects/arrowblock/block");
            MTexture idle = limitation switch {
                ArrowDirection.horizontal => atlasSubtextures[1],
                ArrowDirection.vertical => atlasSubtextures[2],
                ArrowDirection.cardinal => atlasSubtextures[3],
                ArrowDirection.diagonal => atlasSubtextures[4],
                _ => atlasSubtextures[0],
            };
            face = new Image(GFX.Game["isafriend/objects/arrowblock/idle_face"]);
            face.CenterOrigin();
            face.Position = new Vector2(Width / 2, Height / 2);

            Add(face);

            for (int i = 1; i < num; i++) {
                AddImage(idle, i, 0, Calc.Random.Choose(1, 2), 0, 0, -1);
                AddImage(idle, i, num2, Calc.Random.Choose(1, 2), 3, 0, 1);
            }

            for (int j = 1; j < num2; j++) {
                AddImage(idle, 0, j, 0, Calc.Random.Choose(1, 2), -1, 0);
                AddImage(idle, num, j, 3, Calc.Random.Choose(1, 2), 1, 0);
            }

            AddImage(idle, 0, 0, 0, 0, -1, -1);
            AddImage(idle, num, 0, 3, 0, 1, -1);
            AddImage(idle, 0, num2, 0, 3, -1, 1);
            AddImage(idle, num, num2, 3, 3, 1, 1);

            Add(new LightOcclude(0.2f));
        }

        private enum ArrowDirection {
            no_limit,
            horizontal,
            vertical,
            cardinal,
            diagonal
        }

        public int InvertVal => Inverted ? -1 : 1;
        public bool Inverted { get; private set; }
        public int Distance { get; private set; }

        public override void Update() {
            base.Update();

            previousDirection = GetOffsetPosition();

            Vector2 newPos = GetOffsetPosition() * Distance;
            Vector2 dir = newPos - localPos;
            if (dir.X != 0 || dir.Y != 0) {
                dir.Normalize();
            }

            moveDir += dir * Engine.DeltaTime * TurnSpeed;
            if (moveDir.Length() > MoveSpeed) {
                moveDir = moveDir.SafeNormalize() * MoveSpeed;
            }

            if (Calc.AbsAngleDiff(moveDir.Angle(), dir.Angle()) > MathHelper.PiOver2) {
                moveDir = Vector2.Zero;
            }

            Vector2 movement = moveDir * Engine.DeltaTime;

            if (Vector2.Distance(localPos, newPos) < movement.Length()) {
                localPos = newPos;
                moveDir = Vector2.Zero;
            } else {
                localPos += movement;
            }

            if (localPos.Length() > Distance) {
                localPos.Normalize();
                localPos *= Distance;
                moveDir = Vector2.Zero;
            }

            glowDirection = Calc.Approach(glowDirection, previousDirection, 10 * Engine.DeltaTime);

            MoveTo(originalPosition + localPos);
        }

        public override void Render() {
            Rectangle rect = Collider.Bounds;

            face.Position = new Vector2(Width / 2, Height / 2) + (glowDirection * 2);
            face.FlipY = Inverted;

            rect.Inflate(-3, -3);
            Draw.Rect(rect, Calc.HexToColor("483b69"));

            float lightStrength = Calc.ClampedMap(glowDirection.Y, 0, -.9f, 0, 1);
            foreach (Image img in activeTopImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            lightStrength = Calc.ClampedMap(glowDirection.Y, 0, .9f, 0, 1);
            foreach (Image img in activeBottomImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            lightStrength = Calc.ClampedMap(glowDirection.X, 0, -.9f, 0, 1);
            foreach (Image img in activeLeftImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            lightStrength = Calc.ClampedMap(glowDirection.X, 0, .9f, 0, 1);
            foreach (Image img in activeRightImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            switch (limitation) {
                case ArrowDirection.horizontal:
                    lightStrength = Calc.ClampedMap(glowDirection.X, 0, -.9f, 0, 1);
                    topleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    bottomleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(glowDirection.X, 0, .9f, 0, 1);
                    toprightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    bottomrightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    break;
                case ArrowDirection.vertical:
                    lightStrength = Calc.ClampedMap(glowDirection.Y, 0, -.9f, 0, 1);
                    topleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    toprightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(glowDirection.Y, 0, .9f, 0, 1);
                    bottomleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    bottomrightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    break;
                case ArrowDirection.cardinal:
                    topleftCorner.Visible = false;
                    toprightCorner.Visible = false;
                    bottomleftCorner.Visible = false;
                    bottomrightCorner.Visible = false;
                    break;
                default:
                    Vector2 dir = Calc.Rotate(glowDirection, MathHelper.PiOver4);

                    lightStrength = Calc.ClampedMap(dir.Y, 0, -.9f, 0, 1);
                    topleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(dir.X, 0, .9f, 0, 1);
                    toprightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(dir.X, 0, -.9f, 0, 1);
                    bottomleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(dir.Y, 0, .9f, 0, 1);
                    bottomrightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    break;
            }

            base.Render();
        }

        /// <summary>
        /// Get the direction the player is holding, snapped to eight directions and normalized,
        /// or the zero vector if no direction is held. Mostly copied from Celeste.Input.GetAimVector().
        /// </summary>
        /// <returns>A normalized vector on a cardinal or diagonal or the zero vector.</returns>
        private static Vector2 GetEightDirectionalAim() {
            Vector2 value = Input.Feather.Value;
            if (value == Vector2.Zero) {
                return Vector2.Zero;
            }

            float angle = value.Angle();
            float angleThreshold = (float)Math.PI / 8f;
            if (angle < 0) {
                angleThreshold -= Calc.ToRad(5f);
            }

            if (Calc.AbsAngleDiff(angle, 0f) < angleThreshold) {
                return new Vector2(1f, 0f);
            } else if (Calc.AbsAngleDiff(angle, (float)Math.PI) < angleThreshold) {
                return new Vector2(-1f, 0f);
            } else if (Calc.AbsAngleDiff(angle, -(float)Math.PI / 2f) < angleThreshold) {
                return new Vector2(0f, -1f);
            } else if (Calc.AbsAngleDiff(angle, (float)Math.PI / 2f) < angleThreshold) {
                return new Vector2(0f, 1f);
            } else {
                return new Vector2(Math.Sign(value.X), Math.Sign(value.Y)).SafeNormalize();
            }
        }

        private Vector2 GetOffsetPosition() {
            if (GrabBagModule.playerInstance == null || GrabBagModule.playerInstance.Dead) {
                return Vector2.Zero;
            }

            Vector2 move;
            if (SaveData.Instance.Assists.ThreeSixtyDashing && limitation == ArrowDirection.no_limit) {
                move = Input.Feather.Value.SafeNormalize();
            } else {
                move = GetEightDirectionalAim();
                switch (limitation) {
                    case ArrowDirection.horizontal:
                        move.Y = 0;
                        if (move.X != 0) {
                            move.X = Math.Sign(move.X);
                        }

                        break;
                    case ArrowDirection.vertical:
                        move.X = 0;
                        if (move.Y != 0) {
                            move.Y = Math.Sign(move.Y);
                        }

                        break;
                    case ArrowDirection.cardinal:
                        if (move.X != 0 && move.Y != 0) {
                            move = Vector2.Zero;
                        }

                        break;
                    case ArrowDirection.diagonal:
                        if (move.X == 0 || move.Y == 0) {
                            move = Vector2.Zero;
                        }

                        break;
                }
            }

            return move * InvertVal;
        }

        private void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0) {
            MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8, null);
            Vector2 vector = new(x * 8, y * 8);
            if (borderX != 0) {
                Add(new Image(subtexture) {
                    Color = Color.Black,
                    Position = vector + new Vector2(borderX, 0f)
                });
            }

            if (borderY != 0) {
                Add(new Image(subtexture) {
                    Color = Color.Black,
                    Position = vector + new Vector2(0f, borderY)
                });
            }

            Image image = new(subtexture) {
                Position = vector
            };
            Add(image);
            idleImages.Add(image);

            if (borderX != 0 || borderY != 0) {

                if (borderX < 0 && limitation != ArrowDirection.diagonal && limitation != ArrowDirection.vertical) {
                    Image image2 = new(GFX.Game["isafriend/objects/arrowblock/lit_left"].GetSubtexture(0, ty * 8, 8, 8, null));
                    activeLeftImages.Add(image2);
                    image2.Position = vector;
                    Add(image2);
                } else if (borderX > 0 && limitation != ArrowDirection.diagonal && limitation != ArrowDirection.vertical) {
                    Image image3 = new(GFX.Game["isafriend/objects/arrowblock/lit_right"].GetSubtexture(0, ty * 8, 8, 8, null));
                    activeRightImages.Add(image3);
                    image3.Position = vector;
                    Add(image3);
                }

                if (borderY < 0 && limitation != ArrowDirection.diagonal && limitation != ArrowDirection.horizontal) {
                    Image image4 = new(GFX.Game["isafriend/objects/arrowblock/lit_top"].GetSubtexture(tx * 8, 0, 8, 8, null));
                    activeTopImages.Add(image4);
                    image4.Position = vector;
                    Add(image4);
                }

                if (borderY > 0 && limitation != ArrowDirection.diagonal && limitation != ArrowDirection.horizontal) {
                    Image image5 = new(GFX.Game["isafriend/objects/arrowblock/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8, null));
                    activeBottomImages.Add(image5);
                    image5.Position = vector;
                    Add(image5);
                }

                if (borderX < 0 && borderY < 0) {
                    topleftCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_topleft"]) {
                        Position = vector
                    };
                    Add(topleftCorner);
                }

                if (borderX < 0 && borderY > 0) {
                    bottomleftCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_bottomleft"]) {
                        Position = vector
                    };
                    Add(bottomleftCorner);
                }

                if (borderX > 0 && borderY < 0) {
                    toprightCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_topright"]) {
                        Position = vector
                    };
                    Add(toprightCorner);
                }

                if (borderX > 0 && borderY > 0) {
                    bottomrightCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_bottomright"]) {
                        Position = vector
                    };
                    Add(bottomrightCorner);
                }
            }
        }
    }
}