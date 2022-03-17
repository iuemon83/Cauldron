using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionDestroyCardExecuter : IEffectActionExecuter
    {
        private readonly EffectActionDestroyCard _this;

        public EffectActionDestroyCardExecuter(EffectActionDestroyCard _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
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
