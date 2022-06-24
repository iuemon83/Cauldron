#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class HealNotifyMessage
    {
        public CardId SourceCardId { get; }
        public PlayerId TakePlayerId { get; }
        public CardId TakeCardId { get; }
        public int HealValue { get; }
        public Card? EffectOwnerCard { get; }
        public CardEffectId? EffectId { get; }

        public HealNotifyMessage(
            int HealValue,
            CardId SourceCardId = default,
            PlayerId TakePlayerId = default,
            CardId TakeCardId = default,
            Card? EffectOwnerCard = default,
            CardEffectId? EffectId = default
            )
        {
            this.SourceCardId = SourceCardId;
            this.TakePlayerId = TakePlayerId;
            this.TakeCardId = TakeCardId;
            this.HealValue = HealValue;
            this.EffectOwnerCard = EffectOwnerCard;
            this.EffectId = EffectId;
        }
    }
}
