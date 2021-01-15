using System;
using System.Linq;

namespace Cauldron.Server.Models.Effect.Value
{
    public record TextValueCalculator(TextValueCalculator.ValueType Type, Choice CardsChoice)
    {
        public enum ValueType
        {
            CardName,
        }

        public string Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return this.Type switch
            {
                ValueType.CardName => this.CalculateName(effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(this.Type)}: {this.Type}")
            };
        }

        private string CalculateName(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var cards = effectEventArgs.GameMaster
                .ChoiceCards(effectOwnerCard, this.CardsChoice, effectEventArgs)
                .CardList;

            return cards.Any() ? cards[0].FullName : "";
        }
    }
}
