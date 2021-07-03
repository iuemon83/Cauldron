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
                SampleCards.MagicBook,
                SampleCards.GoblinFollower,
                SampleCards.GoblinsPet,
                SampleCards.ShieldGoblin,
                SampleCards.DeadlyGoblin,
                SampleCards.MindController,
                SampleCards.MechanicGoblin,
                SampleCards.NinjaGoblin,
                SampleCards.SuperNinjaGoblin,
                SampleCards.GoblinsGreed,
                SampleCards.ShamanGoblin,
                SampleCards.HealGoblin,
                SampleCards.FireGoblin,
                SampleCards.BeginnerSummoner,
                SampleCards.MadScientist,
                SampleCards.BraveGoblin,
                SampleCards.MagicDragon,
                SampleCards.GiantGoblin,
                SampleCards.LeaderGoblin,
                SampleCards.TyrantGoblin,
                SampleCards.DoctorBomb,
                SampleCards.Doctor,
                SampleCards.Firelord,
                SampleCards.KingGoblin,
                SampleCards.TempRamp,
                SampleCards.Fire,
                SampleCards.Lightning,
                SampleCards.Sword,
                SampleCards.Shield,
                SampleCards.HitOrHeal,
                SampleCards.Salvage,
                SampleCards.Recycle,
                SampleCards.SimpleReborn,
                SampleCards.Copy,
                SampleCards.DoubleCopy,
                SampleCards.Hit,
                SampleCards.Heal,
                SampleCards.FirstAttack,
                SampleCards.SecondAttack,
                SampleCards.HolyShield,
                SampleCards.ChangeHands,
                SampleCards.Ramp,
                SampleCards.Slap,
                SampleCards.FullAttack,
                SampleCards.OldShield,
                SampleCards.OldWall,
                SampleCards.EmergencyFood,
                SampleCards.Gather,
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
