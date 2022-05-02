#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyCardNotifyMessage
    {
        public Card Card { get; }
        public Card? EffectOwnerCard { get; }
        public CardEffectId? EffectId { get; }

        public ModifyCardNotifyMessage(
            Card Card,
            Card? EffectOwnerCard = default,
            CardEffectId? EffectId = default
            )
        {
            this.Card = Card;
            this.EffectOwnerCard = EffectOwnerCard;
            this.EffectId = EffectId;
        }
    }
}
