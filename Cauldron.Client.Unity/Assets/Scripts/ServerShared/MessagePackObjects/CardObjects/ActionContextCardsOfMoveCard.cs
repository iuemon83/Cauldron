using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード移動アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfMoveCard
    {
        public enum ValueType
        {
            Moved
        }

        public string ActionName { get; set; }
        public ActionContextCardsOfMoveCard.ValueType Type { get; set; }

        public ActionContextCardsOfMoveCard(
            string ActionName,
            ActionContextCardsOfMoveCard.ValueType Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
