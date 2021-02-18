using Microsoft.Xna.Framework;
using System.Collections;
using Monocle;

namespace Celeste.Mod.IsaGrabBag
{
    class CustomNPC : NPC
    {
        private string npcValue, dialogue;
        private bool facingLeft;
        private BadelineDummy baddyDummy;

        public CustomNPC(Vector2 position, EntityData data) : base(position)
        {
            facingLeft = data.Bool("faceLeft", false);
            npcValue = data.Attr("npc", "granny");

            switch (npcValue)
            {
                default:
                    Add(Sprite = GFX.SpriteBank.Create(npcValue));
                    break;
                case "oshiro":
                    Add(Sprite = new OshiroSprite(facingLeft ? -1 : 1));
                    break;
                case "badeline":
                    Add(Sprite = (baddyDummy = new BadelineDummy(position)).Sprite);
                    break;
            }
        }
    }

    delegate IEnumerator NPCAction(string value);

    class CS_CustomNPC : CutsceneEntity
    {
        private const int ACTIONS_EXTRA = 1;
        public NPCAction[] actions;

        private string dialogue;

        public CS_CustomNPC(string dialogue, params NPCAction[] _actions)
        {
            actions = new NPCAction[_actions.Length + ACTIONS_EXTRA];
            for (int i = _actions.Length; i < _actions.Length + ACTIONS_EXTRA; i++)
            {

            }
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene()));
        }

        public IEnumerator Cutscene()
        {
            //yield return Textbox.Say(dialogue, actions);
            yield break;
        }
        public override void OnEnd(Level level)
        {

        }
    }
}
