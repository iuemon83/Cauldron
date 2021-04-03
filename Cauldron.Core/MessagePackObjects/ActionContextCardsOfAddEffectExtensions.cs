using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfAddEffectExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfAddEffect actionContextCardsOfAddEffect, Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, actionContextCardsOfAddEffect.ActionName, out var value)
                ? value?.ActionAddEffectContext?.GetCards(actionContextCardsOfAddEffect.Type)
                : Array.Empty<Card>();
        }
    }
}
