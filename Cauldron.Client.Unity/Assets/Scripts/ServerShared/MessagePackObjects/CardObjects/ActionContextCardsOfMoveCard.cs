using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード移動アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfMoveCard
    {
        public enum TypeValue
        {
            [DisplayText("移動したカード")]
            Moved
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextCardsOfMoveCard(
            string ActionName,
            TypeValue Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
