using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionChoiceExecuter : IEffectActionExecuter
    {
        private readonly EffectActionChoice _this;

        public EffectActionChoiceExecuter(EffectActionChoice _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(
            Card effectOwnerCard, CardEffectId effectId, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.Choice, args);

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(Choice: new(
                    choiceResult.CardList.Select(c => c.Id).ToArray(),
                    choiceResult.PlayerIdList.ToArray()
                    ));

                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (true, args);
        }
    }
}
