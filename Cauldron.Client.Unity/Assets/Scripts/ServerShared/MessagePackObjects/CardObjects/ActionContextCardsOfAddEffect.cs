using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード破壊アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfAddEffect
    {
        public enum ValueType
        {
            TargetCards
        }

        public string ActionName { get; set; }
        public ActionContextCardsOfAddEffect.ValueType Type { get; set; }

        public ActionContextCardsOfAddEffect(
            string ActionName,
            ActionContextCardsOfAddEffect.ValueType Type)
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
