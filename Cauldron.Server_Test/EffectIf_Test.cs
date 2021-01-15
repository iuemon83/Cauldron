using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using Cauldron.Server.Models.Effect.Value;
using System;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    /// <summary>
    /// カード効果発動条件のテスト
    /// </summary>
    public class EffectIf_Test
    {
        private record TestEffectAction : EffectAction
        {
            public int CallCount = 0;

            public Action<Card, EffectEventArgs> Action { get; set; }

            public override (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs effectEventArgs)
            {
                this.CallCount++;

                this.Action?.Invoke(ownerCard, effectEventArgs);

                return (true, effectEventArgs);
            }
        }

        [Fact]
        public void 自分の場のカードが2枚以上()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, "test", "", 1, 5,
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
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook(
                DefaultNumTurnsToCanAttack: 0));
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // 以下テスト
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 場のカードが1枚なので発動しない
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(0, testAction.CallCount);

                // 場のカードが2枚なので発動する
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(1, testAction.CallCount);
            });
        }
    }
}
