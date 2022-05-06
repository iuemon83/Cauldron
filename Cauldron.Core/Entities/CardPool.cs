using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    public class CardPool
    {
        public readonly CardSet[] cardSets;

        public CardPool(IEnumerable<CardSet> cardSets)
        {
            this.cardSets = cardSets.ToArray();
        }

        public bool IsValid()
        {
            return this.cardSets.GroupBy(c => c.Name).All(g => g.Count() == 1)
                && this.cardSets.All(c => c.IsValid());
        }
    }
}
