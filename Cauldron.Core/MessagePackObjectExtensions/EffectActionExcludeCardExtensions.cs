using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionExcludeCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionExcludeCard _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster
                .Choice(effectOwnerCard, _this.Choice, effectEventArgs);

            var excludedCardList = new List<Card>();
            foreach (var cardToExclude in choiceResult.CardList)
            {
                var excluded = await effectEventArgs.GameMaster.ExcludeCard(cardToExclude);
                if (excluded)
                {
                    excludedCardList.Add(cardToExclude);
                }
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(ExcludeCard: new(excludedCardList));
                effectEventArgs.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (true, effectEventArgs);
        }
    }
}
