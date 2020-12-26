using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    public class ChoiceResult
    {
        public IReadOnlyList<Player> PlayerList { get; set; } = System.Array.Empty<Player>();
        public IReadOnlyList<Card> CardList { get; set; } = System.Array.Empty<Card>();
        public IReadOnlyList<CardDef> CardDefList { get; set; } = new CardDef[0];
    }
}
