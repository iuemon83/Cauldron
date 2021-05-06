using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
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
            var json = LocalData.DeckListJson;

            try
            {
                return JsonUtility.FromJson<DeckList>(json).Decks.ToList();
            }
            catch
            {
                // デシリアライズに失敗
                return new List<Deck>();
            }
        }

        private static void Save()
        {
            var json = JsonUtility.ToJson(new DeckList() { Decks = inMemoryCache.Value.ToArray() });
            LocalData.DeckListJson = json;
        }

        public void Add(string name, IEnumerable<CardDef> cardDefList)
        {
            inMemoryCache.Value.Add(new Deck()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                CardDefNames = cardDefList.Select(c => c.FullName).ToArray()
            });

            Save();
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

            Save();
        }

        public IDeck[] GetAll()
        {
            return inMemoryCache.Value.ToArray();
        }
    }
}
