using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class DamageNotifyMessage
    {
        public enum ReasonCode
        {
            Attack,
            Effect,
        }

        public DamageNotifyMessage.ReasonCode Reason { get; set; }

        public CardId SourceCardId { get; set; }
        public PlayerId GuardPlayerId { get; set; }
        public CardId GuardCardId { get; set; }
        public int Damage { get; set; }
    }
}
