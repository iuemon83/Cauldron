using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    /// <summary>
    /// �J�[�h���ʔ����^�C�~���O�̃e�X�g
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
        public void ���ׂẴ^�[���J�n��()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
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
        public void �����̃^�[���J�n��()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void ����̃^�[���J�n��()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void ���ׂẴ^�[���I����()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
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
        public void �����̃^�[���I����()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);
        }

        [Fact]
        public void ����̃^�[���I����()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void �J�[�h�̃v���C��()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ���������
                Assert.Equal(1, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔������Ȃ�
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void ���̃J�[�h�̃v���C��()
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

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ��������Ȃ�
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔�������
                Assert.Equal(1, testAction.CallCount);
            });
        }
    }
}
