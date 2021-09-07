using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionDestroyCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDestroyCard _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.Choice, args);

            var deletedCardList = new List<Card>();
            foreach (var card in choiceResult.CardList)
            {
                var deleted = await args.GameMaster.DestroyCard(card);
                if (deleted)
                {
                    deletedCardList.Add(card);
                }
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(DestroyCard: new(deletedCardList));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (deletedCardList.Any(), args);
        }
    }
}
