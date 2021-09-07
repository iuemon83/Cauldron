using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionAddEffectExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionAddEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.CardsChoice, args);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                args.GameMaster.AddEffect(card, _this.EffectToAdd);
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(AddEffect: new(targets));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (done, args);
        }
    }
}
