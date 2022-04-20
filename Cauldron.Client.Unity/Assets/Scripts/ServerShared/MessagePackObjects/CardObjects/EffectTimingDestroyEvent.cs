using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDestroyEvent
    {
        public CardCondition[] OrCardConditions { get; }

        public EffectTimingDestroyEvent(CardCondition[] OrCardConditions)
        {
            this.OrCardConditions = OrCardConditions;
        }
    }
}
