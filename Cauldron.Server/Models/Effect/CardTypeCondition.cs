using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public record CardTypeCondition(IReadOnlyList<CardType> Value, bool Not = false)
    {
        public bool IsMatch(CardType checkValue)
        {
            var result = this.Value.Contains(checkValue);
            return this.Not ? !result : result;
        }
    }
}
