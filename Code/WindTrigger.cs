using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.IsaGrabBag
{
    public class HotColdWind : Trigger
    {
        public WindController.Patterns windHot, windCold;
        private bool freezeDreamBlocks;
        private static bool current, dreamBlocksCurrent;

        public HotColdWind(EntityData data, Vector2 offset) : base(data, offset)
        {
            windHot = data.Enum("patternHot", WindController.Patterns.Up);
            windCold = data.Enum("patternCold", WindController.Patterns.Down);
            freezeDreamBlocks = data.Bool("freezeDream", false);
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);

            WindController.Patterns currentWind = current ? windHot : windCold;
            SetWind(Scene, currentWind);
        }
        public override void OnStay(Player player)
        {
            base.OnStay(player);

            if (SceneAs<Level>().CoreMode == Session.CoreModes.Hot != current)
            {
                current = !current;
                WindController.Patterns currentWind = current ? windHot : windCold;
                
                SetWind(Scene, currentWind);
            }
        }

        public static void SetWind(Monocle.Scene scene, WindController.Patterns currentWind)
        {
            WindController wind = scene.Entities.FindFirst<WindController>();
            if (wind == null)
            {
                wind = new WindController(currentWind);
                scene.Add(wind);
                return;
            }
            wind.SetPattern(WindController.Patterns.None);
            wind.SetPattern(currentWind);

            //if (_freeze)
            //{
            //    List<DreamBlock> dreamblocks = scene.Entities.FindAll<DreamBlock>();

            //    Player player = scene.Entities.FindFirst<Player>();
            //    if (player != null)
            //    {
            //        (scene as Level).Session.Inventory.DreamDash = current;
            //        foreach (DreamBlock block in dreamblocks)
            //        {
            //            block.Added(scene);
            //        }
            //    }
            //}
        }
    }
    public class WindAgainstPlayer : Trigger
    {
        WindController.Patterns left, right;

        public WindAgainstPlayer(EntityData data, Vector2 offset) : base(data, offset)
        {
            if (data.Attr("strongWind", "false").ToLower() == "true")
            {
                left = WindController.Patterns.LeftStrong;
                right = WindController.Patterns.RightStrong;
            }
            else
            {
                left = WindController.Patterns.Left;
                right = WindController.Patterns.Right;
            }
        }

        public override void OnStay(Player player)
        {
            WindController.Patterns pattern = WindController.Patterns.None;
            if (Input.MoveX != 0)
            {
                HotColdWind.SetWind(Scene, Input.MoveX > 0 ? left : right);
            }
        }

    }
}