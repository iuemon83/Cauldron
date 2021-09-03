using MessagePack;
using System.Linq;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardSet
    {
        public string Name { get; }
        public CardDef[] Cards { get; }

        public CardSet(string name, CardDef[] cards)
        {
            this.Name = name;
            this.Cards = cards;
        }

        public bool IsValid()
        {
            return this.Cards.ToLookup(c => c.Name)
                .All(g => g.Count() == 1);
        }
    }
}
