using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfDamageExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfDamage _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.Damage?.GetCards(_this.Type)
                : System.Array.Empty<Card>();
        }
    }
}
