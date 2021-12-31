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
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new()))
                        )),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            var cards = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return new { testCard };
            });

            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task 自分のターン開始時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // 先行
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task 相手のターン開始時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(StartTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // 先行
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task すべてのターン終了時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(EndTurn: new()))
                        )),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            var cards = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);

                return new { testCard };
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 3, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task 自分のターン終了時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(EndTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // 先行
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task 相手のターン終了時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(EndTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });
            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // 先行
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task カードのプレイ時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 1);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp, c.Player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動する
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動しない
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task 他のカードのプレイ時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(Play : new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.Others)
                                }))))),
                        new[]{  TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 1);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先行
            // 効果カード出す
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動しない
                Assert.Equal(beforeHp, c.Player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動する
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘ダメージ前時_すべてのクリーチャー()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5, numTurnsToCanAttack: 0,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.SourceValue.Any,
                                CardCondition : new CardCondition()))))),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]{
                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 5, numTurnsToCanAttack: 0);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            // 先攻
            var normal = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = c.Player2.CurrentHp;

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(beforeHp + 2, c.Player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 2, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(beforeHp + 4, c.Player2.CurrentHp);

                return (testCard, normal2);
            });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 4, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, normal2.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(beforeHp + 6, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘ダメージ前時_自分が攻撃()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5, numTurnsToCanAttack: 0,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore : new (
                                Source: EffectTimingDamageBeforeEvent.SourceValue.DamageSource,
                                CardCondition : new CardCondition(
                                    ContextCondition: CardCondition.ContextConditionValue.This
                                    )))))),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 5, numTurnsToCanAttack: 0);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            // 先攻
            var normal = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = c.Player2.CurrentHp;

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの戦闘時に発動しない
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);

                return (testCard, normal2);
            });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 攻撃されたときも発動する
                Assert.Equal(beforeHp + 2, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘ダメージ前時_ほかカードが防御()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5, numTurnsToCanAttack: 0,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                CardCondition : new CardCondition(
                                    ContextCondition: CardCondition.ContextConditionValue.Others
                                    )))))),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 5, numTurnsToCanAttack: 0);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            // 先攻
            var normal = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = c.Player2.CurrentHp;

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(beforeHp - 1, c.Player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp - 1, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの戦闘時に発動する
                // 攻撃と防御の2度発動する
                Assert.Equal(beforeHp - 3, c.Player2.CurrentHp);

                return (testCard, normal2);
            });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp - 3, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 攻撃されたときも発動する
                Assert.Equal(beforeHp - 4, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task 戦闘以外のダメージ前時_自分が防御()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore: new(
                                Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                CardCondition: new CardCondition(
                                    ContextCondition: CardCondition.ContextConditionValue.This
                                )
                            )))
                        )),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            // クリーチャーに1ダメージの魔法
            var testSorceryDef = SampleCards.Sorcery(0, "test2", "test2",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature })
                                                )
                                            }))
                                ))
                        }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef, testSorceryDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testSorceryDef.Id);
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task 自分の破壊時()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(
                            ByNotPlay: new(
                                ZonePrettyName.YouCemetery,
                                new(new(Destroy: new(
                                    OrCardCondition: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    })))
                            )),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);
                await g.DestroyCard(testCard);
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }
    }
}
