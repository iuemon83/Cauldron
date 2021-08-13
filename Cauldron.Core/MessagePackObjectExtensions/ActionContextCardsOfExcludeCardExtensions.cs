using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfExcludeCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfExcludeCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.ActionExcludeCardContext?.GetCards(_this.Type)
                : System.Array.Empty<Card>();
        }
    }
}
