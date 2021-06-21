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
                NumValueCalculator.ValueType.CardBaseCost => await CalculateCardBaseCost(numValueCalculator, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardPower => await CalculateCardPower(numValueCalculator, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardBasePower => await CalculateCardBasePower(numValueCalculator, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardToughness => await CalculateCardToughness(numValueCalculator, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardBaseToughness => await CalculateCardBaseToughness(numValueCalculator, effectOwnerCard, effectEventArgs),
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

            static async ValueTask<int> CalculateCardBaseCost(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var pickCards = await effectEventArgs.GameMaster
                    .ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return pickCards.CardList.Sum(c => c.BaseCost);
            }

            static async ValueTask<int> CalculateCardPower(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var pickCards = await effectEventArgs.GameMaster
                    .ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return pickCards.CardList.Sum(c => c.Power);
            }

            static async ValueTask<int> CalculateCardBasePower(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var pickCards = await effectEventArgs.GameMaster
                    .ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return pickCards.CardList.Sum(c => c.BasePower);
            }

            static async ValueTask<int> CalculateCardToughness(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var pickCards = await effectEventArgs.GameMaster
                    .ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return pickCards.CardList.Sum(c => c.Toughness);
            }

            static async ValueTask<int> CalculateCardBaseToughness(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var pickCards = await effectEventArgs.GameMaster
                    .ChoiceCards(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return pickCards.CardList.Sum(c => c.BaseToughness);
            }
        }
    }
}
