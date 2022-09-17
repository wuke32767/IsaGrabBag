using System;
using System.IO;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using Celeste.Mod;
using System.Collections.Generic;

namespace Celeste.Mod.IsaGrabBag
{
    public class ArrowBlock : Solid
    {
        private const float MoveSpeed = 500, TurnSpeed = 750;
        private const bool JoystickAlways = false;

        private enum ArrowDirection {
            no_limit,
            horizontal,
            vertical,
            cardinal,
            diagonal
		}

        private bool UseAnalog
        {
            get
            {
                return (JoystickAlways || SaveData.Instance.Assists.ThreeSixtyDashing) && (limitation != ArrowDirection.cardinal && limitation != ArrowDirection.diagonal);
            }
        }

        public int InvertVal { get { return Inverted ? -1 : 1; } }
        public bool Inverted { get; private set; }
        public int Distance { get; private set; }

        private Vector2 moveDir, localPos, previousDirection;
        private Vector2 originalPosition;
        private ArrowDirection limitation;

        private Vector2 glowDirection;

        public ArrowBlock(EntityData _data, Vector2 _offset) : base(_data.Position + _offset, _data.Width, _data.Height, false)
        {
            Distance = _data.Int("distance", 16);
            Inverted = _data.Bool("inverted", false);
            originalPosition = _data.Position + _offset;

            limitation = _data.Enum("movementRestriction", ArrowDirection.no_limit);

            int num = (int)(base.Width / 8f) - 1;
            int num2 = (int)(base.Height / 8f) - 1;

            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("isafriend/objects/arrowblock/block");
            MTexture idle;

            switch (limitation) {
                default:
                    idle = atlasSubtextures[0];
                    break;
                case ArrowDirection.horizontal:
                    idle = atlasSubtextures[1];
                    break;
                case ArrowDirection.vertical:
                    idle = atlasSubtextures[2];
                    break;
                case ArrowDirection.cardinal:
                    idle = atlasSubtextures[3];
                    break;
                case ArrowDirection.diagonal:
                    idle = atlasSubtextures[4];
                    break;
            }

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

        private Vector2 GetOffsetPosition()
        {
            if (GrabBagModule.playerInstance == null || GrabBagModule.playerInstance.Dead)
                return Vector2.Zero;

            Vector2 move = UseAnalog ? Input.Feather.Value : new Vector2(Input.MoveX, Input.MoveY);

            if (move.X != 0 || move.Y != 0)
                move.Normalize();

            switch (limitation) {
                default:
                    break;
                case ArrowDirection.horizontal:
                    move.Y = 0;
                    if (move.X != 0)
                        move.X = Math.Sign(move.X);
                    break;
                case ArrowDirection.vertical:
                    move.X = 0;
                    if (move.Y != 0)
                        move.Y = Math.Sign(move.Y);
                    break;
                case ArrowDirection.cardinal:
                    if (move.X != 0 && move.Y != 0) {
                        move = Vector2.Zero;
					}

                    break;
                case ArrowDirection.diagonal:
                    move = move.Rotate(MathHelper.PiOver4);

                    if (Math.Abs(move.X) > 0.001f && Math.Abs(move.Y) > 0.001f) {
                        move = Vector2.Zero;
                    }
					else {
                        move = move.Rotate(-MathHelper.PiOver4);
                    }


                    break;
            }

            return move * InvertVal;
        }

        public override void Update()
        {
            base.Update();

            previousDirection = GetOffsetPosition();

            var newPos = GetOffsetPosition() * Distance;
            var dir = newPos - localPos;
            if (dir.X != 0 || dir.Y != 0)
            {
                dir.Normalize();
            }

            moveDir += dir * Engine.DeltaTime * TurnSpeed;
            if (moveDir.Length() > MoveSpeed)
            {
                moveDir = (moveDir.SafeNormalize() * MoveSpeed);
            }
            if (Calc.AbsAngleDiff(moveDir.Angle(), dir.Angle()) > MathHelper.PiOver2)
            {
                moveDir = Vector2.Zero;
            }

            var movement = moveDir * Engine.DeltaTime;

            if (Vector2.Distance(localPos, newPos) < movement.Length())
            {
                localPos = newPos;
                moveDir = Vector2.Zero;
            }
            else
            {
                localPos += movement;
            }

            if (localPos.Length() > Distance)
            {
                localPos.Normalize();
                localPos *= Distance;
                moveDir = Vector2.Zero;
			}

            glowDirection = Calc.Approach(glowDirection, previousDirection, 10 * Engine.DeltaTime);


            MoveTo(originalPosition + localPos);
        }
        private void AddImage(MTexture idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0) {
            MTexture subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8, null);
            Vector2 vector = new Vector2((float)(x * 8), (float)(y * 8));
            if (borderX != 0) {
                Add(new Image(subtexture) {
                    Color = Color.Black,
                    Position = vector + new Vector2((float)borderX, 0f)
                });
            }
            if (borderY != 0) {
                Add(new Image(subtexture) {
                    Color = Color.Black,
                    Position = vector + new Vector2(0f, (float)borderY)
                });
            }
            Image image = new Image(subtexture);
            image.Position = vector;
            Add(image);
            idleImages.Add(image);

            if (borderX != 0 || borderY != 0) {

                if (borderX < 0 && (limitation != ArrowDirection.diagonal && limitation != ArrowDirection.vertical)) {
                    Image image2 = new Image(GFX.Game["isafriend/objects/arrowblock/lit_left"].GetSubtexture(0, ty * 8, 8, 8, null));
                    activeLeftImages.Add(image2);
                    image2.Position = vector;
                    Add(image2);
                }
                else if (borderX > 0 && (limitation != ArrowDirection.diagonal && limitation != ArrowDirection.vertical)) {
                    Image image3 = new Image(GFX.Game["isafriend/objects/arrowblock/lit_right"].GetSubtexture(0, ty * 8, 8, 8, null));
                    activeRightImages.Add(image3);
                    image3.Position = vector;
                    Add(image3);
                }
                if (borderY < 0 && (limitation != ArrowDirection.diagonal && limitation != ArrowDirection.horizontal)) {
                    Image image4 = new Image(GFX.Game["isafriend/objects/arrowblock/lit_top"].GetSubtexture(tx * 8, 0, 8, 8, null));
                    activeTopImages.Add(image4);
                    image4.Position = vector;
                    Add(image4);
                }
                if (borderY > 0 && (limitation != ArrowDirection.diagonal && limitation != ArrowDirection.horizontal)) {
                    Image image5 = new Image(GFX.Game["isafriend/objects/arrowblock/lit_bottom"].GetSubtexture(tx * 8, 0, 8, 8, null));
                    activeBottomImages.Add(image5);
                    image5.Position = vector;
                    Add(image5);
                }
                if (borderX < 0 && borderY < 0) {
                    topleftCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_topleft"]);
                    topleftCorner.Position = vector;
                    Add(topleftCorner);
                }
                if (borderX < 0 && borderY > 0) {
                    bottomleftCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_bottomleft"]);
                    bottomleftCorner.Position = vector;
                    Add(bottomleftCorner);
                }
                if (borderX > 0 && borderY < 0) {
                    toprightCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_topright"]);
                    toprightCorner.Position = vector;
                    Add(toprightCorner);
                }
                if (borderX > 0 && borderY > 0) {
                    bottomrightCorner = new Image(GFX.Game["isafriend/objects/arrowblock/lit_bottomright"]);
                    bottomrightCorner.Position = vector;
                    Add(bottomrightCorner);
                }
            }
        }
        List<Image> idleImages = new List<Image>();
        List<Image> activeTopImages = new List<Image>();
        List<Image> activeRightImages = new List<Image>();
        List<Image> activeLeftImages = new List<Image>();
        List<Image> activeBottomImages = new List<Image>();
        Image topleftCorner, toprightCorner, bottomleftCorner, bottomrightCorner, face;

