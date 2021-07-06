using System;
using System.IO;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste;
using Celeste.Mod;

namespace Celeste.Mod.IsaGrabBag
{
    public class ArrowBlock : Solid
    {
        private const float MoveSpeed = 500, TurnSpeed = 750;
        private const bool JoystickAlways = false;

        private static bool UseAnalog
        {
            get
            {
                return JoystickAlways || SaveData.Instance.Assists.ThreeSixtyDashing;
            }
        }

        public int InvertVal { get { return Inverted ? -1 : 1; } }
        public bool Inverted { get; private set; }
        public int Distance { get; private set; }

        private Vector2 moveDir, localPos;
        private Vector2 originalPosition;

        public ArrowBlock(EntityData _data, Vector2 _offset) : base(_data.Position + _offset, _data.Width, _data.Height, false)
        {
            Distance = _data.Int("distance", 16);
            Inverted = _data.Bool("inverted", false);
            originalPosition = _data.Position + _offset;
        }

        private Vector2 GetOffsetPosition()
        {
            if (GrabBagModule.playerInstance == null || GrabBagModule.playerInstance.Dead)
                return Vector2.Zero;

            Vector2 move = UseAnalog ? Input.Feather.Value : new Vector2(Input.MoveX, Input.MoveY);

            if (move.X != 0 || move.Y != 0)
                move.Normalize();

            return move * InvertVal;
        }

        public override void Update()
        {
            base.Update();

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

            MoveTo(originalPosition + localPos);
        }

        public override void Render()
        {
            base.Render();

            Rectangle rect = Collider.Bounds;

            Draw.Rect(rect, Color.Black);
            rect.Inflate(-1, -1);
            Draw.Rect(rect, Inverted ? Color.Red : Color.White);

        }
    }
}