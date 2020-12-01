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
                Cards = new CardDefJson[0]
            };
            //var cardSet = this.LoadFromFile(jsonFilePath);

            var manual = new[] {
                //fairy,
                //goblin,
                TestCards.slime,
                //mouse,
                //waterFairy,
                //ninja, knight, ninjaKnight, whiteGeneral,
                //angel, devil, fortuneSpring,
                //flag,
                //shock,
                //buf,
                TestCards.shield,
                TestCards.wall,
                TestCards.holyKnight,
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