        public override void Render() {
            Rectangle rect = Collider.Bounds;

            face.Position = new Vector2(Width / 2, Height / 2) + glowDirection * 2;
            face.FlipY = Inverted;

            rect.Inflate(-3, -3);
            Draw.Rect(rect, Calc.HexToColor("483b69"));

            float lightStrength = Calc.ClampedMap(glowDirection.Y, 0, -.9f, 0, 1);
            foreach (var img in activeTopImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            lightStrength = Calc.ClampedMap(glowDirection.Y, 0, .9f, 0, 1);
            foreach (var img in activeBottomImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            lightStrength = Calc.ClampedMap(glowDirection.X, 0, -.9f, 0, 1);
            foreach (var img in activeLeftImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            lightStrength = Calc.ClampedMap(glowDirection.X, 0, .9f, 0, 1);
            foreach (var img in activeRightImages) {
                img.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
            }

            switch (limitation) {
				default: {

                    Vector2 dir = Calc.Rotate(glowDirection, MathHelper.PiOver4);

                    lightStrength = Calc.ClampedMap(dir.Y, 0, -.9f, 0, 1);
                    topleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(dir.X, 0, .9f, 0, 1);
                    toprightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(dir.X, 0, -.9f, 0, 1);
                    bottomleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(dir.Y, 0, .9f, 0, 1);
                    bottomrightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                }
                break;
                case ArrowDirection.horizontal: {

                    lightStrength = Calc.ClampedMap(glowDirection.X, 0, -.9f, 0, 1);
                    topleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    bottomleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(glowDirection.X, 0, .9f, 0, 1);
                    toprightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    bottomrightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                }
                break;
                case ArrowDirection.vertical: {

                    lightStrength = Calc.ClampedMap(glowDirection.Y, 0, -.9f, 0, 1);
                    topleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    toprightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);

                    lightStrength = Calc.ClampedMap(glowDirection.Y, 0, .9f, 0, 1);
                    bottomleftCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                    bottomrightCorner.Color = Color.Lerp(Color.Transparent, Color.White, lightStrength * lightStrength * lightStrength);
                }
                break;
                case ArrowDirection.cardinal:
                    topleftCorner.Visible = false;
                    toprightCorner.Visible = false;
                    bottomleftCorner.Visible = false;
                    bottomrightCorner.Visible = false;
                    break;
            }



            base.Render();

        }
    }
}