using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{

    /// <summary>
    /// カード効果発動条件のテスト
    /// </summary>
    public class EffectIf_Test
    {
        [Fact]
        public async Task 自分の場のカードが2枚以上()
        {
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(Play: new(EffectTimingPlayEvent.EventSource.This))),
                            If: new(
                                new NumCondition(2, NumCondition.ConditionCompare.GreaterThan),
                                new NumValue(
                                    NumValueCalculator: new(NumValueCalculator.ValueType.Count,
                                        new Choice()
                                        {
                                            How = Choice.ChoiceHow.All,
                                            CardCondition = new CardCondition()
                                            {
                                                ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                            }
                                        }
                                        )
                                ))
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // 以下テスト
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHp = g.PlayersById[pId].CurrentHp;

                // 場のカードが1枚なので発動しない
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(beforeHp, g.PlayersById[pId].CurrentHp);

                // 場のカードが2枚なので発動する
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(beforeHp - 1, g.PlayersById[pId].CurrentHp);
            });
        }
    }
}
