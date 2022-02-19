#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculator
    {
        public NumValueRandom? Random { get; }
        public NumValueCalculatorForPlayer? ForPlayer { get; }
        public NumValueCalculatorForCard? ForCard { get; }
        public NumValueCalculatorForCounter? ForCounter { get; }

        public NumValueCalculator(
            NumValueRandom? Random = null,
            NumValueCalculatorForPlayer? ForPlayer = null,
            NumValueCalculatorForCard? ForCard = null,
            NumValueCalculatorForCounter? ForCounter = null
            )
        {
            this.Random = Random;
            this.ForPlayer = ForPlayer;
            this.ForCard = ForCard;
            this.ForCounter = ForCounter;
        }
    }
}
