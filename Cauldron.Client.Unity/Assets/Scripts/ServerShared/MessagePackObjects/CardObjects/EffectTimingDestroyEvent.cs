using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDestroyEvent
    {
        public CardCondition[] OrCardCondition { get; }

        public EffectTimingDestroyEvent(CardCondition[] OrCardCondition)
        {
            this.OrCardCondition = OrCardCondition;
        }
    }
}
