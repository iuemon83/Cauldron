using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueRandom
    {
        public int Min { get; }
        public int Max { get; }

        public NumValueRandom(
            int Min,
            int Max
            )
        {
            this.Min = Min;
            this.Max = Max;
        }
    }
}
