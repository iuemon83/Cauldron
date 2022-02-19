using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfAddCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfAddCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.AddCard?.GetCards(_this.Type) ?? Array.Empty<Card>()
                : Array.Empty<Card>();
        }
    }
}
