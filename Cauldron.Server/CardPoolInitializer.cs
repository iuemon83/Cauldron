using Cauldron.Core;
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

            var cardset = JsonConverter.Deserialize<CardSet>(jsonString) ?? new CardSet("", Array.Empty<CardDef>());

            // IDがemptyになるので振りなおす
            foreach (var c in cardset.Cards)
            {
                c.DangerousSetNewId();
            }

            return cardset;
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
