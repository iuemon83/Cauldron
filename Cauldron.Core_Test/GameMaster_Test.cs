using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    public class GameMaster_Test
    {
        [Fact]
        public async Task PlayFromHand_MPが不足()
        {
            var testCardDef = SampleCards.Goblin;
            testCardDef.Cost = 2;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                // MPが不足しているのでプレイできない
                var status = await g.PlayFromHand(pid, c.Player1.Hands.AllCards[0].Id);
                Assert.Equal(GameMasterStatusCode.CardCantPlay, status);
            });
        }

        [Fact]
        public async Task PlayFromHand_タフネスが0()
        {
            var testCardDef = SampleCards.Goblin;
            testCardDef.Cost = 0;
            testCardDef.Toughness = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // タフネスが0なので、場に出ると同時に墓地へ送られる
                Assert.Empty(c.Player1.Field.AllCards);
                Assert.Single(c.Player1.Cemetery.AllCards);
                Assert.Equal(testcard.Id, c.Player1.Cemetery.AllCards[0].Id);
            });
        }
    }
}
