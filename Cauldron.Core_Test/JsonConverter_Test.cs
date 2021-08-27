using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Cauldron.Core_Test
{
    public class JsonConverter_Test
    {
        [Fact]
        public void DeserializeJson()
        {
            var testjson = File.ReadAllText("CardSet/read_test.json");

            var obj = JsonConverter.Deserialize<CardSet>(testjson);
            Assert.NotNull(obj);

            var actual = JsonConverter.Serialize(obj);
            Assert.Equal(testjson, actual);
        }

        [Fact]
        public void SerializeJson()
        {
            var sampleCardType = typeof(SampleCards);
            var carddefs = sampleCardType
                .GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(x => x.PropertyType == typeof(CardDef))
                .Select(x => x.GetValue(sampleCardType, null) as CardDef)
                .ToArray();

            var test = new CardSet("Sample", carddefs);

            var actual = JsonConverter.Serialize(test);
            File.WriteAllText("write_test.json", actual);
            var expected = File.ReadAllText("CardSet/read_test.json");

            Assert.Equal(expected, actual);
        }
    }
}
