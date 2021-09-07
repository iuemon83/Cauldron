using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// ダメージアクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfDamage
    {
        public enum TypeValue
        {
            DamagedCards
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextCardsOfDamage(
            string ActionName,
            TypeValue Type)
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
