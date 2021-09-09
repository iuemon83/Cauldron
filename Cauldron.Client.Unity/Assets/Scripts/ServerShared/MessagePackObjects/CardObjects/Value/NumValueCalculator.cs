using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculator
    {
        public NumValueRandom Random { get; }
        public NumValueCalculatorForCard ForCard { get; }
        public NumValueCalculatorForCounter ForCounter { get; }

        public NumValueCalculator(
            NumValueRandom Random = null,
            NumValueCalculatorForCard ForCard = null,
            NumValueCalculatorForCounter ForCounter = null
            )
        {
            this.Random = Random;
            this.ForCard = ForCard;
            this.ForCounter = ForCounter;
        }
    }
}
