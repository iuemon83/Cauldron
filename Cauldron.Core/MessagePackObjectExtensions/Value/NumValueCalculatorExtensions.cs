using Cauldron.Core.Entities.Effect;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueCalculatorExtensions
    {
        public static async ValueTask<int> Calculate(this NumValueCalculator _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return _this.Type switch
            {
                NumValueCalculator.ValueType.Count => await CalculateCount(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardCost => await CalculateCardCost(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardBaseCost => await CalculateCardBaseCost(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardPower => await CalculateCardPower(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardBasePower => await CalculateCardBasePower(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardToughness => await CalculateCardToughness(_this, effectOwnerCard, effectEventArgs),
                NumValueCalculator.ValueType.CardBaseToughness => await CalculateCardBaseToughness(_this, effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(_this.Type)}: {_this.Type}")
            };

            static async ValueTask<int> CalculateCount(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);
                return picked.PlayerIdList.Length + picked.CardList.Length + picked.CardDefList.Length;
            }

            static async ValueTask<int> CalculateCardCost(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return picked.CardList.Sum(c => c.Cost)
                    + picked.CardDefList.Sum(c => c.Cost);
            }

            static async ValueTask<int> CalculateCardBaseCost(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return picked.CardList.Sum(c => c.BaseCost)
                    + picked.CardDefList.Sum(c => c.Cost);
            }

            static async ValueTask<int> CalculateCardPower(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return picked.CardList.Sum(c => c.Power)
                    + picked.CardDefList.Sum(c => c.Power);
            }

            static async ValueTask<int> CalculateCardBasePower(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return picked.CardList.Sum(c => c.BasePower)
                    + picked.CardDefList.Sum(c => c.Power);
            }

            static async ValueTask<int> CalculateCardToughness(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return picked.CardList.Sum(c => c.Toughness)
                    + picked.CardDefList.Sum(c => c.Toughness);
            }

            static async ValueTask<int> CalculateCardBaseToughness(NumValueCalculator numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                return picked.CardList.Sum(c => c.BaseToughness)
                    + picked.CardDefList.Sum(c => c.Toughness);
            }
        }
    }
}
