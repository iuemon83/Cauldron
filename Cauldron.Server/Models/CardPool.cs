using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cauldron.Server.Models
{
    public class CardPool
    {
        public CardDef[] Load()
        {
            var jsonFilePath = "cardset.json";
            var cardSet = new CardSet()
            {
                Name = "sample",
                Cards = System.Array.Empty<CardDefJson>()
            };
            //var cardSet = this.LoadFromFile(jsonFilePath);

            var manual = new[] {
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
                //TestCards.fairy,
                //TestCards.goblin,
                //TestCards.slime,
                //TestCards.mouse,
                //TestCards.waterFairy,
                //TestCards.ninja,
                //TestCards.knight,
                //TestCards.ninjaKnight,
                //TestCards.whiteGeneral,
                //TestCards.angel,
                //TestCards.devil,
                //TestCards.fortuneSpring,
                //TestCards.flag,
                //TestCards.shock,
                //TestCards.buf,
                //TestCards.shield,
                //TestCards.wall,
                //TestCards.holyKnight,
                //shippu
            };

            //return cardSet.AsCardDefs().ToArray();

            return cardSet.AsCardDefs()
                .Concat(manual)
                .ToArray();
        }

        private CardSet LoadFromFile(string jsonFilePath)
        {
            var jsonString = File.ReadAllText(jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            var cardSet = JsonSerializer.Deserialize<CardSet>(jsonString, options);
            return cardSet;
        }
    }
}
