using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード選択アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfChoice
    {
        public enum TypeValue
        {
            [DisplayText("選択されたカード")]
            ChoiceCards
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextCardsOfChoice(
            string ActionName,
            TypeValue Type)
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
