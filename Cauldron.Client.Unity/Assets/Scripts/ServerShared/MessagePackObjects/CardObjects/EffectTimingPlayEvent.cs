using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingPlayEvent
    {
        public enum SourceValue
        {
            [DisplayText("このカード")]
            This,
            [DisplayText("他のカード")]
            Other
        }

        public SourceValue Source { get; }

        public CardCondition CardCondition { get; }

        public EffectTimingPlayEvent(SourceValue Source, CardCondition CardCondition = null)
        {
            this.Source = Source;
            this.CardCondition = CardCondition;
        }
    }
}
