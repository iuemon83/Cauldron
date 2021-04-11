using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// カード効果発動期間のテスト
    /// </summary>
    public class EffectWhile_Test
    {
        [Fact]
        public async Task ターン終了時まで()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "", 2, 2);
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    Source: EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                    CardCondition: new()
                                    {
                                        Context = CardCondition.CardConditionContext.This,
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                    }
                                    ))),
                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 0, 0)
                        ),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        PlayerCondition = new(Type: PlayerCondition.PlayerConditionType.You),
                                    },
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var goblin = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            var beforeHp = player2.CurrentHp;

            // 後攻
            var testCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 効果が発動する
                Assert.Equal(beforeHp, player2.CurrentHp);
                var status = await g.AttackToCreature(pId, testCard.Id, goblin.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(beforeHp + 1, player2.CurrentHp);

                return testCard;
            });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ターンをまたいだので効果が発動しない
                Assert.Equal(beforeHp + 1, player2.CurrentHp);
                var status = await g.AttackToCreature(pId, goblin.Id, testCard.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(beforeHp + 1, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task 次の自分ターン開始時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You))),
                            While: new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)), 0, 1)
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            var beforeHp = player1.CurrentHp;

            // 1ターン目
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 効果が発動しない
            Assert.Equal(beforeHp, player1.CurrentHp);

            // 2ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 効果が発動する
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 2度目は効果が発動しない
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task 次の自分ターン終了時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You))),
                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 1, 1)
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            var beforeHp = player1.CurrentHp;

            // 1ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 2ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 効果が発動しない
                Assert.Equal(beforeHp, player1.CurrentHp);
            });

            // 効果が発動する
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 2度目は効果が発動しない
            Assert.Equal(beforeHp - 1, player1.CurrentHp);
        }

        [Fact]
        public async Task 三ターン後の開始時に()
        {
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You))),
                            While: new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)), 2, 1)
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            var beforeHp = player1.CurrentHp;

            // 1ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 2ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 効果が発動しない
            Assert.Equal(beforeHp, player1.CurrentHp);

            // 4ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 効果が発動する
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3ターン目
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 2度目は効果が発動しない
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }
    }
}
