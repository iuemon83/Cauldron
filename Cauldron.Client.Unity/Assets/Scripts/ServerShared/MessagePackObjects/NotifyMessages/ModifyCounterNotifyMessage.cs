#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyCounterNotifyMessage
    {
        public string CounterName { get; }

        public int NumCounters { get; }

        public PlayerId TargetPlayerId { get; }

        public Card? TargetCard { get; }
        public Card? EffectOwnerCard { get; }

        public ModifyCounterNotifyMessage(
            string CounterName,
            int NumCounters,
            PlayerId TargetPlayerId = default,
            Card? TargetCard = default,
            Card? EffectOwnerCard = default
            )
        {
            this.CounterName = CounterName;
            this.NumCounters = NumCounters;
            this.TargetPlayerId = TargetPlayerId;
            this.TargetCard = TargetCard;
            this.EffectOwnerCard = EffectOwnerCard;
        }
    }
}
