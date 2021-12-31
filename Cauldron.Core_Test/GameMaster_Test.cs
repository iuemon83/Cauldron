using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    public class GameMaster_Test
    {
        [Fact]
        public async Task StartTurn_Test()
        {
            var testCardDef = SampleCards.Goblin;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            var actual = await c.GameMaster.StartTurn();
            Assert.Equal(GameMasterStatusCode.OK, actual);

            actual = await c.GameMaster.StartTurn();
            Assert.Equal(GameMasterStatusCode.AlreadyTurnStarted, actual);
        }

        [Fact]
        public async Task EndTurn_Test()
        {
            var testCardDef = SampleCards.Goblin;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            var actual = await c.GameMaster.StartTurn();
            Assert.Equal(GameMasterStatusCode.OK, actual);

            actual = await c.GameMaster.EndTurn();
            Assert.Equal(GameMasterStatusCode.OK, actual);

            actual = await c.GameMaster.EndTurn();
            Assert.Equal(GameMasterStatusCode.NotTurnStart, actual);
        }

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

        [Fact]
        public async Task クリーチャーでクリーチャーに攻撃する()
        {
            var testCardDef = SampleCards.Goblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            var test1 = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var test2 = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // 相手のクリーチャーに攻撃する
                var actual = await g.AttackToCreature(pid, test2.Id, test1.Id);

                Assert.Equal(GameMasterStatusCode.OK, actual);
                // 相打ちする
                Assert.Equal(test1.BaseToughness - test2.Power, test1.Toughness);
                Assert.Equal(test2.BaseToughness - test1.Power, test2.Toughness);
                // タフネスが0でないので場に残ったまま
                Assert.Equal(Shared.ZoneName.Field, test1.Zone.ZoneName);
                Assert.Equal(Shared.ZoneName.Field, test2.Zone.ZoneName);

                var test3 = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // 相手のクリーチャーに攻撃する
                var actual2 = await g.AttackToCreature(pid, test3.Id, test1.Id);

                Assert.Equal(GameMasterStatusCode.OK, actual2);
                // 相打ちする
                Assert.Equal(0, test1.Toughness);
                Assert.Equal(test3.BaseToughness - test1.Power, test3.Toughness);
                // タフネスが0なので墓地に行く
                Assert.Equal(Shared.ZoneName.Cemetery, test1.Zone.ZoneName);
                Assert.Equal(Shared.ZoneName.Field, test3.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task クリーチャーでプレイヤーに攻撃する()
        {
            var testCardDef = SampleCards.Goblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var beforePlayerHp = c.Player1.CurrentHp;

                var test = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);
                var actual = await g.AttackToPlayer(pid, test.Id, c.Player2.Id);

                Assert.Equal(GameMasterStatusCode.OK, actual);
                // 相手のHPが減少している
                Assert.Equal(beforePlayerHp - test.Power, c.Player2.CurrentHp);
            });
        }
    }
}
