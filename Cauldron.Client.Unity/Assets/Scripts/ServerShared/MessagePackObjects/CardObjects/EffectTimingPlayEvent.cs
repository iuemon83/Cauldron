using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingPlayEvent
    {
        public CardCondition[] OrCardConditions { get; }

        public EffectTimingPlayEvent(CardCondition[] OrCardConditions)
        {
            this.OrCardConditions = OrCardConditions;
        }
    }
}
