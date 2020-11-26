using System;

namespace Cauldron.Core
{
    public class Logger
    {
        public string LogText(Card card) => card.Type switch
        {
            CardType.Artifact => $"{card.Name}[{card.Cost}]",
            CardType.Sorcery => $"{card.Name}[{card.Cost}]",
            _ => $"{card.Name}[{card.Cost},{card.Power},{card.Toughness}]",
        };

        public void Information(string message)
        {
            Console.WriteLine(message);
        }
    }
}
