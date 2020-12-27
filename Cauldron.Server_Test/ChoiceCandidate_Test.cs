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
        public void �����_���Ȏ����N���[�`���[1��()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = new(new[] { ZoneType.YouField }),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature })
                },
                NumPicks = 1,
                How = Choice.ChoiceHow.Random
            };

            var testCardDef = CardDef.Creature(0, $"test.�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 2, 2, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            Play: new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.This)
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, goblinCard2, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                PlayerList = Array.Empty<Player>(),
                //CardDefList = Array.Empty<CardDef>(),
                CardList = new[] { goblinCard, goblinCard2 },
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertCollection(
                Array.Empty<Player>(),
                actual.PlayerList);

            //TestUtil.AssertCollection(
            //    Array.Empty<CardDef>(),
            //    actual.CardDefList);

            // �ǂ��������I�΂�Ă���
            Assert.Single(actual2.CardList);
            Assert.Contains(actual2.CardList, c => new[] { goblinCard.Id, goblinCard2.Id }.Contains(c.Id));
        }

        [Fact]
        public void �����̃N���[�`���[���ׂ�()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = new(new[] { ZoneType.YouField }),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature }),
                },
                How = Choice.ChoiceHow.All,
            };

            var testCardDef = CardDef.Creature(0, $"test.�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 3, 3, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            Play: new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.This)
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, goblinCard2, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                PlayerList = Array.Empty<Player>(),
                //CardDefList = Array.Empty<CardDef>(),
                CardList = new[] { goblinCard, goblinCard2 },
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult()
            {
                PlayerList = Array.Empty<Player>(),
                //CardDefList = Array.Empty<CardDef>(),
                CardList = new[] { goblinCard, goblinCard2 },
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void �J�[�h�v�[�����疼�O�w��ňꖇ()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = CardDef.Creature(0, $"test.�t�F�A���[", "�t�F�A���[", "�e�X�g�N���[�`���[", 1, 1, 1, isToken: true);

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new[] { ZoneType.CardPool }),
                    NameCondition = new TextCondition(
                        fairy.FullName,
                        TextCondition.ConditionCompare.Equality
                    )
                },
                NumPicks = 1,
            };

            var testCardDef = CardDef.Creature(0, $"test.�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 1, 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, fairy, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                PlayerList = Array.Empty<Player>(),
                //CardDefList = new[] { fairy },
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult()
            {
                PlayerList = Array.Empty<Player>(),
                //CardDefList = new[] { fairy },
                CardList = Array.Empty<Card>()
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void �J�[�h�v�[�����疼�O�w���2��()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = CardDef.Creature(0, $"test.�t�F�A���[", "�t�F�A���[", "�e�X�g�N���[�`���[", 1, 1, 1, isToken: true);

            var testChoice = new Choice()
            {
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new[] { ZoneType.CardPool }),
                    NameCondition = new(
                        fairy.FullName,
                        TextCondition.ConditionCompare.Equality
                    )
                },
                NumPicks = 2,
            };

            var testCardDef = CardDef.Creature(0, $"test.�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 1, 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    Choice = testChoice
                                }
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, fairy, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                PlayerList = Array.Empty<Player>(),
                //CardDefList = new[] { fairy, fairy },
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult()
            {
                PlayerList = Array.Empty<Player>(),
                //CardDefList = new[] { fairy, fairy },
                CardList = Array.Empty<Card>()
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void ����v���C���[���I�������()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                PlayerCondition = new PlayerCondition()
                {
                    Type = PlayerCondition.PlayerConditionType.Opponent,
                }
            };
            var testCardDef = CardDef.Creature(0, $"test.�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 1, 1, 1,
                effects: new[]
                {
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            Destroy: new EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource.This)
                        ),
                        new []
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage()
                                {
                                    Choice =testChoice
                                }
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                PlayerList = new[] { testGameMaster.PlayersById[player2Id] },
                //CardDefList = Array.Empty<CardDef>(),
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult()
            {
                PlayerList = new[] { testGameMaster.PlayersById[player2Id] },
                //CardDefList = Array.Empty<CardDef>(),
                CardList = Array.Empty<Card>()
            };
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public void �^�[�����̃v���C���[���I�������()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                PlayerCondition = new PlayerCondition()
                {
                    Type = PlayerCondition.PlayerConditionType.Active
                }
            };
            var testCardDef = CardDef.Artifact(0, $"test.test", "test", "test", false,
                new[]
                {
                    // �^�[���J�n���A�J�����g�v���C���[��1�_���[�W
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)
                        ),
                        new []{
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage()
                                {
                                    Choice = testChoice,
                                    Value = 1
                                }
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���̌���
                var expected = new ChoiceResult()
                {
                    PlayerList = new[] { testGameMaster.PlayersById[player1Id] },
                    //CardDefList = Array.Empty<CardDef>(),
                    CardList = Array.Empty<Card>()
                };
                var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
                TestUtil.AssertChoiceResult(expected, actual);

                // ���o���ʂ̌���
                var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
                TestUtil.AssertChoiceResult(expected, actual2);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                PlayerList = new[] { testGameMaster.PlayersById[player2Id] },
                //CardDefList = Array.Empty<CardDef>(),
                CardList = Array.Empty<Card>()
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
            TestUtil.AssertChoiceResult(expected, actual2);
        }

        [Fact]
        public void �����_���ȑ���N���[�`���[���()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Random,
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new[] { ZoneType.OpponentField }),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature })
                },
                NumPicks = 1
            };
            var testCardDef = CardDef.Artifact(0, $"test.test", "test", "test", false);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o��
            var (goblinCard, goblinCard2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // ��U
            // ���ʃN���[�`���[���o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���̌���
                var expected = new ChoiceResult()
                {
                    CardList = new[] { goblinCard, goblinCard2 }
                };
                var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual);

                // ���o���ʂ̌���
                var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual2, 1);
            });
        }

        [Fact]
        public void ����v���C���[�������_���ȑ���N���[�`���[1��()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
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
                    ZoneCondition = new(new[] { ZoneType.OpponentField }),
                }
            };
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o��
            var (goblinCard, goblinCard2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // ��U
            // ���ʃN���[�`���[���o��
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                PlayerList = new[] { testGameMaster.PlayersById[player1Id] },
                CardList = new[] { goblinCard, goblinCard2 }
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual2, 1);
        }

        [Fact]
        public void �Ώۂ̑���N���[�`���[1��()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Choose,
                NumPicks = 1,
                CardCondition = new CardCondition()
                {
                    ZoneCondition = new(new[] { ZoneType.OpponentField }),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature })
                }
            };
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �J�[�h�̑I�������̃e�X�g
            var isCalledAskAction = false;
            ChoiceResult expected = null;
            ChoiceResult testAskCardAction(PlayerId _, ChoiceResult c, int i)
            {
                isCalledAskAction = true;
                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return c;
            }

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), testAskCardAction, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o��
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // ��U
            // ���ʃN���[�`���[���o��
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ���̌���
            expected = new ChoiceResult()
            {
                CardList = new[] { cards.goblinCard, cards.goblinCard2 }
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
            testGameMaster.ChoiceCards(testCard, testChoice, null);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public void �Ώۂ̎����N���[�`���[1��()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.Choose,
                NumPicks = 1,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.Others,
                    ZoneCondition = new(new[] { ZoneType.YouField }),
                    TypeCondition = new CardTypeCondition(new[] { CardType.Creature, })
                }
            };
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �J�[�h�̑I�������̃e�X�g
            var isCalledAskAction = false;
            ChoiceResult expected = null;
            ChoiceResult testAskCardAction(PlayerId _, ChoiceResult c, int i)
            {
                isCalledAskAction = true;

                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return c;
            }

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), testAskCardAction, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o���Ă�����ʃJ�[�h�o��
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // ���̌���
            expected = new ChoiceResult()
            {
                CardList = new[] { cards.goblinCard, cards.goblinCard2 }
            };
            var actual = testGameMaster.ChoiceCandidates(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
            testGameMaster.ChoiceCards(cards.testCard, testChoice, null);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public void �������g��I������()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.This,
                }
            };
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // �S�u�����ƌ��ʃJ�[�h�o��
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                CardList = new[] { cards.testCard }
            };
            var actual = testGameMaster.ChoiceCandidates(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
            var actual2 = testGameMaster.ChoiceCards(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual2);
        }

        [Fact]
        public void �C�x���g�\�[�X��I������()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.EventSource,
                }
            };
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { });

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // �S�u�����ƌ��ʃJ�[�h�o��
            var (goblinCard, testCard) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceResult()
            {
                CardList = new[] { goblinCard }
            };
            var actual = testGameMaster.ChoiceCandidates(testCard, testChoice,
                new EffectEventArgs(GameEvent.OnBattle, testGameMaster, SourceCard: goblinCard));
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
            var actual2 = testGameMaster.ChoiceCards(testCard, testChoice,
                new EffectEventArgs(GameEvent.OnBattle, testGameMaster, SourceCard: goblinCard));
            TestUtil.AssertChoiceResult(expected, actual2);
        }
    }
}
