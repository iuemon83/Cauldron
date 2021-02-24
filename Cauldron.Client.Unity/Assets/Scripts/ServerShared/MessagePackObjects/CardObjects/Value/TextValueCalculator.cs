using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class TextValueCalculator
    {
        public enum ValueType
        {
            CardName,
        }

        public TextValueCalculator.ValueType Type { get; set; }
        public Choice CardsChoice { get; set; }

        public TextValueCalculator(
            TextValueCalculator.ValueType Type,
            Choice CardsChoice
            )
        {
            this.Type = Type;
            this.CardsChoice = CardsChoice;
        }
    }
}
