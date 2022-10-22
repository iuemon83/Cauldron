using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCardsOfChoiceExtensions
    {
        public static IEnumerable<Card> GetRsult(this ActionContextCardsOfChoice _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (!args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value))
            {
                return Array.Empty<Card>();
            }

            var cardIdList = value?.Choice?.GetCards(_this.Type) ?? Array.Empty<CardId>();

            return cardIdList
                .Select(cid => args.GameMaster.TryGet(cid))
                .Where(x => x.Exists)
                .Select(x => x.Card);
        }
    }
}
