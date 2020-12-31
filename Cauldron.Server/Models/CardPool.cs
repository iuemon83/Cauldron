using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class CardPool
    {
        public static IEnumerable<CardDef> LoadFromDirectory(string cardsetDirectoryPath)
        {
            return Directory.GetFiles(cardsetDirectoryPath)
                .SelectMany(LoadFromFile);
        }
        public static IEnumerable<CardDef> LoadFromFile(string jsonFilePath)
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            var cardset = JsonConverter.Deserialize<CardSet>(jsonString);
            return cardset.Cards
                .Select(c =>
                {
                    c.FullName = $"{cardset.Name}.{c.Name}";
                    return c;
                });
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
