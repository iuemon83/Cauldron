using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsExtensions
    {
        public static IEnumerable<Card> GetCards(this ActionContextCards _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (_this?.ActionContextCardsOfAddEffect != null)
            {
                return _this.ActionContextCardsOfAddEffect.GetRsult(effectOwnerCard, eventArgs);
            }
            if (_this?.ActionContextCardsOfDestroyCard != null)
            {
                return _this.ActionContextCardsOfDestroyCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.ActionContextCardsOfMoveCard != null)
            {
                return _this.ActionContextCardsOfMoveCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.ActionContextCardsOfExcludeCard != null)
            {
                return _this.ActionContextCardsOfExcludeCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else if (_this?.ActionContextCardsOfModifyCard != null)
            {
                return _this.ActionContextCardsOfModifyCard.GetRsult(effectOwnerCard, eventArgs);
            }
            else
            {
                return Array.Empty<Card>();
            }
        }
    }
}
