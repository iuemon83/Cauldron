using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculator
    {
        public enum ValueType
        {
            Count,
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
