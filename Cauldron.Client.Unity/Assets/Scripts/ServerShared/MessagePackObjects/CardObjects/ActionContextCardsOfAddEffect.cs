using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード破壊アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfAddEffect
    {
        public enum TypeValue
        {
            TargetCards
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextCardsOfAddEffect(
            string ActionName,
            TypeValue Type)
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
