using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カウンター変更アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfModifyCounter
    {
        public enum TypeValue
        {
            Modified
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextCardsOfModifyCounter(
            string ActionName,
            TypeValue Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
