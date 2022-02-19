using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cauldron.Core.Entities
{
    public class CardPool
    {
        public static CardPool ReadFromDirectory(string cardsetDirectoryPath)
        {
            var cardSets = Directory.GetFiles(cardsetDirectoryPath, "*.json")
                .Select(ReadFromFile);

            return new CardPool(cardSets);
        }

        private static CardSet ReadFromFile(string jsonFilePath)
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            return JsonConverter.Deserialize<CardSet>(jsonString) ?? new CardSet("", Array.Empty<CardDef>());
        }

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
