using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionChoiceContext(IReadOnlyList<CardId> ChoiceCards, IReadOnlyList<PlayerId> ChoicePlayers)
    {
        public IEnumerable<CardId> GetCards(ActionContextCardsOfChoice.TypeValue type)
            => type switch
            {
                ActionContextCardsOfChoice.TypeValue.ChoiceCards => this.ChoiceCards,
                _ => Array.Empty<CardId>()
            };
        public IEnumerable<PlayerId> GetPlayers(ActionContextPlayersOfChoice.TypeValue type)
            => type switch
            {
                ActionContextPlayersOfChoice.TypeValue.ChoicePlayers => this.ChoicePlayers,
                _ => Array.Empty<PlayerId>()
            };
    }
}
