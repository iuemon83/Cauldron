using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class DamageNotifyMessage
    {
        public enum ReasonCode
        {
            /// <summary>
            /// クリーチャーによる攻撃
            /// </summary>
            Attack,
            /// <summary>
            /// カードの効果によるダメージ
            /// </summary>
            Effect,
            /// <summary>
            /// デッキがない状態でのドロー
            /// </summary>
            DrawDeath,
        }

        public DamageNotifyMessage.ReasonCode Reason { get; set; }

        public CardId SourceCardId { get; set; }
        public PlayerId GuardPlayerId { get; set; }
        public CardId GuardCardId { get; set; }
        public int Damage { get; set; }

        public DamageNotifyMessage(ReasonCode Reason, int Damage, CardId SourceCardId = default, PlayerId GuardPlayerId = default, CardId GuardCardId = default)
        {
            this.Reason = Reason;
            this.SourceCardId = SourceCardId;
            this.GuardPlayerId = GuardPlayerId;
            this.GuardCardId = GuardCardId;
            this.Damage = Damage;
        }
    }
}
