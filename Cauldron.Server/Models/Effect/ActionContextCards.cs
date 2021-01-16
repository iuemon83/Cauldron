﻿using System;
using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    public record ActionContextCards(
        ActionContextCardsOfAddEffect ActionContextCardsOfAddEffect = null,
        ActionContextCardsOfDestroyCard ActionContextCardsOfDestroyCard = null,
        ActionContextCardsOfMoveCard ActionContextCardsOfMoveCard = null
        )
    {
        public IEnumerable<Card> GetCards(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (this?.ActionContextCardsOfAddEffect != null)
            {
                return this.ActionContextCardsOfAddEffect.GetRsult(effectOwnerCard, eventArgs);
            }
            if (this?.ActionContextCardsOfDestroyCard != null)
            {
                return this.ActionContextCardsOfDestroyCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (this?.ActionContextCardsOfMoveCard != null)
            {
                return this.ActionContextCardsOfMoveCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else
            {
                return Array.Empty<Card>();
            }
        }
    }
}