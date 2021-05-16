using Cauldron.Core.Entities.Effect;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class TextValueCalculatorExtensions
    {
        public static async ValueTask<string> Calculate(this TextValueCalculator textValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return textValueCalculator.Type switch
            {
                TextValueCalculator.ValueType.CardName => await CalculateName(textValueCalculator, effectOwnerCard, effectEventArgs),
                _ => throw new InvalidOperationException($"{nameof(textValueCalculator.Type)}: {textValueCalculator.Type}")
            };

            static async ValueTask<string> CalculateName(TextValueCalculator textValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                var choiceResult = await effectEventArgs.GameMaster
                    .ChoiceCards(effectOwnerCard, textValueCalculator.CardsChoice, effectEventArgs);

                var pickCards = choiceResult.CardList;

                return pickCards.Any() ? pickCards[0].FullName : "";
            }
        }
    }
}
