using Assets.Scripts.ServerShared.MessagePackObjects;
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
            [DisplayText("対象となったカード")]
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
