using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectWhen
    {
        public EffectTiming Timing { get; }

        public EffectWhen(EffectTiming Timing)
        {
            this.Timing = Timing;
        }
    }
}
