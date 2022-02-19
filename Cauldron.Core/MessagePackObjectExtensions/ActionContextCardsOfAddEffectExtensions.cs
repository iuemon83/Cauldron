using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfAddEffectExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfAddEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.AddEffect?.GetCards(_this.Type) ?? Array.Empty<Card>()
                : Array.Empty<Card>();
        }
    }
}
