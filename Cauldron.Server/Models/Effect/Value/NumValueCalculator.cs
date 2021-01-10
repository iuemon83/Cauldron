using System;

namespace Cauldron.Server.Models.Effect.Value
{
    public record NumValueCalculator(NumValueCalculator.ValueType Type, Choice CardsChoice)
    {
        public enum ValueType
        {
            Count,
        }

        public int Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return this.Type switch
            {
                ValueType.Count => this.CalculateCount(effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(this.Type)}: {this.Type}")
            };
        }

        private int CalculateCount(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, this.CardsChoice, effectEventArgs).CardList.Count;
        }
    }
}
