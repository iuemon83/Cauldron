using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    /// <summary>
    /// カード効果発動タイミングのテスト
    /// </summary>
    public class EffectTiming_Test
    {
        private class TestEffectAction : IEffectAction
        {
            public int CallCount = 0;

            public void Execute(GameMaster gameMaster, Card ownerCard, Card eventSource)
            {
                this.CallCount++;
            }

            public System.Action<EffectEventArgs> Execute(Card ownerCard) => _ =>
            {
                this.CallCount++;
            };
        }

        [Fact]
        public void すべてのターン開始時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            StartTurn = new EffectTimingStartTurnEvent()
                            {
                                Source = EffectTimingStartTurnEvent.EventSource.Both,
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);

                return new { testCard };
            });
            Assert.Equal(0, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(2, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);
        }

        [Fact]
        public void 自分のターン開始時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            StartTurn = new EffectTimingStartTurnEvent()
                            {
                                Source = EffectTimingStartTurnEvent.EventSource.Owner,
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void 相手のターン開始時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            StartTurn = new EffectTimingStartTurnEvent()
                            {
                                Source = EffectTimingStartTurnEvent.EventSource.Other,
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void すべてのターン終了時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            EndTurn = new EffectTimingEndTurnEvent()
                            {
                                Source = EffectTimingEndTurnEvent.EventSource.Both,
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);

                return new { testCard };
            });
            Assert.Equal(1, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(2, testAction.CallCount);
            });
            Assert.Equal(3, testAction.CallCount);
        }

        [Fact]
        public void 自分のターン終了時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            EndTurn = new EffectTimingEndTurnEvent()
                            {
                                Source = EffectTimingEndTurnEvent.EventSource.Owner,
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);
        }

        [Fact]
        public void 相手のターン終了時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            EndTurn = new EffectTimingEndTurnEvent()
                            {
                                Source = EffectTimingEndTurnEvent.EventSource.Other,
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void カードのプレイ時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testNormalCardDef = CardDef.CreatureCard(0, $"test.test2", "test2", "test2", 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動する
                Assert.Equal(1, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動しない
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void 他のカードのプレイ時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.CreatureCard(0, $"test.test", "test", "test", 1, 1,
                effects: new[]{
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.Other
                            }
                        },
                        Actions = new[]{ testAction }
                    }
                });

            var testNormalCardDef = CardDef.CreatureCard(0, $"test.test2", "test2", "test2", 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動しない
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動する
                Assert.Equal(1, testAction.CallCount);
            });
        }
    }
}
