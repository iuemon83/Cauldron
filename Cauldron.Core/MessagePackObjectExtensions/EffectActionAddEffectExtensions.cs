using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionAddEffectExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionAddEffect effectActionAddEffect, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionAddEffect.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                args.GameMaster.AddEffect(card, effectActionAddEffect.EffectToAdd);
            }

            if (!string.IsNullOrEmpty(effectActionAddEffect.Name))
            {
                var context = new ActionContext(ActionAddEffectContext: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionAddEffect.Name, context);
            }

            return (done, args);
        }
    }
}
