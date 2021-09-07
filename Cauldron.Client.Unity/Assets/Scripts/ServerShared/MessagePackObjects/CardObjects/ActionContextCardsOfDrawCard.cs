using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// ドローアクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfDrawCard
    {
        public enum TypeValue
        {
            DrawnCards
        }

        public string ActionName { get; }

        public TypeValue Type { get; }

        public ActionContextCardsOfDrawCard(
            string ActionName,
            TypeValue Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
