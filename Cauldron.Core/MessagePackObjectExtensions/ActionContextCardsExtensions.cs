using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsExtensions
    {
        public static IEnumerable<Card> GetCards(this ActionContextCards actionContextCards, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (actionContextCards?.ActionContextCardsOfAddEffect != null)
            {
                return actionContextCards.ActionContextCardsOfAddEffect.GetRsult(effectOwnerCard, eventArgs);
            }
            if (actionContextCards?.ActionContextCardsOfDestroyCard != null)
            {
                return actionContextCards.ActionContextCardsOfDestroyCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (actionContextCards?.ActionContextCardsOfMoveCard != null)
            {
                return actionContextCards.ActionContextCardsOfMoveCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else
            {
                return Array.Empty<Card>();
            }
        }
    }
}
