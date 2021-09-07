using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfDrawCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfDrawCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.DrawCard?.GetCards(_this.Type)
                : System.Array.Empty<Card>();
        }
    }
}
