using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionMoveCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionMoveCard effectActionMoveCard, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionMoveCard.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                var toZone = effectActionMoveCard.To.FromPrettyName(effectOwnerCard.OwnerId, args.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id);
                await args.GameMaster.MoveCard(card.Id, new(card.Zone, toZone));

                done = true;
            }

            if (!string.IsNullOrEmpty(effectActionMoveCard.Name))
            {
                var context = new ActionContext(ActionMoveCardContext: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionMoveCard.Name, context);
            }

            return (done, args);
        }
    }
}
