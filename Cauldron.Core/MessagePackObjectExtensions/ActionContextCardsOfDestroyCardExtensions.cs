using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfDestroyCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfDestroyCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.DestroyCard?.GetCards(_this.Type)
                : System.Array.Empty<Card>();
        }
    }
}
