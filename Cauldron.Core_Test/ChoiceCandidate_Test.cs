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
        public async Task �����_���Ȏ����N���[�`���[1��()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
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

            var testCardDef = MessageObjectExtensions.Creature(0, "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 2, 2, 1,
                effects: new[]
                {
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard(
                                    new NumValue(0),
                                    new NumValue(0),
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

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, goblinCard2, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertCollection(
                Array.Empty<PlayerId>(),
                actual.PlayerIdList);

            //TestUtil.AssertCollection(
            //    Array.Empty<CardDef>(),
            //    actual.CardDefList);

            // �ǂ��������I�΂�Ă���
            Assert.Single(actual2.CardList);
            Assert.Contains(actual2.CardList, c => new[] { goblinCard.Id, goblinCard2.Id }.Contains(c.Id));
        }

        [Fact]
        public async Task �����̃N���[�`���[���ׂ�()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
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

            var testCardDef = MessageObjectExtensions.Creature(0, "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 3, 3, 1,
                effects: new[]
                {
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                ModifyCard = new EffectActionModifyCard(
                                    new NumValue(0),
                                    new NumValue(0),
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

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, goblinCard2, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task �J�[�h�v�[�����疼�O�w��ňꖇ()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = MessageObjectExtensions.Creature(0, "�t�F�A���[", "�e�X�g�N���[�`���[", 1, 1, 1, isToken: true);

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

            var testCardDef = MessageObjectExtensions.Creature(0, "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 1, 1, 1,
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

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy }
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy }
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task �J�[�h�v�[�����疼�O�w���2��()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var fairy = MessageObjectExtensions.Creature(0, "�t�F�A���[", "�e�X�g�N���[�`���[", 1, 1, 1, isToken: true);

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

            var testCardDef = MessageObjectExtensions.Creature(0, "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 1, 1, 1,
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

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy, fairy }
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                Array.Empty<Card>(),
                new[] { fairy, fairy }
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task ����v���C���[���I�������()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                PlayerCondition = new PlayerCondition()
                {
                    Type = PlayerCondition.PlayerConditionType.Opponent,
                }
            };
            var testCardDef = MessageObjectExtensions.Creature(0, "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 1, 1, 1,
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
                                    1,
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

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult2(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task �^�[�����̃v���C���[���I�������()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                PlayerCondition = new PlayerCondition()
                {
                    Type = PlayerCondition.PlayerConditionType.Active
                }
            };
            var testCardDef = MessageObjectExtensions.Artifact(0, "test", "test", false,
                new[]
                {
                    // �^�[���J�n���A�J�����g�v���C���[��1�_���[�W
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)))
                        ),
                        new []{
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    1,
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

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���̌���
                var expected = new ChoiceCandidates(
                     new[] { player1Id },
                     Array.Empty<Card>(),
                     Array.Empty<CardDef>()
                 );
                var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
                TestUtil.AssertChoiceResult(expected, actual);

                // ���o���ʂ̌���
                var expected2 = new ChoiceResult2(
                     new[] { player1Id },
                     Array.Empty<Card>(),
                     Array.Empty<CardDef>()
                 );
                var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
                TestUtil.AssertChoiceResult(expected2, actual2);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var expected2 = new ChoiceResult2(
                new[] { player2Id },
                Array.Empty<Card>(),
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, new EffectEventArgs(GameEvent.OnBattle, testGameMaster));
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task �����_���ȑ���N���[�`���[���()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
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
            var testCardDef = MessageObjectExtensions.Artifact(0, "test", "test", false);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o��
            var (goblinCard, goblinCard2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // ��U
            // ���ʃN���[�`���[���o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���̌���
                var expected = new ChoiceCandidates(
                     Array.Empty<PlayerId>(),
                     new[] { goblinCard, goblinCard2 },
                     Array.Empty<CardDef>()
                 );
                var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual);

                // ���o���ʂ̌���
                var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
                TestUtil.AssertChoiceResult(expected, actual2, 1);
            });
        }

        [Fact]
        public async Task ����v���C���[�������_���ȑ���N���[�`���[1��()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
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
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o��
            var (goblinCard, goblinCard2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // ��U
            // ���ʃN���[�`���[���o��
            var testCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                new[] { player1Id },
                new[] { goblinCard, goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // ���o���ʂ̌���
            var actual2 = await testGameMaster.ChoiceCards(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual2, 1);
        }

        [Fact]
        public async Task �Ώۂ̑���N���[�`���[1��()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
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
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // �J�[�h�̑I�������̃e�X�g
            var isCalledAskAction = false;
            ChoiceCandidates expected = null;
            ValueTask<ChoiceResult> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                isCalledAskAction = true;
                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return ValueTask.FromResult(new ChoiceResult(
                    c.PlayerIdList,
                    c.CardList.Select(c => c.Id).ToArray(),
                    c.CardDefList.Select(c => c.Id).ToArray()
                    ));
            }

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o��
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return (goblinCard, goblinCard2);
            });

            // ��U
            // ���ʃN���[�`���[���o��
            var testCard = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ���̌���
            expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { cards.goblinCard, cards.goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
            await testGameMaster.ChoiceCards(testCard, testChoice, null);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public async Task �Ώۂ̎����N���[�`���[1��()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
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
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // �J�[�h�̑I�������̃e�X�g
            var isCalledAskAction = false;
            ChoiceCandidates expected = null;
            ValueTask<ChoiceResult> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                isCalledAskAction = true;

                Assert.Equal(1, i);
                TestUtil.AssertChoiceResult(expected, c);
                return ValueTask.FromResult(new ChoiceResult(
                    c.PlayerIdList,
                    c.CardList.Select(c => c.Id).ToArray(),
                    c.CardDefList.Select(c => c.Id).ToArray()
                    ));
            }

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��s
            // �S�u����2�̏o���Ă�����ʃJ�[�h�o��
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard2, testCard);
            });

            // ���̌���
            expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { cards.goblinCard, cards.goblinCard2 },
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
            await testGameMaster.ChoiceCards(cards.testCard, testChoice, null);
            Assert.True(isCalledAskAction);
        }

        [Fact]
        public async Task �������g��I������()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.This,
                }
            };
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��s
            // �S�u�����ƌ��ʃJ�[�h�o��
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { cards.testCard },
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
            var expected2 = new ChoiceResult2(
                Array.Empty<PlayerId>(),
                new[] { cards.testCard },
                Array.Empty<CardDef>()
            );
            var actual2 = await testGameMaster.ChoiceCards(cards.testCard, testChoice, null);
            TestUtil.AssertChoiceResult(expected2, actual2);
        }

        [Fact]
        public async Task �C�x���g�\�[�X��I������()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 1);
            goblin.NumTurnsToCanAttack = 0;

            var testChoice = new Choice()
            {
                How = Choice.ChoiceHow.All,
                CardCondition = new CardCondition()
                {
                    Context = CardCondition.CardConditionContext.EventSource,
                }
            };
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "test", 1, 1, 1);

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��s
            // �S�u�����ƌ��ʃJ�[�h�o��
            var (goblinCard, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, testCard);
            });

            // ���̌���
            var expected = new ChoiceCandidates(
                Array.Empty<PlayerId>(),
                new[] { goblinCard },
                Array.Empty<CardDef>()
            );
            var actual = await testGameMaster.ChoiceCandidates(testCard, testChoice,
                new EffectEventArgs(GameEvent.OnBattle, testGameMaster, SourceCard: goblinCard));
            TestUtil.AssertChoiceResult(expected, actual);

            // �J�[�h�I�������̃e�X�g
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
