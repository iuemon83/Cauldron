using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cauldron.Core_Test
{
    public class ChoiceCandidate_Test
    {
        private readonly ITestOutputHelper output;

        public ChoiceCandidate_Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task ランダムな自分クリーチャー1体()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 1, 2);

            var testChoice = new Choice(
                new ChoiceSource(
                    orCardConditions: new[]{
                        new CardCondition(
                            ContextCondition: CardCondition.ContextConditionValue.Others,
                            ZoneCondition: new(new(new[] { ZonePrettyName.YouField })),
                            TypeCondition: new CardTypeCondition(new[] { CardType.Creature })
                        )
                    }),
                Choice.HowValue.Random,
                new NumValue(1));

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", 2, 2,
                effects: new[]
                {
                    new CardEffect(
                        "",
                        new EffectConditionWrap(
                            ByPlay: new()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new EffectActionModifyCard(
                                    testChoice
                                ))
                        }
                    )
                }
                );

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef }, this.output);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, goblinCard2, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var numPicks = await testChoice.NumPicks.Calculate(
                testCard, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster));
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard,
                new EffectEventArgs(GameEvent.OnAttack, c.GameMaster),
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var actual2 = await c.GameMaster.Choice(testCard, testChoice,
                new EffectEventArgs(GameEvent.OnAttack, c.GameMaster));
            TestUtil.AssertCollection(
                Array.Empty<PlayerId>(),
                actual.PlayerIdList);

            TestUtil.AssertCollection(
                Array.Empty<CardDef>(),
                actual.CardDefList);

            // どっちかが選ばれている
            Assert.Single(actual2.CardList);
            Assert.Contains(actual2.CardList, c => new[] { goblinCard.Id, goblinCard2.Id }.Contains(c.Id));
        }

        [Fact]
        public async Task 自分のクリーチャーすべて()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orCardConditions: new[]{
                        new CardCondition(
                            ContextCondition: CardCondition.ContextConditionValue.Others,
                            ZoneCondition: new(new(new[] { ZonePrettyName.YouField })),
                            TypeCondition: new CardTypeCondition(new[] { CardType.Creature })
                        )
                    }));

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", 3, 3,
                effects: new[]
                {
                    new CardEffect(
                        "",
                        new EffectConditionWrap(
                            ByPlay: new()),
                        new[]
                        {
                            new EffectAction(
                                ModifyCard: new EffectActionModifyCard(
                                    testChoice
                                ))
                        }
                    )
                }
                );

            var c = await TestUtil.InitTest(new[] { goblin, testCardDef }, this.output);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, goblinCard2, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var eventargs = new EffectEventArgs(GameEvent.OnAttack, c.GameMaster);
            var numPicks = await testChoice.NumPicks.Calculate(testCard, eventargs);
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, eventargs,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual2 = await c.GameMaster.Choice(testCard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task カードプールから名前指定で一枚()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = SampleCards.Creature(0, "フェアリー", 1, 1, isToken: true);

            var testChoice = new Choice(
                new ChoiceSource(
                    OrCardDefConditions: new[]
                    {
                        new CardDefCondition(
                            OutZoneCondition: new(new[] { OutZonePrettyName.CardPool }),
                            NameCondition: new TextCompare(
                                new TextValue(fairy.FullName),
                                TextCompare.CompareValue.Equality
                            ))
                    }));

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "",
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(Destroy: new(
                                OrCardCondition: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.This)
                                })))
                            )),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    testChoice,
                                    new ZoneValue(new[]{ ZonePrettyName.OpponentField })
                                ))
                        }
                    )
                }
                );

            var c = await TestUtil.InitTest(new[] { goblin, fairy, testCardDef }, this.output);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy }
            );
            var numPicks = await testChoice.NumPicks.Calculate(testCard, null);
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, null,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy }
            );
            var actual2 = await c.GameMaster.Choice(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task カードプールから名前指定で2枚()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = SampleCards.Creature(0, "フェアリー", 1, 1, isToken: true);

            var testChoice = new Choice(
                new ChoiceSource(
                    OrCardDefConditions: new[]
                    {
                        new CardDefCondition(
                            OutZoneCondition: new(new[] { OutZonePrettyName.CardPool }),
                            NameCondition: new(
                                new TextValue(fairy.FullName),
                                TextCompare.CompareValue.Equality
                            ))
                    }),
                numPicks: new NumValue(2));

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                                ZonePrettyName.YouField,
                                new(new(Destroy: new(
                                    OrCardCondition: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    })))
                            )),
                        new[]
                        {
                            new EffectAction(
                                AddCard: new(
                                    testChoice,
                                    new ZoneValue(new[]{ ZonePrettyName.OpponentField })
                                ))
                        }
                    )
                }
                );

            var c = await TestUtil.InitTest(new[] { goblin, fairy, testCardDef }, this.output);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy, fairy }
            );
            var numPicks = await testChoice.NumPicks.Calculate(testCard, null);
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, null,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy, fairy }
            );
            var actual2 = await c.GameMaster.Choice(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task 相手プレイヤーが選択される()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orPlayerConditions: new[]
                    {
                        new PlayerCondition(
                            PlayerCondition.ContextValue.Opponent
                        )
                    }));
            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        "",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                                ZonePrettyName.YouField,
                                new(new(Destroy: new(
                                    OrCardCondition: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    })))
                            )),
                        new []
                        {
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(1),
                                    testChoice
                                ))
                        }
                    )
                }
                );

            var c = await TestUtil.InitTest(new[] { goblin, testCardDef }, this.output);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                new[] { c.Player2.Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var numPicks = await testChoice.NumPicks.Calculate(testCard, null);
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, null,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult(
                new[] { c.Player2.Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual2 = await c.GameMaster.Choice(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task ターン中のプレイヤーが選択される()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orPlayerConditions: new[]
                    {
                        new PlayerCondition(
                            PlayerCondition.ContextValue.Active
                        )
                    }));
            var testCardDef = SampleCards.Artifact(0, "test", false,
                effects: new[]
                {
                    // ターン開始時、カレントプレイヤーに1ダメージ
                    new CardEffect(
                        "",
                        new EffectConditionWrap(
                            ByNotPlay: new(
                                ZonePrettyName.YouField,
                                new(new(StartTurn: new(
                                    OrPlayerCondition: new[]
                                    {
                                        new PlayerCondition()
                                    })))
                            )),
                        new []{
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(1),
                                    testChoice
                                ))
                        }
                    )
                }
                );

            var c = await TestUtil.InitTest(new[] { goblin, testCardDef }, this.output);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 候補の検証
                var expected = new ChoiceCandidates(
                     new[] { c.Player1.Id },
                     Array.Empty<Card>(),
                     Array.Empty<CardDef>()
                 );
                var numPicks = await testChoice.NumPicks.Calculate(
                    testCard, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster));
                var actual = await testChoice.Source.ChoiceCandidates(
                    testCard, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster),
                    c.GameMaster.playerRepository,
                    c.CardRepository,
                    testChoice.How,
                    numPicks);
                TestUtil.AssertChoiceResult(expected, actual);

                // 抽出結果の検証
                var expected2 = new ChoiceResult(
                     new[] { c.Player1.Id },
                     Array.Empty<Card>(),
                     Array.Empty<CardDef>()
                 );
                var actual2 = await c.GameMaster.Choice(testCard, testChoice, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster));
                TestUtil.AssertChoiceResult(expected2, actual2);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                new[] { c.Player2.Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var numPicks = await testChoice.NumPicks.Calculate(
                testCard, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster));
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster),
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult(
                new[] { c.Player2.Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual2 = await c.GameMaster.Choice(testCard, testChoice, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster));
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task ランダムな相手クリーチャー一体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orCardConditions: new[]
                    {
                        new CardCondition(
                            ZoneCondition: new(new(new[] { ZonePrettyName.OpponentField })),
                            TypeCondition: new CardTypeCondition(new[] { CardType.Creature })
                        )
                    }),
                Choice.HowValue.Random,
                new NumValue(1));
            var testCardDef = SampleCards.Artifact(0, "test", false);

            // 以下テスト
            var c = await TestUtil.InitTest(new[] { goblin, testCardDef }, this.output);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 候補の検証
                var expected = new ChoiceCandidates(
                     Array.Empty<PlayerId>(),
                     new[] { goblinCard, goblinCard2 },
                     Array.Empty<CardDef>()
                 );

                var eventargs = new EffectEventArgs(GameEvent.OnAttack, c.GameMaster);
                var numPicks = await testChoice.NumPicks.Calculate(testCard, eventargs);
                var actual = await testChoice.Source.ChoiceCandidates(
                    testCard, eventargs,
                    c.GameMaster.playerRepository,
                    c.CardRepository,
                    testChoice.How,
                    numPicks);
                TestUtil.AssertChoiceResult(expected, actual);

                // 抽出結果の検証
                var actual2 = await c.GameMaster.Choice(testCard, testChoice, eventargs);
                TestUtil.AssertChoiceResult(expected, actual2, 1);
            });
        }

        [Fact]
        public async Task 相手プレイヤーかランダムな相手クリーチャー1体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orPlayerConditions: new[]
                    {
                        new PlayerCondition(
                            PlayerCondition.ContextValue.Opponent
                        )
                    },
                    orCardConditions: new[]
                    {
                        new CardCondition(
                            TypeCondition: new CardTypeCondition(new[] { CardType.Creature }),
                            ZoneCondition: new(new(new[] { ZonePrettyName.OpponentField }))
                        )
                    }),
                Choice.HowValue.Random,
                new NumValue(1));
            var testCardDef = SampleCards.Creature(0, "test", 1, 1);

            // 以下テスト
            var c = await TestUtil.InitTest(new[] { goblin, testCardDef }, this.output);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                new[] { c.Player1.Id },
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var eventargs = new EffectEventArgs(GameEvent.OnAttack, c.GameMaster);
            var numPicks = await testChoice.NumPicks.Calculate(testCard, eventargs);
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, eventargs,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var actual2 = await c.GameMaster.Choice(testCard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected, actual2, 1);
        }

        [Fact]
        public async Task 対象の相手クリーチャー1体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orCardConditions: new[]
                    {
                        new CardCondition(
                            ZoneCondition: new(new(new[] { ZonePrettyName.OpponentField })),
                            TypeCondition: new CardTypeCondition(new[] { CardType.Creature })
                        )
                    }),
                Choice.HowValue.Choose,
                new NumValue(1));
            var testCardDef = SampleCards.Creature(0, "test", 1, 1);

            // カードの選択処理のテスト
            var isCalledAskAction = false;
            ChoiceCandidates expected = null;
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                isCalledAskAction = true;
                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                    ));
            }

            // 以下テスト
            var c = await TestUtil.InitTest(new[] { goblin, testCardDef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var (goblinCard3, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard3, testcard);
            });

            // 候補の検証
            // 自軍クリーチャーは候補にならない
            expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var eventargs = new EffectEventArgs(GameEvent.OnAttack, c.GameMaster);
            var numPicks = await testChoice.NumPicks.Calculate(testCard, eventargs);
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, eventargs,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            await c.GameMaster.Choice(testCard, testChoice, eventargs);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public async Task 対象の自分クリーチャー1体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orCardConditions: new[]
                    {
                        new CardCondition(
                            ContextCondition: CardCondition.ContextConditionValue.Others,
                            ZoneCondition: new(new(new[] { ZonePrettyName.YouField })),
                            TypeCondition: new CardTypeCondition(new[] { CardType.Creature, })
                        )
                    }),
                Choice.HowValue.Choose,
                new NumValue(1));
            var testCardDef = SampleCards.Creature(0, "test", 1, 1);

            // カードの選択処理のテスト
            var isCalledAskAction = false;
            ChoiceCandidates expected = null;
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                isCalledAskAction = true;

                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);

                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                    ));
            }

            // 以下テスト
            var c = await TestUtil.InitTest(new[] { goblin, testCardDef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var (goblinCard3, goblinCard4, testcard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard3, goblinCard4, testcard);
            });

            // 候補の検証
            // 相手クリーチャーは候補にならない
            expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard3, goblinCard4 },
                Array.Empty<CardDef>()
            );
            var eventargs = new EffectEventArgs(GameEvent.OnAttack, c.GameMaster);
            var numPicks = await testChoice.NumPicks.Calculate(testcard, eventargs);
            var actual = await testChoice.Source.ChoiceCandidates(
                testcard, eventargs,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            await c.GameMaster.Choice(testcard, testChoice, eventargs);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public async Task 自分自身を選択する()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orCardConditions: new[]
                    {
                        new CardCondition(
                            ContextCondition: CardCondition.ContextConditionValue.This
                        )
                    }));
            var testCardDef = SampleCards.Creature(0, "test", 1, 1);

            // 以下テスト
            var c = await TestUtil.InitTest(new[] { goblin, testCardDef });

            // 先行
            // ゴブリンと効果カード出す
            var cards = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { cards.testCard },
                Array.Empty<CardDef>()
            );
            var numPicks = await testChoice.NumPicks.Calculate(cards.testCard, null);
            var actual = await testChoice.Source.ChoiceCandidates(
                cards.testCard, null,
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            var expected2 = new ChoiceResult(
                Array.Empty<PlayerId>(),
                new[] { cards.testCard },
                Array.Empty<CardDef>()
            );
            var actual2 = await c.GameMaster.Choice(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task イベントソースを選択する()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice(
                new ChoiceSource(
                    orCardConditions: new[]
                    {
                        new CardCondition(
                            ContextCondition: CardCondition.ContextConditionValue.EventSource
                        )
                    }));
            var testCardDef = SampleCards.Creature(0, "test", 1, 1);

            // 以下テスト
            var c = await TestUtil.InitTest(new[] { goblin, testCardDef });

            // 先行
            // ゴブリンと効果カード出す
            var (goblinCard, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard },
                Array.Empty<CardDef>()
            );
            var numPicks = await testChoice.NumPicks.Calculate(
                testCard, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster, SourceCard: goblinCard));
            var actual = await testChoice.Source.ChoiceCandidates(
                testCard, new EffectEventArgs(GameEvent.OnAttack, c.GameMaster, SourceCard: goblinCard),
                c.GameMaster.playerRepository,
                c.CardRepository,
                testChoice.How,
                numPicks);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            var expected2 = new ChoiceResult(
                Array.Empty<PlayerId>(),
                new[] { goblinCard },
                Array.Empty<CardDef>()
            );
            var actual2 = await c.GameMaster.Choice(testCard, testChoice,
                new EffectEventArgs(GameEvent.OnAttack, c.GameMaster, SourceCard: goblinCard));
            TestUtil.AssertChoiceResult(expected2, actual2);
        }
    }
}
