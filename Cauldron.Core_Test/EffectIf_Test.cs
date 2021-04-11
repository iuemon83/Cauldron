using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHp = player1.CurrentHp;

                // 場のカードが1枚なので発動しない
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(beforeHp, player1.CurrentHp);

                // 場のカードが2枚なので発動する
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }
    }
}
