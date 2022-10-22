using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionAddCardContext(IReadOnlyList<Card> AddCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfAddCard.TypeValue type)
            => type switch
            {
                ActionContextCardsOfAddCard.TypeValue.AddCards => this.AddCards,
                _ => Array.Empty<Card>()
            };
    }
}
