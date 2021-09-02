using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カウンター変更アクションのコンテキスト（カウンター型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCountersOfModifyCounter
    {
        public enum TypeValue
        {
            Before,
            After,
            Modified,
        }

        public string ActionName { get; }

        public TypeValue Type { get; }

        public ActionContextCountersOfModifyCounter(
            string ActionName,
            TypeValue Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
