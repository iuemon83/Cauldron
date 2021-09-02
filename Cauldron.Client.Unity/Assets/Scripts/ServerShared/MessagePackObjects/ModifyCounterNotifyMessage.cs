using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyCounterNotifyMessage
    {
        public string CounterName { get; }

        public int NumCounters { get; }

        public PlayerId TargetPlayerId { get; }

        public CardId TargetCardId { get; }

        public ModifyCounterNotifyMessage(
            string CounterName,
            int NumCounters,
            PlayerId TargetPlayerId = default,
            CardId TargetCardId = default)
        {
            this.CounterName = CounterName;
            this.NumCounters = NumCounters;
            this.TargetPlayerId = TargetPlayerId;
            this.TargetCardId = TargetCardId;
        }
    }
}
