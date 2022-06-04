using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueCalculatorForCardExtensions
    {
        public static async ValueTask<int> Calculate(this NumValueCalculatorForCard _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            if (_this.Type != default
                && _this.CardsChoice != default)
            {
                return _this.Type switch
                {
                    NumValueCalculatorForCard.TypeValue.Count => await CalculateCount(_this, effectOwnerCard, effectEventArgs),
                    NumValueCalculatorForCard.TypeValue.DefCount => await DefCalculateCount(_this, effectOwnerCard, effectEventArgs),
                    NumValueCalculatorForCard.TypeValue.CardCost => await CalculateCardCost(_this, effectOwnerCard, effectEventArgs),
                    NumValueCalculatorForCard.TypeValue.CardBaseCost => await CalculateCardBaseCost(_this, effectOwnerCard, effectEventArgs),
                    NumValueCalculatorForCard.TypeValue.CardPower => await CalculateCardPower(_this, effectOwnerCard, effectEventArgs),
                    NumValueCalculatorForCard.TypeValue.CardBasePower => await CalculateCardBasePower(_this, effectOwnerCard, effectEventArgs),
                    NumValueCalculatorForCard.TypeValue.CardToughness => await CalculateCardToughness(_this, effectOwnerCard, effectEventArgs),
                    NumValueCalculatorForCard.TypeValue.CardBaseToughness => await CalculateCardBaseToughness(_this, effectOwnerCard, effectEventArgs),
                    _ => throw new InvalidOperationException($"{nameof(_this.Type)}: {_this.Type}")
                };

                static async ValueTask<int> CalculateCount(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);
                    return picked.CardList.Length + picked.CardDefList.Length;
                }

                static async ValueTask<int> DefCalculateCount(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);
                    return picked.CardList.DistinctBy(c => c.CardDefId).Count() + picked.CardDefList.Length;
                }

                static async ValueTask<int> CalculateCardCost(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                    return picked.CardList.Sum(c => c.Cost)
                        + picked.CardDefList.Sum(c => c.Cost);
                }

                static async ValueTask<int> CalculateCardBaseCost(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                    return picked.CardList.Sum(c => c.BaseCost)
                        + picked.CardDefList.Sum(c => c.Cost);
                }

                static async ValueTask<int> CalculateCardPower(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                    return picked.CardList.Sum(c => c.Power)
                        + picked.CardDefList.Sum(c => c.Power);
                }

                static async ValueTask<int> CalculateCardBasePower(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                    return picked.CardList.Sum(c => c.BasePower)
                        + picked.CardDefList.Sum(c => c.Power);
                }

                static async ValueTask<int> CalculateCardToughness(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                    return picked.CardList.Sum(c => c.Toughness)
                        + picked.CardDefList.Sum(c => c.Toughness);
                }

                static async ValueTask<int> CalculateCardBaseToughness(NumValueCalculatorForCard numValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
                {
                    var picked = await effectEventArgs.GameMaster
                        .Choice(effectOwnerCard, numValueCalculator.CardsChoice, effectEventArgs);

                    return picked.CardList.Sum(c => c.BaseToughness)
                        + picked.CardDefList.Sum(c => c.Toughness);
                }
            }

            return 0;
        }
    }
}
