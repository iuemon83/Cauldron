using System;
using System.Linq;

namespace Cauldron.Server.Models.Effect.Value
{
    public record NumValueCalculator(NumValueCalculator.ValueType Type, Choice CardsChoice)
    {
        public enum ValueType
        {
            Count,
            CardCost,
        }

        public int Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return this.Type switch
            {
                ValueType.Count => this.CalculateCount(effectOwnerCard, effectEventArgs),
                ValueType.CardCost => this.CalculateCardCost(effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(this.Type)}: {this.Type}")
            };
        }

        private int CalculateCount(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, this.CardsChoice, effectEventArgs).CardList.Count;
        }

        private int CalculateCardCost(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster
                .ChoiceCards(effectOwnerCard, this.CardsChoice, effectEventArgs)
                .CardList
                .Sum(c => c.Cost);
        }
    }
}
