using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    public class ChoiceCandidate_Test
    {
        [Fact]
        public void ランダムな自分クリーチャー1体()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = ZoneType.YouField,
                    TypeCondition = new CardTypeCondition()
                    {
                        Value = new[] { CardType.Creature },
                    },
                },
                NumPicks = 1,
                How = Choice.ChoiceHow.Random
            };

            var testCardDef = CardDef.CreatureCard(0, $"test.テストクリーチャー", "テストクリーチャー", "テストクリーチャー", 2, 2,
                effects: new[]
                {
                    new CardEffect2()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, goblinCard2, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);

            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                PlayerIdList = Array.Empty<Guid>(),
                CardDefList = Array.Empty<CardDef>(),
                CardList = new[] { goblinCard, goblinCard2 },
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertCollection(
                Array.Empty<Guid>(),
                actual.PlayerIdList);

            TestUtil.AssertCollection(
                Array.Empty<CardDef>(),
                actual.CardDefList);

            // どっちかが選ばれている
            Assert.Single(actual2.CardList);
            Assert.Contains(actual2.CardList, c => new[] { goblinCard.Id, goblinCard2.Id }.Contains(c.Id));
        }

        [Fact]
        public void 自分のクリーチャーすべて()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = ZoneType.YouField,
                    TypeCondition = new CardTypeCondition()
                    {
                        Value = new[] { CardType.Creature },
                    }
                },
                How = Choice.ChoiceHow.All,
            };

            var testCardDef = CardDef.CreatureCard(0, $"test.テストクリーチャー", "テストクリーチャー", "テストクリーチャー", 3, 3,
                effects: new[]
                {
                    new CardEffect2()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, goblinCard2, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                PlayerIdList = Array.Empty<Guid>(),
                CardDefList = Array.Empty<CardDef>(),
                CardList = new[] { goblinCard, goblinCard2 },
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult()
            {
                PlayerIdList = Array.Empty<Guid>(),
                CardDefList = Array.Empty<CardDef>(),
                CardList = new[] { goblinCard, goblinCard2 },
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void カードプールから名前指定で一枚()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var fairy = CardDef.TokenCard(0, $"test.フェアリー", "フェアリー", "テストクリーチャー", 1, 1);

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                CardCondition = new CardCondition()
                {
                    ZoneCondition = ZoneType.CardPool,
                    NameCondition = new TextCondition()
                    {
                        Value = fairy.FullName,
                        Compare = TextCondition.ConditionCompare.Equality
                    }
                },
                NumPicks = 1,
            };

            var testCardDef = CardDef.CreatureCard(0, $"test.テストクリーチャー", "テストクリーチャー", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    new CardEffect2()
                    {
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent()
                            {
                                Owner = EffectTimingDestroyEvent.EventOwner.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, fairy, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                PlayerIdList = Array.Empty<Guid>(),
                CardDefList = new[] { fairy },
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult()
            {
                PlayerIdList = Array.Empty<Guid>(),
                CardDefList = new[] { fairy },
                CardList = Array.Empty<Card>()
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void カードプールから名前指定で2枚()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var fairy = CardDef.TokenCard(0, $"test.フェアリー", "フェアリー", "テストクリーチャー", 1, 1);

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                CardCondition = new CardCondition()
                {
                    ZoneCondition = ZoneType.CardPool,
                    NameCondition = new TextCondition()
                    {
                        Value = fairy.FullName,
                        Compare = TextCondition.ConditionCompare.Equality
                    }
                },
                NumPicks = 2,
            };

            var testCardDef = CardDef.CreatureCard(0, $"test.テストクリーチャー", "テストクリーチャー", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    new CardEffect2()
                    {
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent()
                            {
                                Owner = EffectTimingDestroyEvent.EventOwner.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, fairy, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                PlayerIdList = Array.Empty<Guid>(),
                CardDefList = new[] { fairy, fairy },
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult()
            {
                PlayerIdList = Array.Empty<Guid>(),
                CardDefList = new[] { fairy, fairy },
                CardList = Array.Empty<Card>()
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void 相手プレイヤーが選択される()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.OtherOwnerPlayer },
                NumPicks = 1
            };
            var testCardDef = CardDef.CreatureCard(0, $"test.テストクリーチャー", "テストクリーチャー", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    new CardEffect2(){
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent(){
                                Owner = EffectTimingDestroyEvent.EventOwner.This
                            }
                        },
                        Actions =new []
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage()
                                {
                                    Choice =testChoice
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                PlayerIdList = new[] { player2Id },
                CardDefList = Array.Empty<CardDef>(),
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var expected2 = new ChoiceResult()
            {
                PlayerIdList = new[] { player2Id },
                CardDefList = Array.Empty<CardDef>(),
                CardList = Array.Empty<Card>()
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void ターン中のプレイヤーが選択される()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.TurnPlayer }
            };
            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                new[]
                {
                    // ターン開始時、カレントプレイヤーに1ダメージ
                    new CardEffect2()
                    {
                        Timing = new EffectTiming()
                        {
                            StartTurn = new EffectTimingStartTurnEvent()
                            {
                                Source = EffectTimingStartTurnEvent.EventSource.Both,
                            }
                        },
                        Actions = new []{
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage()
                                {
                                    Choice = testChoice,
                                    Value = 1
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ゴブリン出してから効果クリーチャーを出す
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 候補の検証
                var expected = new ChoiceResult()
                {
                    PlayerIdList = new[] { player1Id },
                    CardDefList = Array.Empty<CardDef>(),
                    CardList = Array.Empty<Card>()
                };
                var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual);

                // 抽出結果の検証
                var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual2);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                PlayerIdList = new[] { player2Id },
                CardDefList = Array.Empty<CardDef>(),
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual2);
        }

        [Fact]
        public void ランダムな相手クリーチャー一体()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Random,
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                CardCondition = new CardCondition()
                {
                    ZoneCondition = ZoneType.OpponentField,
                    TypeCondition = new CardTypeCondition()
                    {
                        Value = new[] { CardType.Creature }
                    }
                },
                NumPicks = 1
            };
            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test");

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 候補の検証
                var expected = new ChoiceResult()
                {
                    CardList = new[] { goblinCard, goblinCard2 }
                };
                var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual);

                // 抽出結果の検証
                var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual2, 1);
            });
        }

        [Fact]
        public void 相手プレイヤーかランダムな相手クリーチャー1体()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.Card, Choice.ChoiceCandidateType.OtherOwnerPlayer },
                NumPicks = 1,
                How = Choice.ChoiceHow.Random,
                CardCondition = new CardCondition()
                {
                    TypeCondition = new CardTypeCondition()
                    {
                        Value = new[] { CardType.Creature }
                    },
                    ZoneCondition = ZoneType.OpponentField,
                }
            };
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出す
            var (goblinCard, goblinCard2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                PlayerIdList = new[] { player1Id },
                CardList = new[] { goblinCard, goblinCard2 }
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // 抽出結果の検証
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual2, 1);
        }

        [Fact]
        public void 対象の相手クリーチャー1体()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Choose,
                NumPicks = 1,
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                CardCondition = new CardCondition()
                {
                    ZoneCondition = ZoneType.OpponentField,
                    TypeCondition = new CardTypeCondition()
                    {
                        Value = new[] { CardType.Creature, CardType.Token }
                    }
                }
            };
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // カードの選択処理のテスト
            var isCalledAskAction = false;
            ChoiceResult expected = null;
            ChoiceResult testAskCardAction(Guid _, ChoiceResult c, int i)
            {
                isCalledAskAction = true;
                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return c;
            }

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), testAskCardAction);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出す
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // 後攻
            // 効果クリーチャーを出す
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 候補の検証
            expected = new ChoiceResult()
            {
                CardList = new[] { cards.goblinCard, cards.goblinCard2 }
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            testGameMaster.ChoiceCards(testCard, testChoice, null);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public void 対象の自分クリーチャー1体()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Choose,
                NumPicks = 1,
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = ZoneType.YouField,
                    TypeCondition = new CardTypeCondition()
                    {
                        Value = new[] { CardType.Creature, CardType.Token }
                    }
                }
            };
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // カードの選択処理のテスト
            var isCalledAskAction = false;
            ChoiceResult expected = null;
            ChoiceResult testAskCardAction(Guid _, ChoiceResult c, int i)
            {
                isCalledAskAction = true;

                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return c;
            }

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), testAskCardAction);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // ゴブリン2体出してから効果カード出す
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // 候補の検証
            expected = new ChoiceResult()
            {
                CardList = new[] { cards.goblinCard, cards.goblinCard2 }
            };
            var actual = testGameMaster.ChoiceCandidates(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            testGameMaster.ChoiceCards(cards.testCard, testChoice, null);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public void 自分自身を選択する()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Me,
                }
            };
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // ゴブリンと効果カード出す
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                CardList = new[] { cards.testCard }
            };
            var actual = testGameMaster.ChoiceCandidates(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            var actual2 = testGameMaster.ChoiceCards(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual2);
        }

        [Fact]
        public void イベントソースを選択する()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testChoice = new Choice()
            {
                Candidates = new[] { Choice.ChoiceCandidateType.Card },
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.EventSource,
                }
            };
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // ゴブリンと効果カード出す
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // 候補の検証
            var expected = new ChoiceResult()
            {
                CardList = new[] { goblinCard }
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, goblinCard);
            TestUtil.AssertChoiceResult(expected, actual);

            // カード選択処理のテスト
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, goblinCard);
            TestUtil.AssertChoiceResult(expected, actual2);
        }
    }
}
