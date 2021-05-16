using Cauldron.Core.Entities.Effect;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueCalculatorExtensions
    {
        public static async ValueTask<int> Calculate(this NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return numValueCalculator.Type switch
            {
                NumValueCalculator.ValueType.Count => await CalculateCount(numValueCalculator, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardCost => await CalculateCardCost(numValueCalculator, effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(numValueCalculator.Type)}: {numValueCalculator.Type}")
            };

            static async ValueTask<int> CalculateCount(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var pickCards = await effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);
                return pickCards.CardList.Length;
            }

            static async ValueTask<int> CalculateCardCost(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var pickCards = await effectEventArgs.GameMaster
                    .ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return pickCards.CardList.Sum(c => c.Cost);
            }
        }
    }
}
