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
            var slime = TestCards.slime;
            slime.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { slime });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(slime.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(slime.Id, 40));

            testGameMaster.Start(player1Id);
            TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

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
            var slime = CardDef.Creature(0, $"test.�X���C��", "�X���C��", "�e�X�g�N���[�`���[", 1, 1, 1,
                effects: new[]
                {
                    // �������A�X���C����2�̏���
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField,
                            Play: new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.This)
                        ),
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new EffectActionAddCard()
                                {
                                    ZoneToAddCard = ZoneType.YouField,
                                    Choice = new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            NameCondition = new(
                                                $"test.�X���C��",
                                                TextCondition.ConditionCompare.Equality
                                            ),
                                            ZoneCondition = new(new[]{ ZoneType.CardPool }),
                                        },
                                        NumPicks=2
                                    }
                                }
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { slime });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(slime.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(slime.Id, 40));

            testGameMaster.Start(player1Id);
            TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

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
            var goblinDef = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var mouseDef = TestCards.mouse;
            mouseDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { mouseDef, goblinDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(mouseDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(mouseDef.Id, 40));

            testGameMaster.Start(player1Id);

            var testCard = TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pid, mouseDef.Id);
            });

            TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                var goblin = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                TestUtil.AssertGameAction(() => g.AttackToCreature(pid,
                    goblin.Id,
                    testCard.Id
                    ));

                // �U�����̓S�u��������̂���
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
                Assert.Equal(goblin.Id, g.ActivePlayer.Field.AllCards[0].Id);

                // �h�䑤�̓t�B�[���h����
                Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

                // �U���v���C���[�Ɉ�_�_���[�W
                Assert.Equal(g.RuleBook.MaxPlayerHp - 1, g.ActivePlayer.CurrentHp);
            });

            //testGameMaster.StartTurn(player1Id);
            //TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));
            //TestUtil.AssertPhase(() => testGameMaster.EndTurn(player1Id));

            //testGameMaster.StartTurn(player2Id);
            //var newHandCard = testGameMaster.GenerateNewCard(goblinDef.Id, testGameMaster.ActivePlayer.Id);
            //testGameMaster.AddHand(testGameMaster.ActivePlayer, newHandCard);
            //TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player2Id, newHandCard.Id));
            //TestUtil.AssertPhase(() => testGameMaster.AttackToCreature(player2Id,
            //    newHandCard.Id,
            //    testGameMaster.PlayersById[player1Id].Field.AllCards[0].Id
            //    ));

            //// �U�����̓S�u��������̂���
            //Assert.Equal(1, testGameMaster.ActivePlayer.Field.AllCards.Count);
            //foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            //{
            //    Assert.Equal(goblinDef.Id, card.CardDefId);
            //}

            //// �h�䑤�̓t�B�[���h����
            //Assert.Equal(0, testGameMaster.PlayersById[player1Id].Field.AllCards.Count);

            //// �U���v���C���[�Ɉ�_�_���[�W
            //Assert.Equal(testGameMaster.RuleBook.MaxPlayerHp - 1, testGameMaster.ActivePlayer.Hp);
        }

        [Fact]
        public void �j�󎞂Ƀt�F�A���[�P������D�ɉ�����()
        {
            var goblinDef = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var fairyDef = TestCards.fairy;
            var waterFairyDef = TestCards.waterFairy;
            waterFairyDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { fairyDef, waterFairyDef, goblinDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(waterFairyDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(waterFairyDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            // �E�H�[�^�[�t�F�A���[���o��
            var waterFairy = TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pid, waterFairyDef.Id);
            });

            var beforeHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Select(c => c.Id).ToArray();

            // ��U
            // �S�u�����o���ăE�H�[�^�[�t�F�A���[�ɍU�����Ĕj�󂷂�
            TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                var goblin = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                TestUtil.AssertGameAction(() => testGameMaster.AttackToCreature(player2Id,
                    goblin.Id,
                    waterFairy.Id
                    ));

                // �U�����̓S�u��������̂���
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
                foreach (var card in g.ActivePlayer.Field.AllCards)
                {
                    Assert.Equal(goblinDef.Id, card.CardDefId);
                }

                // �h�䑤�̓t�B�[���h����
                Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

                // �h�䑤�̎�D�Ƀt�F�A���[���ꖇ������
                var addedHands = g.PlayersById[player1Id].Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
                Assert.Single(addedHands);
                Assert.Equal(fairyDef.Id, addedHands[0].CardDefId);
            });
        }

        [Fact]
        public void �������Ɏ����̃N���[�`���[�������_���ň�̂��C��()
        {
            var goblinDef = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var testCreatureDef = TestCards.whiteGeneral;
            testCreatureDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblinDef, testCreatureDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            testGameMaster.Start(player1Id);

            TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

                // �U������2��
                Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
                foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
                {
                    Assert.Contains(card.CardDefId, new[] { goblinDef.Id, testCreatureDef.Id });
                }

                // power+2 �̃o�t����Ă�
                Assert.Equal(2, goblinCard.PowerBuff);
                Assert.Equal(0, goblinCard.ToughnessBuff);

                // �����������̓o�t����Ȃ�
                Assert.Equal(0, testCreatureCard.PowerBuff);
                Assert.Equal(0, testCreatureCard.ToughnessBuff);
            });
        }

        [Fact]
        public void �������Ɏ����̃N���[�`���[���ׂĂ��C��()
        {
            var goblinDef = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var testCreatureDef = TestCards.commander;
            testCreatureDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblinDef, testCreatureDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // �S�u�����Q�̏o���Ă�����ʃN���[�`���[���o��
            testGameMaster.Start(player1Id);
            TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                //testGameMaster.StartTurn();
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

                // �U������3��
                Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
                foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
                {
                    Assert.Contains(card.CardDefId, new[] { goblinDef.Id, testCreatureDef.Id });
                }

                // �S�u������2�̂Ƃ�+1/+2 ����Ă���
                Assert.Equal(1, goblinCard.PowerBuff);
                Assert.Equal(2, goblinCard.ToughnessBuff);
                Assert.Equal(1, goblinCard2.PowerBuff);
                Assert.Equal(2, goblinCard2.ToughnessBuff);

                // �����������̓o�t����Ȃ�
                Assert.Equal(0, testCreatureCard.PowerBuff);
                Assert.Equal(0, testCreatureCard.ToughnessBuff);
            });
        }

        [Fact]
        public void �^�[���I�����Ƀ����_���ȑ���N���[�`���[1�̂�1�_���[�W_���̌ケ�̃J�[�h��j��()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.devil;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

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
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.shock;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

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

                // �t�B�[���h�ɂ�0��
                Assert.Equal(0, g.ActivePlayer.Field.AllCards.Count);

                // �S�u����������v���C���[�Ƀ_���[�W
                Assert.True(
                    cards.goblinCard.Toughness == goblin.BaseToughness - 2
                    || testGameMaster.PlayersById[player1Id].CurrentHp == testGameMaster.RuleBook.MaxPlayerHp - 2);
            });
        }

        [Fact]
        public void �Ώۂ̎����N���[�`���[���C��()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.buf;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // �J�[�h�̑I�������̃e�X�g
            static ChoiceResult testAskCardAction(PlayerId _, ChoiceResult c, int i)
            {
                return new ChoiceResult()
                {
                    CardList = c.CardList.Take(1).ToArray()
                };
            }

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), testAskCardAction, (_, _) => { }));

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
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.flag;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

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
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = TestCards.shield;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

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
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = TestCards.wall;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

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
                Assert.Equal(g.PlayersById[player1Id].MaxHp - 1, g.PlayersById[player1Id].CurrentHp);
            });
        }

        [Fact]
        public void �J�[�h��1������()
        {
            var goblin = CardDef.Creature(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = TestCards.hikari;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 1, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                Assert.Equal(beforeNumOfHands + 1, afterNumOfHands);
            });
        }

        [Fact]
        public void ���̃J�[�h����D����̂Ă�ꂽ��1������()
        {
            var testCardDef = TestCards.hikari;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                TestUtil.AssertGameAction(() => g.Discard(pId, new[] { g.ActivePlayer.Hands.AllCards[0].Id }));

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 1, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                // 1��������1��������
                Assert.Equal(beforeNumOfHands, afterNumOfHands);
            });
        }

        [Fact]
        public void �������Ɏ����v���C���[��2��()
        {
            var testCardDef = TestCards.healingAngel;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // ���O�Ƀ_���[�W
                g.PlayersById[pId].Damage(5);

                var beforeHp = g.PlayersById[pId].CurrentHp;
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 2�񕜂��Ă���
                Assert.Equal(beforeHp + 2, g.PlayersById[pId].CurrentHp);
            });
        }

        //[Fact]
        //public void �����̃N���[�`���[�̍U���_���[�W�𑝉�����()
        //{
        //    var goblin = CardDef.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 3);
        //    goblin.TurnCountToCanAttack = 0;

        //    var testCardDef = TestCards.holyKnight;
        //    testCardDef.BaseCost = 0;

        //    var testCardFactory = new CardFactory(new RuleBook());
        //    testCardFactory.SetCardPool(new[] { goblin, testCardDef });

        //    var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null, (_,_) => {});

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
