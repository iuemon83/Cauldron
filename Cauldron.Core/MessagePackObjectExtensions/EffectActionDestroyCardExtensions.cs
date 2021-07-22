using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionDestroyCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDestroyCard effectActionDestroyCard, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, effectActionDestroyCard.Choice, args);

            var done = false;
            foreach (var card in choiceResult.CardList)
            {
                await args.GameMaster.DestroyCard(card);

                done = true;
            }

            if (!string.IsNullOrEmpty(effectActionDestroyCard.Name))
            {
                var context = new ActionContext(ActionDestroyCardContext: new(choiceResult.CardList));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionDestroyCard.Name, context);
            }

            return (done, args);
        }
    }
}
