using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    public record ZoneCondition(IEnumerable<ZoneType> ZoneTypes, bool Not = false)
    {
        public enum ZoneType
        {
            None,
            CardPool,
            YouField,
            OpponentField,
            YouHand,
            OpponentHand,
            YouDeck,
            OpponentDeck,
            YouCemetery,
            OpponentCemetery,
        }
    }
}
