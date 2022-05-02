#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class MoveCardNotifyMessage
    {
        public Card Card { get; }
        public Zone FromZone { get; }
        public Zone ToZone { get; }
        public int Index { get; }
        public Card? EffectOwnerCard { get; }
        public CardEffectId? EffectId { get; }

        public MoveCardNotifyMessage(
            Card Card,
            Zone FromZone,
            Zone ToZone,
            int Index,
            Card? EffectOwnerCard = default,
            CardEffectId? EffectId = default
            )
        {
            this.Card = Card;
            this.FromZone = FromZone;
            this.ToZone = ToZone;
            this.Index = Index;
            this.EffectOwnerCard = EffectOwnerCard;
            this.EffectId = EffectId;
        }
    }
}
