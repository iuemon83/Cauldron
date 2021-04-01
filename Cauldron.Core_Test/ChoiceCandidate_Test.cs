using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    public class ChoiceCandidate_Test
    {
        [Fact]
        public async Task ランダムな自分クリーチャー1体()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblinDef.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = new(new(new[] { ZonePrettyName.YouField })),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature })
                },
                NumPicks = 1,
                How = Choice.ChoiceHow.Random
            };

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", "テストクリーチャー", 2, 2, 1,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard(
                                    testChoice
                                )
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, goblinCard2, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
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
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = new(new(new[] { ZonePrettyName.YouField })),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature }),
                },
                How = Choice.ChoiceHow.All,
            };

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", "テストクリーチャー", 3, 3, 1,
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard(
                                    testChoice
                                )
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, goblinCard2, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var eventargs = new EffectEventArgs(GameEvent.OnBattle, testGameMaster);
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task カードプールから名前指定で一枚()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = SampleCards.Creature(0, "フェアリー", "テストクリーチャー", 1, 1, 1, isToken: true);

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new(new[] { ZonePrettyName.CardPool })),
                    NameCondition = new TextCondition(
                        new TextValue(fairy.FullName),
                        TextCondition.ConditionCompare.Equality
                    )
                },
                NumPicks = 1,
            };

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", "テストクリーチャー", 1, 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)))
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new(
                                    new ZoneValue(new[]{ ZonePrettyName.OpponentField }),
                                    testChoice
                                )
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, fairy, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy }
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task カードプールから名前指定で2枚()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = SampleCards.Creature(0, "フェアリー", "テストクリーチャー", 1, 1, 1, isToken: true);

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new(new[] { ZonePrettyName.CardPool })),
                    NameCondition = new(
                        new TextValue(fairy.FullName),
                        TextCondition.ConditionCompare.Equality
                    )
                },
                NumPicks = 2,
            };

            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", "テストクリーチャー", 1, 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)))
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new(
                                    new ZoneValue(new[]{ ZonePrettyName.OpponentField }),
                                    testChoice
                                )
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, fairy, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy, fairy }
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task 相手プレイヤーが選択される()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                PlayerCondition = new PlayerCondition()
                {
                    Type = PlayerCondition.PlayerConditionType.Opponent,
                }
            };
            var testCardDef = SampleCards.Creature(0, "テストクリーチャー", "テストクリーチャー", 1, 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)))
                        ),
                        new []
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(1),
                                    testChoice
                                )
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult2(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task ターン中のプレイヤーが選択される()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                PlayerCondition = new PlayerCondition()
                {
                    Type = PlayerCondition.PlayerConditionType.Active
                }
            };
            var testCardDef = SampleCards.Artifact(0, "test", "test", false,
                new[]
                {
                    // ターン開始時、カレントプレイヤーに1ダメージ
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)))
                        ),
                        new []{
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(1),
                                    testChoice
                                )
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 候補の検証
                var expected = new ChoiceCandidates(
                     new[] { player1Id },
                     Array.Empty<Card>(),
                     Array.Empty<CardDef>()
                 );
                var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
                TestUtil.AssertChoiceResult(expected, actual);

                // 抽出結果の検証
                var expected2 = new ChoiceResult2(
                     new[] { player1Id },
                     Array.Empty<Card>(),
                     Array.Empty<CardDef>()
                 );
                var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
                TestUtil.AssertChoiceResult(expected2, actual2);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult2(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task ランダムな相手クリーチャー一体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Random,
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new(new[] { ZonePrettyName.OpponentField })),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature })
                },
                NumPicks = 1
            };
            var testCardDef = SampleCards.Artifact(0, "test", "test", false);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // 以下テスト
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 候補の検証
                var expected = new ChoiceCandidates(
                     Array.Empty<PlayerId>(),
                     new[] { goblinCard, goblinCard2 },
                     Array.Empty<CardDef>()
                 );

                var eventargs = new EffectEventArgs(GameEvent.OnBattle, testGameMaster);
                var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, eventargs);
                TestUtil.AssertChoiceResult(expected, actual);

                // 抽出結果の検証
                var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, eventargs);
                TestUtil.AssertChoiceResult(expected, actual2, 1);
            });
        }

        [Fact]
        public async Task 相手プレイヤーかランダムな相手クリーチャー1体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                PlayerCondition = new PlayerCondition()
                {
                    Type = PlayerCondition.PlayerConditionType.Opponent,
                },
                NumPicks = 1,
                How = Choice.ChoiceHow.Random,
                CardCondition = new CardCondition()
                {
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature }),
                    ZoneCondition = new(new(new[] { ZonePrettyName.OpponentField })),
                }
            };
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // 以下テスト
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var testCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 候補の検証
            var expected = new ChoiceCandidates(
                new[] { player1Id },
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var eventargs = new EffectEventArgs(GameEvent.OnBattle, testGameMaster);
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected, actual2, 1);
        }

        [Fact]
        public async Task 対象の相手クリーチャー1体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Choose,
                NumPicks = 1,
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new(new[] { ZonePrettyName.OpponentField })),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature })
                }
            };
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // カードの選択処理のテスト
            var isCalledAskAction = false;
            ChoiceCandidates expected = null;
            ValueTask<ChoiceResult> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                isCalledAskAction = true;
                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return ValueTask.FromResult(new ChoiceResult(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                    ));
            }

            // 以下テスト
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var (goblinCard3, testcard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var eventargs = new EffectEventArgs(GameEvent.OnBattle, testGameMaster);
            var actual = await testGameMaster.ChoiceCandidates(testcard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            await testGameMaster.ChoiceCards(testcard, testChoice, eventargs);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public async Task 対象の自分クリーチャー1体()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Choose,
                NumPicks = 1,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = new(new(new[] { ZonePrettyName.YouField })),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature, })
                }
            };
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // カードの選択処理のテスト
            var isCalledAskAction = false;
            ChoiceCandidates expected = null;
            ValueTask<ChoiceResult> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                isCalledAskAction = true;

                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);

                return ValueTask.FromResult(new ChoiceResult(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                    ));
            }

            // 以下テスト
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var (goblinCard3, goblinCard4, testcard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var eventargs = new EffectEventArgs(GameEvent.OnBattle, testGameMaster);
            var actual = await testGameMaster.ChoiceCandidates(testcard, testChoice, eventargs);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            await testGameMaster.ChoiceCards(testcard, testChoice, eventargs);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public async Task 自分自身を選択する()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.This,
                }
            };
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // 以下テスト
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先行
            // ゴブリンと効果カード出す
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var actual = await testGameMaster.ChoiceCandidates(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                new[] { cards.testCard },
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task イベントソースを選択する()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.EventSource,
                }
            };
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // 以下テスト
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先行
            // ゴブリンと効果カード出す
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice,
                new EffectEventArgs(GameEvent.OnBattle, testGameMaster, SourceCard: goblinCard));
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                new[] { goblinCard },
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice,
                new EffectEventArgs(GameEvent.OnBattle, testGameMaster, SourceCard: goblinCard));
            TestUtil.AssertChoiceResult(expected2, actual2);
        }
    }
}
