using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculator
    {
        public NumValueCalculatorForCard ForCard { get; }
        public NumValueCalculatorForCounter ForCounter { get; }

        public NumValueCalculator(
            NumValueCalculatorForCard ForCard = null,
            NumValueCalculatorForCounter ForCounter = null
            )
        {
            this.ForCard = ForCard;
            this.ForCounter = ForCounter;
        }
    }
}
