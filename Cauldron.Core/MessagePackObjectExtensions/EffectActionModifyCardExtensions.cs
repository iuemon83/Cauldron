using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionModifyCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionModifyCard effectActionModifyCard, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, effectActionModifyCard.Choice, effectEventArgs);
            var targets = choiceResult.CardList;

            var done = false;
            foreach (var card in targets)
            {
                //var buffPower = await (effectActionModifyCard.Power?.Modify(effectOwnerCard, effectEventArgs, card.Power)
                //    ?? ValueTask.FromResult(card.Power));
                //var buffToughness = await (effectActionModifyCard.Toughness?.Modify(effectOwnerCard, effectEventArgs, card.Toughness)
                //    ?? ValueTask.FromResult(card.Toughness));

                //await effectEventArgs.GameMaster.Buff(card, buffPower - card.Power, buffToughness - card.Toughness);

                await effectEventArgs.GameMaster.ModifyCard(card, effectActionModifyCard, effectOwnerCard, effectEventArgs);

                done = true;
            }

            return (done, effectEventArgs);
        }
    }
}
