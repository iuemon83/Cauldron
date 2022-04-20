using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// 効果追加アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfAddEffect
    {
        public enum TypeValue
        {
            [DisplayText("対象となったカード")]
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
