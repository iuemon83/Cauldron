using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード追加アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfAddCard
    {
        public enum TypeValue
        {
            [DisplayText("追加されたカード")]
            AddCards
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextCardsOfAddCard(
            string ActionName,
            TypeValue Type)
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
