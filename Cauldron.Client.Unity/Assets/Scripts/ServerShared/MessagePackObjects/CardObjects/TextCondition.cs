using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class TextCondition
    {
        public enum CompareValue
        {
            /// <summary>
            /// 指定の文字列と完全一致
            /// </summary>
            [DisplayText("等しい")]
            Equality,

            /// <summary>
            /// 指定の文字列を含む
            /// </summary>
            [DisplayText("含む")]
            Contains,
        }

        public TextValue Value { get; }
        public TextCondition.CompareValue Compare { get; }
        public bool Not { get; }

        public TextCondition(
            TextValue Value,
            TextCondition.CompareValue Compare,
            bool not = false
            )
        {
            this.Value = Value;
            this.Compare = Compare;
            this.Not = not;
        }
    }
}
