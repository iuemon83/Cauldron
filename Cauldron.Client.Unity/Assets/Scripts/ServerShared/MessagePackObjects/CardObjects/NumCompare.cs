using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class NumCompare
    {
        public enum CompareValue
        {
            [DisplayText("等しい")]
            Equality,
            [DisplayText("以下")]
            LessThan,
            [DisplayText("以上")]
            GreaterThan,
        }

        public int Value { get; }
        public NumCompare.CompareValue Compare { get; }

        public NumCompare(
            int Value,
            NumCompare.CompareValue Compare
            )
        {
            this.Value = Value;
            this.Compare = Compare;
        }
    }
}
