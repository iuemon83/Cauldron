using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfDestroyCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfDestroyCard actionContextCardsOfDestroyCard, Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, actionContextCardsOfDestroyCard.ActionName, out var value)
                ? value?.ActionDestroyCardContext?.GetCards(actionContextCardsOfDestroyCard.Type)
                : System.Array.Empty<Card>();
        }
    }
}
