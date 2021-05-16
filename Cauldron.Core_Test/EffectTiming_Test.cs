using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// カード効果発動タイミングのテスト
    /// </summary>
    public class EffectTiming_Test
    {
        [Fact]
        public async Task すべてのターン開始時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)))
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return new { testCard };
            });

            Assert.Equal(beforeHp, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, player1.CurrentHp);
        }

        [Fact]
        public async Task 自分のターン開始時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.You)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp, player1.CurrentHp);

            // 先行
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);
        }

        [Fact]
        public async Task 相手のターン開始時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Opponent)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            Assert.Equal(beforeHp, player1.CurrentHp);

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // 先行
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);
        }

        [Fact]
        public async Task すべてのターン終了時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Both)))
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);

                return new { testCard };
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 3, player1.CurrentHp);
        }

        [Fact]
        public async Task 自分のターン終了時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // 先行
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, player1.CurrentHp);
        }

        [Fact]
        public async Task 相手のターン終了時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Opponent)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });
            Assert.Equal(beforeHp, player1.CurrentHp);

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // 先行
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);
        }

        [Fact]
        public async Task カードのプレイ時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 1, 1);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp, player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動する
                Assert.Equal(beforeHp - 1, player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動しない
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task 他のカードのプレイ時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(Play : new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.Other)))),
                        new[]{  TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 1, 1);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動しない
                Assert.Equal(beforeHp, player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動する
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘ダメージ前時_すべてのクリーチャー()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.EventSource.Any,
                                CardCondition : new CardCondition())))),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]{
                                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 5, 0);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            // 先攻
            var normal = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = player2.CurrentHp;

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(beforeHp + 2, player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 2, player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(beforeHp + 4, player2.CurrentHp);

                return (testCard, normal2);
            });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 4, player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, normal2.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(beforeHp + 6, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘ダメージ前時_自分が攻撃()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new (
                                Source: EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })))),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 5, 0);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            // 先攻
            var normal = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = player2.CurrentHp;

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(beforeHp + 1, player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 1, player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの戦闘時に発動しない
                Assert.Equal(beforeHp + 1, player2.CurrentHp);

                return (testCard, normal2);
            });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 1, player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 攻撃されたときも発動する
                Assert.Equal(beforeHp + 2, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘ダメージ前時_ほかカードが防御()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })))),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 5, 0);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            // 先攻
            var normal = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = player2.CurrentHp;

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(beforeHp - 1, player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp - 1, player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの戦闘時に発動する
                // 攻撃と防御の2度発動する
                Assert.Equal(beforeHp - 3, player2.CurrentHp);

                return (testCard, normal2);
            });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp - 3, player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 攻撃されたときも発動する
                Assert.Equal(beforeHp - 4, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘以外のダメージ前時_自分が防御()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(DamageBefore: new(
                                Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                CardCondition: new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }
                            )))
                        ),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            // クリーチャーに1ダメージの魔法
            var testSorceryDef = SampleCards.Sorcery(0, "test2", "test2",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                                },
                                            }))
                                )
                            }
                        }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testSorceryDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testSorceryDef.Id);
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task 自分の破壊時()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouCemetery,
                            new(new(Destroy: new (EffectTimingDestroyEvent.EventSource.This)))
                        ),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);
                await g.DestroyCard(testCard);
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }
    }
}
