using MessagePack;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardTypeCondition
    {
        public IReadOnlyList<CardType> Value { get; }
        public bool Not { get; }

        public CardTypeCondition(IReadOnlyList<CardType> Value, bool Not = false)
        {
            this.Value = Value;
            this.Not = Not;
        }
    }
}
