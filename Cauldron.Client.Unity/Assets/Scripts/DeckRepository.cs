using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    class DeckRepository
    {
        private static readonly Lazy<List<Deck>> inMemoryCache
            = new Lazy<List<Deck>>(InitInMemoryCache);

        private static List<Deck> InitInMemoryCache()
        {
            if (!File.Exists(Config.DeckListFielPath))
            {
                return new List<Deck>();
            }

            var json = File.ReadAllText(Config.DeckListFielPath);
            return JsonUtility.FromJson<DeckList>(json).Decks.ToList();
        }

        public void Add(string name, IEnumerable<CardDef> cardDefList)
        {
            inMemoryCache.Value.Add(new Deck()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CardDefNames = cardDefList.Select(c => c.FullName).ToArray()
            });

            var json = JsonUtility.ToJson(new DeckList() { Decks = inMemoryCache.Value.ToArray() });

            File.WriteAllText(Config.DeckListFielPath, json);
        }

        public void Update(string id, string name, IEnumerable<CardDef> cardDefList)
        {
            var deck = inMemoryCache.Value.FirstOrDefault(d => d.Id == id);
            if (deck == default)
            {
                // 指定したid のデッキが存在しない
                return;
            }

            deck.Name = name;
            deck.CardDefNames = cardDefList.Select(c => c.FullName).ToArray();

            var json = JsonUtility.ToJson(new DeckList() { Decks = inMemoryCache.Value.ToArray() });

            File.WriteAllText(Config.DeckListFielPath, json);
        }

        public IDeck[] GetAll()
        {
            return inMemoryCache.Value.ToArray();
        }
    }
}
