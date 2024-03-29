﻿#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ExcludeCardNotifyMessage
    {
        public Card Card { get; }
        public Zone FromZone { get; }
        public Card? EffectOwnerCard { get; }
        public CardEffectId? EffectId { get; }

        public ExcludeCardNotifyMessage(
            Card Card,
            Zone FromZone,
            Card? EffectOwnerCard = default,
            CardEffectId? EffectId = default
            )
        {
            this.Card = Card;
            this.FromZone = FromZone;
            this.EffectOwnerCard = EffectOwnerCard;
            this.EffectId = EffectId;
        }
    }
}
