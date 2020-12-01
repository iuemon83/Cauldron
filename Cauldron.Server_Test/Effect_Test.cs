using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    public class Effect_Test
    {
        [Fact]
        public void �������ɃN���[�`���[����̏o���\��()
        {
            var slime = CardDef.CreatureCard(0, $"test.�X���C��", "�X���C��", "�e�X�g�N���[�`���[", 1, 1,
                effects: new[]
                {
                    // �������A�X���C������̏���
                    new CardEffect()
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
                                AddCard = new EffectActionAddCard()
                                {
                                    ZoneToAddCard = ZoneType.YouField,
                                    Choice = new Choice()
                                    {
                                        NewCardCondition = new CardCondition()
                                        {
                                            NameCondition = new TextCondition()
                                            {
                                                Value = $"test.�X���C��",
                                                Compare = TextCondition.ConditionCompare.Equality
                                            },
                                            ZoneCondition = ZoneType.CardPool,
                                        },
                                        NumPicks=1
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { slime });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(slime.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(slime.Id, 40));

            testGameMaster.Start(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�2�̏o�Ă��āA����ԃX���C��
            Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public void �������ɃN���[�`���[��2�̏o���\��()
        {
            var slime = CardDef.CreatureCard(0, $"test.�X���C��", "�X���C��", "�e�X�g�N���[�`���[", 1, 1,
                effects: new[]
                {
                    // �������A�X���C����2�̏���
                    new CardEffect()
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
                                AddCard = new EffectActionAddCard()
                                {
                                    ZoneToAddCard = ZoneType.YouField,
                                    Choice = new Choice()
                                    {
                                        NewCardCondition = new CardCondition()
                                        {
                                            NameCondition = new TextCondition()
                                            {
                                                Value = $"test.�X���C��",
                                                Compare = TextCondition.ConditionCompare.Equality
                                            },
                                            ZoneCondition = ZoneType.CardPool,
                                        },
                                        NumPicks=2
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { slime });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(slime.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(slime.Id, 40));

            testGameMaster.Start(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�3�̏o�Ă��āA����ԃX���C��
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public void ���S���ɑ���v���C���[��1�_���[�W()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var mouse = CardDef.CreatureCard(0, $"test.�l�Y�~", "�l�Y�~", "�e�X�g�N���[�`���[", 1, 1,
                effects: new[]
                {
                    // ���S���A����v���C���[��1�_���[�W
                    new CardEffect(){
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent(){
                                Source = EffectTimingDestroyEvent.EventSource.This
                            }
                        },
                        Actions =new []
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage()
                                {
                                    Value = 1,
                                    Choice = new Choice()
                                    {
                                        PlayerCondition = new PlayerCondition()
                                        {
                                            Type = PlayerCondition.PlayerConditionType.NotOwner,
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { mouse, goblin });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(mouse.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(mouse.Id, 40));

            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));
            TestUtil.AssertPhase(() => testGameMaster.EndTurn(player1Id));

            testGameMaster.StartTurn(player2Id);
            var newHandCard = testGameMaster.GenerateNewCard(goblin.Id, testGameMaster.ActivePlayer.Id);
            testGameMaster.AddHand(testGameMaster.ActivePlayer, newHandCard);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player2Id, newHandCard.Id));
            TestUtil.AssertPhase(() => testGameMaster.AttackToCreature(player2Id,
                newHandCard.Id,
                testGameMaster.PlayersById[player1Id].Field.AllCards[0].Id
                ));

            // �U�����̓S�u��������̂���
            Assert.Equal(1, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(goblin.Id, card.CardDefId);
            }

            // �h�䑤�̓t�B�[���h����
            Assert.Equal(0, testGameMaster.PlayersById[player1Id].Field.AllCards.Count);

            // �U���v���C���[�Ɉ�_�_���[�W
            Assert.Equal(testGameMaster.RuleBook.MaxPlayerHp - 1, testGameMaster.ActivePlayer.Hp);
        }

        [Fact]
        public void �j�󎞂Ƀt�F�A���[�P������D�ɉ�����()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var fairy = CardDef.CreatureCard(0, $"test.�t�F�A���[", "�t�F�A���[", "�e�X�g�N���[�`���[", 1, 1, isToken: true);
            var waterFairy = CardDef.CreatureCard(0, $"test.�E�H�[�^�[�t�F�A���[", "�E�H�[�^�[�t�F�A���[", "�e�X�g�N���[�`���[", 1, 1,

                effects: new[]
                {
                    // �j�󎞁A�t�F�A���[�P������D�ɉ�����
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent()
                            {
                                Source = EffectTimingDestroyEvent.EventSource.This
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    ZoneToAddCard = ZoneType.YouHand,
                                    Choice = new Choice()
                                    {
                                        NewCardCondition = new CardCondition()
                                        {
                                            ZoneCondition = ZoneType.CardPool,
                                            NameCondition = new TextCondition()
                                            {
                                                Value = fairy.FullName,
                                                Compare = TextCondition.ConditionCompare.Equality
                                            }
                                        },
                                        NumPicks=1
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { fairy, waterFairy, goblin });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(waterFairy.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(waterFairy.Id, 40));

            // �E�H�[�^�[�t�F�A���[���o��
            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));
            TestUtil.AssertPhase(() => testGameMaster.EndTurn(player1Id));

            var beforeHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Select(c => c.Id).ToArray();

            // �S�u�����o���ăE�H�[�^�[�t�F�A���[�ɍU�����Ĕj�󂷂�
            testGameMaster.StartTurn(player2Id);
            var newHandCard = testGameMaster.GenerateNewCard(goblin.Id, testGameMaster.ActivePlayer.Id);
            testGameMaster.AddHand(testGameMaster.ActivePlayer, newHandCard);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player2Id, newHandCard.Id));
            TestUtil.AssertPhase(() => testGameMaster.AttackToCreature(player2Id,
                newHandCard.Id,
                testGameMaster.PlayersById[player1Id].Field.AllCards[0].Id
                ));

            // �U�����̓S�u��������̂���
            Assert.Equal(1, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(goblin.Id, card.CardDefId);
            }

            // �h�䑤�̓t�B�[���h����
            Assert.Equal(0, testGameMaster.PlayersById[player1Id].Field.AllCards.Count);

            // �h�䑤�̎�D�Ƀt�F�A���[���ꖇ������
            var addedHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
            Assert.Single(addedHands);
            Assert.Equal(fairy.Id, addedHands[0].CardDefId);
        }

        [Fact]
        public void �������Ɏ����̃N���[�`���[�������_���ň�̂��C��()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var testCreature = CardDef.CreatureCard(0, $"test.�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", "�e�X�g�N���[�`���[", 2, 2,
                effects: new[]
                {
                    // �������A�����̃N���[�`���[�������_���ň�̂�+2/+1
                    new CardEffect()
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
                                    Choice = new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.Others,
                                            ZoneCondition = ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new[]{ CardType.Creature },
                                            },
                                        },
                                        NumPicks = 1,
                                        How = Choice.ChoiceHow.Random
                                    },
                                    Power = 2,
                                    Toughness=1
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCreature });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreature.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreature.Id, 40));

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            var goblinCard = testGameMaster.GenerateNewCard(goblin.Id, player1Id);
            testGameMaster.AddHand(testGameMaster.ActivePlayer, goblinCard);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, goblinCard.Id));

            var testCreatureCard = testGameMaster.ActivePlayer.Hands.AllCards[0];
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testCreatureCard.Id));

            // �U������2��
            Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Contains(card.CardDefId, new[] { goblin.Id, testCreature.Id });
            }

            // power+2 �̃o�t����Ă�
            Assert.Equal(2, goblinCard.PowerBuff);
            Assert.Equal(1, goblinCard.ToughnessBuff);

            // �����������̓o�t����Ȃ�
            Assert.Equal(0, testCreatureCard.PowerBuff);
            Assert.Equal(0, testCreatureCard.ToughnessBuff);
        }

        [Fact]
        public void �������Ɏ����̃N���[�`���[���ׂĂ��C��()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var testCreature = CardDef.CreatureCard(0, $"test.�Z�[�W�R�}���_�[", "�Z�[�W�R�}���_�[", "�e�X�g�N���[�`���[", 3, 3,
                effects: new[]
                {
                    // �������A�����̃N���[�`���[���ׂĂ�+1/+2
                    new CardEffect()
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
                                    Choice = new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.Others,
                                            ZoneCondition= ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value =new[]{ CardType.Creature },
                                            }
                                        },
                                        How = Choice.ChoiceHow.All,
                                    },
                                    Power=1,
                                    Toughness=2
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCreature });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreature.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreature.Id, 40));

            // �S�u�����Q�̏o���Ă�����ʃN���[�`���[���o��
            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            var goblinCard = testGameMaster.GenerateNewCard(goblin.Id, player1Id);
            var goblinCard2 = testGameMaster.GenerateNewCard(goblin.Id, player1Id);
            testGameMaster.AddHand(testGameMaster.ActivePlayer, goblinCard);
            testGameMaster.AddHand(testGameMaster.ActivePlayer, goblinCard2);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, goblinCard.Id));
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, goblinCard2.Id));

            var testCreatureCard = testGameMaster.ActivePlayer.Hands.AllCards[0];
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testCreatureCard.Id));

            // �U������3��
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Contains(card.CardDefId, new[] { goblin.Id, testCreature.Id });
            }

            // �S�u������2�̂Ƃ�+1/+2 ����Ă���
            Assert.Equal(1, goblinCard.PowerBuff);
            Assert.Equal(2, goblinCard.ToughnessBuff);
            Assert.Equal(1, goblinCard2.PowerBuff);
            Assert.Equal(2, goblinCard2.ToughnessBuff);

            // �����������̓o�t����Ȃ�
            Assert.Equal(0, testCreatureCard.PowerBuff);
            Assert.Equal(0, testCreatureCard.ToughnessBuff);
        }

        [Fact]
        public void �^�[���I�����Ƀ����_���ȑ���N���[�`���[1�̂�1�_���[�W_���̌ケ�̃J�[�h��j��()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                effects: new[]
                {
                    // �^�[���I�����A�����_���ȑ���N���[�`���[��̂�1�_���[�W�B���̌ケ�̃J�[�h��j��
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            EndTurn = new EffectTimingEndTurnEvent()
                            {
                                Source = EffectTimingEndTurnEvent.EventSource.Both,
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage()
                                {
                                    Value=1,
                                    Choice = new Choice()
                                    {
                                        How = Choice.ChoiceHow.Random,
                                        CardCondition= new CardCondition()
                                        {
                                            ZoneCondition = ZoneType.OpponentField,
                                            TypeCondition=new CardTypeCondition()
                                            {
                                                Value = new[]{CardType.Creature}
                                            }
                                        },
                                        NumPicks=1
                                    }
                                }
                            },
                            new EffectAction()
                            {
                                DestroyCard = new EffectActionDestroyCard()
                                {
                                    Choice = new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.This,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            // �S�u�����Q�̏o��
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard, goblinCard2 };
            });

            // ��U
            // �e�X�g�J�[�h���o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �t�B�[���h�ɂ�1��
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);

                // �܂��S�u�����̓m�[�_��
                Assert.Equal(2, cards.goblinCard.Toughness);
                Assert.Equal(2, cards.goblinCard2.Toughness);
            });

            // �j�󂳂��̂Ńt�B�[���h�ɂ�0��
            Assert.Equal(0, testGameMaster.PlayersById[player2Id].Field.AllCards.Count);

            // �ǂ��炩�̃S�u������1�_���[�W
            Assert.True(
                cards.goblinCard.Toughness == goblin.BaseToughness - 1
                || cards.goblinCard2.Toughness == goblin.BaseToughness - 1);
        }

        [Fact]
        public void ���肩�����_���ȑ���N���[�`���[��̂�2�_���[�W()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                effects: new[]
                {
                    // �g�p���A���肩�����_���ȑ���N���[�`���[��̂�2�_���[�W
                    new CardEffect()
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
                                Damage=  new EffectActionDamage()
                                {
                                    Choice = new Choice()
                                    {
                                        PlayerCondition = new PlayerCondition()
                                        {
                                            Type = PlayerCondition.PlayerConditionType.NotOwner,
                                        },
                                        NumPicks= 1,
                                        How= Choice.ChoiceHow.Random,
                                        CardCondition = new CardCondition()
                                        {
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new []{CardType.Creature}
                                            },
                                            ZoneCondition = ZoneType.OpponentField,
                                        }
                                    },
                                    Value=2
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            // �S�u�����Q�̏o��
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard };
            });

            // ��U
            // �e�X�g�J�[�h���o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �t�B�[���h�ɂ�1��
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);

                // �S�u����������v���C���[�Ƀ_���[�W
                Assert.True(
                    cards.goblinCard.Toughness == goblin.BaseToughness - 2
                    || testGameMaster.PlayersById[player1Id].Hp == testGameMaster.RuleBook.MaxPlayerHp - 2);
            });
        }

        [Fact]
        public void �Ώۂ̎����N���[�`���[���C��()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                effects: new[]
                {
                    // �g�p���A�Ώۂ̎����N���[�`���[��+2/+2
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source= EffectTimingPlayEvent.EventSource.This
                            }
                        },
                        Actions= new[]
                        {
                            new EffectAction(){
                                ModifyCard=new EffectActionModifyCard()
                                {
                                    Power=2,
                                    Toughness=2,
                                    Choice =new Choice()
                                    {
                                        How= Choice.ChoiceHow.Choose,
                                        NumPicks=1,
                                        CardCondition=new CardCondition()
                                        {
                                            ZoneCondition= ZoneType.YouField,
                                            TypeCondition =new CardTypeCondition()
                                            {
                                                Value= new[]{CardType.Creature, }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �J�[�h�̑I�������̃e�X�g
            static ChoiceResult testAskCardAction(Guid _, ChoiceResult c, int i)
            {
                return new ChoiceResult()
                {
                    CardList = c.CardList.Take(1).ToArray()
                };
            }

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), testAskCardAction);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            // �S�u�����Q�̏o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.True(goblinCard.PowerBuff == 2 && goblinCard.ToughnessBuff == 2);
            });
        }

        [Fact]
        public void �g�p���ɂ��ׂĂ̎����N���[�`���[���C���������N���[�`���[�̃v���C���ɏC��()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                effects: new[]
                {
                    // �g�p���A���ׂĂ̎����N���[�`���[��+1/+0
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.This,
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction(){
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Power = 1,
                                    Toughness = 0,
                                    Choice = new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new CardCondition()
                                        {
                                            ZoneCondition = ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new[]{ CardType.Creature }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // �����N���[�`���[�̃v���C��+1/+0
                    new CardEffect()
                    {
                        Timing = new EffectTiming()
                        {
                            Play = new EffectTimingPlayEvent()
                            {
                                Source = EffectTimingPlayEvent.EventSource.Other,
                            }
                        },
                        Actions = new[]
                        {
                            new EffectAction(){
                                ModifyCard = new EffectActionModifyCard()
                                {
                                    Power = 1,
                                    Toughness = 0,
                                    Choice = new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new CardCondition()
                                        {
                                            ZoneCondition = ZoneType.YouField,
                                            TypeCondition = new CardTypeCondition()
                                            {
                                                Value = new[]{ CardType.Creature }
                                            },
                                            Context = CardCondition.CardConditionContext.EventSource,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                );

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ��ɏ�ɂ����S�u����2�̂��C�������
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);

                var goblinCard3 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                // ���Ƃɏ�ɏo���S�u�����������C�������
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);
                Assert.True(goblinCard3.PowerBuff == 1 && goblinCard3.ToughnessBuff == 0);
            });
        }

        [Fact]
        public void �����N���[�`���[�̔�_���[�W���y������()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 2, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = TestCards.shield;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var goblinCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblinCard;
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
            });
        }

        [Fact]
        public void �����v���C���[�̔�_���[�W���y������()
        {
            var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 2, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = TestCards.wall;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                testGameMaster.AttackToPlayer(pId, goblinCard.Id, player1Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(g.PlayersById[player1Id].MaxHp - 1, g.PlayersById[player1Id].Hp);
            });
        }

        //[Fact]
        //public void �����̃N���[�`���[�̍U���_���[�W�𑝉�����()
        //{
        //    var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 3);
        //    goblin.TurnCountToCanAttack = 0;

        //    var testCardDef = TestCards.holyKnight;
        //    testCardDef.BaseCost = 0;

        //    var testCardFactory = new CardFactory();
        //    testCardFactory.SetCardPool(new[] { goblin, testCardDef });

        //    var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

        //    testGameMaster.Start(player1Id);

        //    // ��U
        //    var goblinCard = TestUtil.Turn(testGameMaster, (g, pId) =>
        //    {
        //        return TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
        //    });

        //    // ��U
        //    TestUtil.Turn(testGameMaster, (g, pId) =>
        //    {
        //        var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
        //        TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

        //        // �N���[�`���[�֍U��
        //        g.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);
        //        Assert.Equal(goblinCard.BaseToughness, goblinCard.Toughness);

        //        // �v���C���[�֍U��
        //        g.AttackToPlayer(pId, goblinCard2.Id, player1Id);
        //        Assert.Equal(g.PlayersById[player1Id].MaxHp, g.PlayersById[player1Id].Hp);
        //    });
        //}
    }
}
