using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using System.IO;
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
            var test = new CardSet("Sample", new[]
            {
                SampleCards.KarakuriGoblin,
                SampleCards.Goblin,
                SampleCards.QuickGoblin,
                SampleCards.ShieldGoblin,
                SampleCards.DeadlyGoblin,
                SampleCards.MechanicGoblin,
                SampleCards.NinjaGoblin,
                SampleCards.SuperNinjaGoblin,
                SampleCards.GoblinsGreed,
                SampleCards.ShamanGoblin,
                SampleCards.HealGoblin,
                SampleCards.FireGoblin,
                SampleCards.MadScientist,
                SampleCards.BraveGoblin,
                SampleCards.GiantGoblin,
                SampleCards.LeaderGoblin,
                SampleCards.TyrantGoblin,
                SampleCards.KingGoblin,
                SampleCards.Sword,
                SampleCards.Shield,
                SampleCards.HitOrHeal,
                SampleCards.Hit,
                SampleCards.Heal,
                SampleCards.FirstAttack,
                SampleCards.SecondAttack,
                SampleCards.HolyShield,
                SampleCards.ChangeHands,
                SampleCards.Slap,
                SampleCards.FullAttack,
                SampleCards.OldShield,
                SampleCards.OldWall,
                SampleCards.EmergencyFood,
                SampleCards.GoblinStatue,
                SampleCards.HolyStatue,
                SampleCards.GoblinCaptureJar,
            });

            var actual = JsonConverter.Serialize(test);
            File.WriteAllText("write_test.json", actual);
            var expected = File.ReadAllText("CardSet/read_test.json");

            Assert.Equal(expected, actual);
        }
    }
}
