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
                                        new Choice(
                                            new ChoiceSource(
                                                orCardConditions: new[]{
                                                    new CardCondition()
                                                    {
                                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                    }
                                                }))
                                        )
                                ))
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHp = c.Player1.CurrentHp;

                // 場のカードが1枚なので発動しない
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(beforeHp, c.Player1.CurrentHp);

                // 場のカードが2枚なので発動する
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }
    }
}
