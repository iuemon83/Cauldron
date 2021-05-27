using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculator
    {
        public enum ValueType
        {
            [DisplayText("カウント")]
            Count,
            [DisplayText("カードのコスト")]
            CardCost,
        }

        public NumValueCalculator.ValueType Type { get; set; }

        public Choice CardsChoice { get; set; }

        public NumValueCalculator(
            NumValueCalculator.ValueType Type,
           Choice CardsChoice)
        {
            this.Type = Type;
            this.CardsChoice = CardsChoice;
        }
    }
}
