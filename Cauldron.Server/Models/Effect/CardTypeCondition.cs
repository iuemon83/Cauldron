using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public class CardTypeCondition
    {
        public IReadOnlyList<CardType> Value { get; set; }
        public bool Not { get; set; }

        public bool IsMatch(CardType checkValue)
        {
            var result = this.Value.Contains(checkValue);
            return this.Not ? !result : result;
        }
    }
}
