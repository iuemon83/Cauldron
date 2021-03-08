using Cauldron.Core.Entities;
using System.IO;
using Xunit;

namespace Cauldron.Core_Test
{
    public class JsonConverter_Test
    {
        [Fact]
        public void CardId_test()
        {
            var testfile = File.ReadAllText("CardSet/test.json");

            var actual = JsonConverter.Deserialize<CardSet>(testfile);
        }
    }
}
