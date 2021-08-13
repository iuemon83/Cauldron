using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionModifyCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionModifyCard effectActionModifyCard, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster.Choice(effectOwnerCard, effectActionModifyCard.Choice, effectEventArgs);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                await effectEventArgs.GameMaster.ModifyCard(card, effectActionModifyCard, effectOwnerCard, effectEventArgs);

                done = true;
            }

            return (done, effectEventArgs);
        }
    }
}
