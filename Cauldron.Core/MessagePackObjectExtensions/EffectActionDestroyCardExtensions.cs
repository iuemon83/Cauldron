using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionDestroyCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDestroyCard effectActionDestroyCard, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, effectActionDestroyCard.Choice, args);

            var deletedCardList = new List<Card>();
            foreach (var card in choiceResult.CardList)
            {
                var deleted = await args.GameMaster.DestroyCard(card);
                if (deleted)
                {
                    deletedCardList.Add(card);
                }
            }

            if (!string.IsNullOrEmpty(effectActionDestroyCard.Name))
            {
                var context = new ActionContext(ActionDestroyCardContext: new(deletedCardList));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, effectActionDestroyCard.Name, context);
            }

            return (deletedCardList.Any(), args);
        }
    }
}
