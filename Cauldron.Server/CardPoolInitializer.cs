using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.IO;
using System.Linq;

namespace Cauldron.Server
{
    public class CardPoolInitializer
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

        public static void Init(string cardsetDirectoryPath)
        {
            var cardpool = ReadFromDirectory(cardsetDirectoryPath);
            if (!cardpool.IsValid())
            {
                throw new InvalidOperationException("カードプールが不正です。");
            }

            CardPoolSingleton = cardpool;
        }

        public static CardPool CardPoolSingleton { get; private set; }
    }
}
