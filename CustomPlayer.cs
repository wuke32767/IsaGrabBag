using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.IsaGrabBag
{
    public class CustomPlayer : Actor
    {

        public CustomPlayer(Player player) : base(player.Position)
        {
            Collider = new Hitbox(8, 8);
        }

        public override void Update()
        {
            base.Update();

            MoveH(Input.MoveX * Engine.DeltaTime * 100);
        }

        public override void Render()
        {
            base.Render();

            Draw.Rect(Collider, Color.White);
        }
    }
}
