using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionExcludeCardExecuter : IEffectActionExecuter
    {
        private readonly EffectActionExcludeCard _this;

        public EffectActionExcludeCardExecuter(EffectActionExcludeCard _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs effectEventArgs)
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
