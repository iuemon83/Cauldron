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
