using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectWhen
    {
        [Key(1)]
        public EffectTiming Timing { get; set; }

        public EffectWhen(EffectTiming Timing)
        {
            this.Timing = Timing;
        }
    }
}
