using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード破壊アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfDestroyCard
    {
        public enum ValueType
        {
            Destroyed
        }

        public string ActionName { get; set; }

        public ActionContextCardsOfDestroyCard.ValueType Type { get; set; }

        public ActionContextCardsOfDestroyCard(
            string ActionName,
            ActionContextCardsOfDestroyCard.ValueType Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
