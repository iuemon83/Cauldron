using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード除外アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfExcludeCard
    {
        public enum TypeValue
        {
            Excluded
        }

        public string ActionName { get; }

        public TypeValue Type { get; }

        public ActionContextCardsOfExcludeCard(
            string ActionName,
            TypeValue Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
