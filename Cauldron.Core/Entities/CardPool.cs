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

        public static void WriteToFile(string jsonFilePath)
        {
            var cardSet = new CardSet(
                "Sample",
                new[] {
                    TestCards.angelSnipe,
                    TestCards.bellAngel,
                    TestCards.mino,
                    TestCards.kenma,
                    TestCards.hikari,
                    TestCards.unmei,
                    TestCards.shieldAngel,
                    TestCards.healingAngel,
                    TestCards.lizardman,
                    TestCards.angelBarrage,
                    TestCards.gakkyoku,
                    TestCards.ulz,
                    TestCards.demonStraike,
                    TestCards.meifu,
                    TestCards.goblinDemon,
                    TestCards.fujin,
                    TestCards.excution,
                    TestCards.atena,
                    TestCards.tenyoku,
                    TestCards.gabriel,
                    TestCards.gilgamesh,
                    TestCards.lucifer,
                    TestCards.satan,
                }
            );

            var jsonText = JsonConverter.Serialize(cardSet);

            File.WriteAllText(jsonFilePath, jsonText);
        }
    }
}
