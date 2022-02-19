using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfMoveCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfMoveCard actionContextCardsOfMoveCard, Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, actionContextCardsOfMoveCard.ActionName, out var value)
                ? value?.MoveCard?.GetCards(actionContextCardsOfMoveCard.Type) ?? Array.Empty<Card>()
                : Array.Empty<Card>();
        }
    }
}
