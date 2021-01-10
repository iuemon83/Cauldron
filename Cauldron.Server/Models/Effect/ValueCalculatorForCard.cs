using System;

namespace Cauldron.Server.Models.Effect
{
    public record ValueCalculatorForCard(ValueCalculatorForCard.ValueType Type, Choice CardsChoice)
    {
        public enum ValueType
        {
            CardCount
        }

        public int Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return this.Type switch
            {
                ValueType.CardCount => this.CalculateCount(effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(this.Type)}: {this.Type}")
            };
        }

        private int CalculateCount(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, this.CardsChoice, effectEventArgs).CardList.Count;
        }
    }
}
