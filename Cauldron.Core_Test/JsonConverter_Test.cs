using Cauldron.Core;
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
            static void Assertion(string testJsonName)
            {
                var testjson = File.ReadAllText($"CardSet/{testJsonName}");

                var obj = JsonConverter.Deserialize<CardSet>(testjson);
                Assert.NotNull(obj);

                var actual = JsonConverter.Serialize(obj);
                Assert.Equal(testjson, actual);
            }

            Assertion("read_test1.json");
            Assertion("read_test2.json");
        }

        [Fact]
        public void SerializeJson()
        {
            static void Assertion<T>(string name, string testJsonName)
            {
                var sampleCardType = typeof(T);
                var carddefs = sampleCardType
                    .GetProperties(BindingFlags.Static | BindingFlags.Public)
                    .Where(x => x.PropertyType == typeof(CardDef))
                    .Select(x => x.GetValue(sampleCardType, null) as CardDef)
                    .ToArray();

                var test = new CardSet(name, carddefs);

                var actual = JsonConverter.Serialize(test);
                File.WriteAllText($"{sampleCardType.Name}.json", actual);
                var expected = File.ReadAllText($"CardSet/{testJsonName}");

                Assert.Equal(expected, actual);
            }

            Assertion<SampleCards1>(SampleCards1.CardsetName, "read_test1.json");
            Assertion<SampleCards2>(SampleCards2.CardsetName, "read_test2.json");
        }
    }
}
