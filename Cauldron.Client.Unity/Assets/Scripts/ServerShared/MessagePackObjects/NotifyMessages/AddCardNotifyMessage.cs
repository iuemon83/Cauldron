#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AddCardNotifyMessage
    {
        public CardId CardId { get; }
        public Zone ToZone { get; }
        public int Index { get; }
        public Card? EffectOwnerCard { get; }
        public CardEffectId? EffectId { get; }

        public AddCardNotifyMessage(
            CardId CardId, Zone ToZone, int Index,
            Card? EffectOwnerCard = default,
            CardEffectId? EffectId = default
            )
        {
            this.CardId = CardId;
            this.ToZone = ToZone;
            this.Index = Index;
            this.EffectOwnerCard = EffectOwnerCard;
            this.EffectId = EffectId;
        }
    }
}
