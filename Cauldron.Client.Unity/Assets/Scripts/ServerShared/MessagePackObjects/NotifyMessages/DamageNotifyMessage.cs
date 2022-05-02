#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class DamageNotifyMessage
    {
        public enum ReasonValue
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

        public DamageNotifyMessage.ReasonValue Reason { get; }

        public CardId SourceCardId { get; }
        public PlayerId GuardPlayerId { get; }
        public CardId GuardCardId { get; }
        public int Damage { get; }
        public Card? EffectOwnerCard { get; }
        public CardEffectId? EffectId { get; }

        public DamageNotifyMessage(
            ReasonValue Reason,
            int Damage,
            CardId SourceCardId = default,
            PlayerId GuardPlayerId = default,
            CardId GuardCardId = default,
            Card? EffectOwnerCard = default,
            CardEffectId? EffectId = default
            )
        {
            this.Reason = Reason;
            this.SourceCardId = SourceCardId;
            this.GuardPlayerId = GuardPlayerId;
            this.GuardCardId = GuardCardId;
            this.Damage = Damage;
            this.EffectOwnerCard = EffectOwnerCard;
            this.EffectId = EffectId;
        }
    }
}
