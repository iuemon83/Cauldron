using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cauldron.Core.Entities
{
    public class CardPool
    {
        public static IEnumerable<CardSet> ReadFromDirectory(string cardsetDirectoryPath)
        {
            return Directory.GetFiles(cardsetDirectoryPath, "*.json")
                .Select(ReadFromFile);
        }
        public static CardSet ReadFromFile(string jsonFilePath)
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            return JsonConverter.Deserialize<CardSet>(jsonString);
        }
    }
}
