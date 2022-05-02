using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class BattleNotifyMessage
    {
        public CardId AttackCardId { get; }
        public PlayerId GuardPlayerId { get; }
        public CardId GuardCardId { get; }
        public CardEffectId? EffectId { get; }

        public BattleNotifyMessage(
            CardId AttackCardId,
            PlayerId GuardPlayerId = default,
            CardId GuardCardId = default)
        {
            this.AttackCardId = AttackCardId;
            this.GuardPlayerId = GuardPlayerId;
            this.GuardCardId = GuardCardId;
            this.EffectId = EffectId;
        }
    }
}
