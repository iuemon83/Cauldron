using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfExcludeCardExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfExcludeCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.ExcludeCard?.GetCards(_this.Type) ?? Array.Empty<Card>()
                : Array.Empty<Card>();
        }
    }
}
