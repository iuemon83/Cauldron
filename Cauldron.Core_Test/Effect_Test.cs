using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    public class Effect_Test
    {
        [Fact]
        public async Task �������ɃN���[�`���[����̏o���\��()
        {
            var slime = TestCards.slime;
            slime.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(TestCards.CardsetName, new[] { slime }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(slime.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(slime.Id, 40));

            await testGameMaster.Start(player1Id);
            await TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�2�̏o�Ă��āA����ԃX���C��
            Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task �������ɃN���[�`���[��2�̏o���\��()
        {
            var slime = MessageObjectExtensions.Creature(0, "�X���C��", "�e�X�g�N���[�`���[", 1, 1, 1,
                effects: new[]
                {
                    // �������A�X���C����2�̏���
                    new CardEffect(
                        MessageObjectExtensions.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                AddCard = new(
                                    new ZoneValue(new[]{ ZonePrettyName.YouField }),
                                    new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            NameCondition = new(
                                                new TextValue($"Test.�X���C��"),
                                                TextCondition.ConditionCompare.Equality
                                            ),
                                            ZoneCondition = new(new(new[]{ ZonePrettyName.CardPool })),
                                        },
                                        NumPicks=2
                                    }
                                )
                            }
                        }
                    )
                }
                );

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { slime }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(slime.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(slime.Id, 40));

            await testGameMaster.Start(player1Id);
            await TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�3�̏o�Ă��āA����ԃX���C��
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task ���S���ɑ���v���C���[��1�_���[�W()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var mouseDef = TestCards.mouse;
            mouseDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { mouseDef, goblinDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(mouseDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(mouseDef.Id, 40));

            await testGameMaster.Start(player1Id);

            var testCard = await TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pid, mouseDef.Id);
            });

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var beforeHp = g.ActivePlayer.CurrentHp;

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid,
                    goblin.Id,
                    testCard.Id
                    ));

                // �U�����̓S�u��������̂���
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
                Assert.Equal(goblin.Id, g.ActivePlayer.Field.AllCards[0].Id);

                // �h�䑤�̓t�B�[���h����
                Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

                // �U���v���C���[�Ɉ�_�_���[�W
                Assert.Equal(beforeHp - 1, g.ActivePlayer.CurrentHp);
            });
        }

        [Fact]
        public async Task �j�󎞂Ƀt�F�A���[�P������D�ɉ�����()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var fairyDef = TestCards.fairy;
            var waterFairyDef = TestCards.waterFairy;
            waterFairyDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(TestCards.CardsetName, new[] { fairyDef, waterFairyDef, goblinDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(waterFairyDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(waterFairyDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            // �E�H�[�^�[�t�F�A���[���o��
            var waterFairy = await TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pid, waterFairyDef.Id);
            });

            var beforeHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Select(c => c.Id).ToArray();

            // ��U
            // �S�u�����o���ăE�H�[�^�[�t�F�A���[�ɍU�����Ĕj�󂷂�
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                await TestUtil.AssertGameAction(() => testGameMaster.AttackToCreature(player2Id,
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
        public async Task �������Ɏ����̃N���[�`���[�������_���ň�̂��C��()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var testCreatureDef = TestCards.whiteGeneral;
            testCreatureDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // �S�u�����o���Ă�����ʃN���[�`���[���o��
            await testGameMaster.Start(player1Id);

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

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
        public async Task �������Ɏ����̃N���[�`���[���ׂĂ��C��()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var testCreatureDef = TestCards.commander;
            testCreatureDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // �S�u�����Q�̏o���Ă�����ʃN���[�`���[���o��
            await testGameMaster.Start(player1Id);
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                //testGameMaster.StartTurn();
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

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
        public async Task �^�[���I�����Ƀ����_���ȑ���N���[�`���[1�̂�1�_���[�W_���̌ケ�̃J�[�h��j��()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.devil;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            // �S�u�����Q�̏o��
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard, goblinCard2 };
            });

            // ��U
            // �e�X�g�J�[�h���o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

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
                cards.goblinCard.Toughness == goblin.Toughness - 1
                || cards.goblinCard2.Toughness == goblin.Toughness - 1);
        }

        [Fact]
        public async Task ���肩�����_���ȑ���N���[�`���[��̂�2�_���[�W()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.shock;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            // �S�u�����Q�̏o��
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard };
            });

            // ��U
            // �e�X�g�J�[�h���o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �t�B�[���h�ɂ�0��
                Assert.Equal(0, g.ActivePlayer.Field.AllCards.Count);

                // �S�u����������v���C���[�Ƀ_���[�W
                Assert.True(
                    cards.goblinCard.Toughness == goblin.Toughness - 2
                    || testGameMaster.PlayersById[player1Id].CurrentHp == testGameMaster.RuleBook.MaxPlayerHp - 2);
            });
        }

        [Fact]
        public async Task �Ώۂ̎����N���[�`���[���C��()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.buf;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // �J�[�h�̑I�������̃e�X�g
            static ValueTask<ChoiceResult> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                return ValueTask.FromResult(new ChoiceResult(
                    Array.Empty<PlayerId>(),
                    c.CardList.Select(c => c.Id).Take(1).ToArray(),
                    Array.Empty<CardDefId>()
                ));
            }

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            // �S�u�����Q�̏o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.True(goblinCard.PowerBuff == 2 && goblinCard.ToughnessBuff == 2);
            });
        }

        [Fact]
        public async Task �g�p���ɂ��ׂĂ̎����N���[�`���[���C���������N���[�`���[�̃v���C���ɏC��()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = TestCards.flag;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ��ɏ�ɂ����S�u����2�̂��C�������
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);

                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                // ���Ƃɏ�ɏo���S�u�����������C�������
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);
                Assert.True(goblinCard3.PowerBuff == 1 && goblinCard3.ToughnessBuff == 0);
            });
        }

        [Fact]
        public async Task �����N���[�`���[�̔�_���[�W���y������()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = TestCards.shield;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            var goblinCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblinCard;
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
            });
        }

        [Fact]
        public async Task �����v���C���[�̔�_���[�W���y������()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = TestCards.wall;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var beforeHp = g.PlayersById[player1Id].CurrentHp;
                await testGameMaster.AttackToPlayer(pId, goblinCard.Id, player1Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(beforeHp - 1, g.PlayersById[player1Id].CurrentHp);
            });
        }

        [Fact]
        public async Task �J�[�h��1������()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = TestCards.hikari;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 1, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                Assert.Equal(beforeNumOfHands + 1, afterNumOfHands);
            });
        }

        [Fact]
        public async Task �J�[�h�����ׂĎ̂Ăē��������h���[����()
        {
            var testCardDef = TestCards.unmei;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                var beforeHandIdList = g.ActivePlayer.Hands.AllCards.Select(c => c.Id).ToArray();
                Assert.True(beforeNumOfHands != 0);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �̂Ă������Ɠ��������h���[���Ă�
                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                Assert.Equal(beforeNumOfHands, afterNumOfHands);
                foreach (var handCard in g.ActivePlayer.Hands.AllCards)
                {
                    Assert.True(beforeHandIdList.All(beforeHandId => beforeHandId != handCard.Id));
                }
            });
        }

        [Fact]
        public async Task ���̃J�[�h����D����̂Ă�ꂽ��1������()
        {
            var testCardDef = TestCards.hikari;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                await TestUtil.AssertGameAction(() => g.Discard(pId, new[] { g.ActivePlayer.Hands.AllCards[0].Id }));

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 1, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                // 1��������1��������
                Assert.Equal(beforeNumOfHands, afterNumOfHands);
            });
        }

        [Fact]
        public async Task �������Ɏ����v���C���[��2��()
        {
            var testCardDef = TestCards.healingAngel;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ���O�Ƀ_���[�W
                g.PlayersById[pId].Damage(5);

                var beforeHp = g.PlayersById[pId].CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 2�񕜂��Ă���
                Assert.Equal(beforeHp + 2, g.PlayersById[pId].CurrentHp);
            });
        }

        [Fact]
        public async Task �������Ɏ��R�N���[�`���[�Ɍ��ʂ�t�^����()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2);

            var testCardDef = TestCards.atena;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);

            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �S�u�����œG���U��
                // �U���������̓_���[�W���󂯂Ȃ�
                await g.AttackToCreature(pId, goblin3.Id, goblin1.Id);
                Assert.Equal(0, goblin1.Toughness);
                Assert.Equal(goblinDef.Toughness, goblin3.Toughness);

                // �e�X�g�J�[�h�œG���U��
                // �����ɂ͌��ʂ��t�^����Ȃ��̂ŁA�_���[�W���󂯂�
                await g.AttackToCreature(pId, testCard.Id, goblin2.Id);
                Assert.Equal(0, goblin2.Toughness);
                Assert.Equal(testCardDef.Toughness - goblin2.Power, testCard.Toughness);
            });
        }

        [Fact]
        public async Task ��D�����ׂĎ̂Ă��̖�����������������()
        {
            var testCardDef = TestCards.tenyoku;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ����1�������邩��
                var numHands = g.PlayersById[pId].Hands.AllCards.Count;
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(testCard.BasePower + numHands, testCard.Power);
                Assert.Equal(testCard.BaseToughness + numHands, testCard.Toughness);
            });
        }

        [Fact]
        public async Task ����ׂɎ�D���P���̂ĂĂ��̃J�[�h�̃R�X�g�����C�t���񕜂���()
        {
            var testCardDef = MessageObjectExtensions.Sorcery(0, "test", "", new[] {
                new CardEffect(
                    MessageObjectExtensions.Spell,
                    new[]{
                        new EffectAction(MoveCard: new(
                            new Choice()
                            {
                                How = Choice.ChoiceHow.Random,
                                NumPicks = 1,
                                CardCondition = new()
                                {
                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouHand }))
                                }
                            },
                            ZonePrettyName.YouCemetery,
                            "moveCard"
                            )),
                        new EffectAction(ModifyPlayer: new(
                            new Choice()
                            {
                                How = Choice.ChoiceHow.All,
                                PlayerCondition = new(
                                    Type: PlayerCondition.PlayerConditionType.You)
                            },
                            new PlayerModifier(
                                Hp: new(
                                    NumValueModifier.ValueModifierOperator.Add,
                                    new NumValue(NumValueCalculator: new(
                                        NumValueCalculator.ValueType.CardCost,
                                        new Choice()
                                        {
                                            How = Choice.ChoiceHow.All,
                                            CardCondition = new()
                                            {
                                                ActionContext = new(ActionContextCardsOfMoveCard: new(
                                                    "moveCard",
                                                    ActionContextCardsOfMoveCard.ValueType.Moved
                                                    ))
                                            }
                                        }
                                        ))))
                            )),
                    })
            }); ;

            var testRulebook = new RuleBook(
                InitialPlayerHp: 10, MaxPlayerHp: 20, MinPlayerHp: 0, MaxNumDeckCards: 40, MinNumDeckCards: 40, InitialNumHands: 5,
                MaxNumHands: 10, InitialMp: 1, MaxLimitMp: 10, MinMp: 1, MpByStep: 1, MaxNumFieldCars: 5, DefaultNumTurnsToCanAttack: 0,
                DefaultNumAttacksLimitInTurn: 1);
            var testCardFactory = new CardRepository(testRulebook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforenumHands = g.PlayersById[pId].Hands.AllCards.Count;
                var beforeHp = g.PlayersById[pId].CurrentHp;
                // ����1�������邩��
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforenumHands - 1, g.PlayersById[pId].Hands.AllCards.Count);
                Assert.Equal(beforeHp + testCardDef.Cost, g.PlayersById[pId].CurrentHp);
            });
        }

        [Fact]
        public async Task �N���[�`���[����̔j�󂵂ē�����ɃR�s�[���o��()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2);

            var testCardDef = TestCards.ulz;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick)
                ));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            var goblin = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(1, g.PlayersById[player1Id].Field.Count);
                Assert.Equal(goblin.Id, g.PlayersById[player1Id].Field.AllCards[0].Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(1, g.PlayersById[player1Id].Field.Count);
                Assert.NotEqual(goblin.Id, g.PlayersById[player1Id].Field.AllCards[0].Id);
            });
        }

        //[Fact]
        //public void �����̃N���[�`���[�̍U���_���[�W�𑝉�����()
        //{
        //    var goblin = MessageObjectExtensions.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 3);
        //    goblin.TurnCountToCanAttack = 0;

        //    var testCardDef = TestCards.holyKnight;
        //    testCardDef.BaseCost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { goblin, testCardDef });

        //    var testGameMaster = new GameMaster(TestUtil.TestRuleBook, testCardFactory, new TestLogger(), null, (_,_) => {});

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

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
