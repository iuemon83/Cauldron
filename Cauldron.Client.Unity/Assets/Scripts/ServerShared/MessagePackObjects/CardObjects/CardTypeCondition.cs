using MessagePack;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardTypeCondition
    {
        public IReadOnlyList<CardType> Value { get; set; }
        public bool Not { get; set; }

        public CardTypeCondition(IReadOnlyList<CardType> value, bool not = false)
        {
            this.Value = value;
            this.Not = not;
        }
    }
}
