using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionModifyCardExecuter : IEffectActionExecuter
    {
        private readonly EffectActionModifyCard _this;

        public EffectActionModifyCardExecuter(EffectActionModifyCard _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster.Choice(effectOwnerCard, _this.Choice, effectEventArgs);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                var newArgs = effectEventArgs with
                {
                    ActionTargetCard = card,
                    ActionTargetCards = targets
                };
                await effectEventArgs.GameMaster.ModifyCard(card, _this, effectOwnerCard, newArgs);

                done = true;
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(ModifyCard: new(targets));
                effectEventArgs.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (done, effectEventArgs);
        }
    }
}
