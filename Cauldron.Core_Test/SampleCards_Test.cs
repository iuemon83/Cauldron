using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    public class SampleCards_Test
    {
        [Fact]
        public async Task QuickGoblin()
        {
            var testCardDef = SampleCards.QuickGoblin;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(SampleCards.CardsetName, new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var status = await g.AttackToPlayer(pid, testcard.Id, player2Id);

                // ���U�����Ȃ̂ōU���ł���
                Assert.Equal(GameMasterStatusCode.OK, status);
            });
        }

        [Fact]
        public async Task ShieldGoblin()
        {
            var normalcardDef = SampleCards.Goblin;
            normalcardDef.Cost = 0;
            normalcardDef.NumTurnsToCanAttack = 0;
            var testCardDef = SampleCards.ShieldGoblin;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(SampleCards.CardsetName, new[] { normalcardDef, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);
            var (normal, test) = await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var normalCard = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                return (normalCard, testcard);
            });

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                // �J�o�[������̂ōU���ł��Ȃ�
                var status = await g.AttackToCreature(pid, normal2.Id, normal.Id);
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                // �J�o�[�ɂ͍U���ł���
                status = await g.AttackToCreature(pid, normal2.Id, test.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
            });
        }

        [Fact]
        public async Task DeadlyGoblin()
        {
            var normalcardDef = SampleCards.Goblin;
            normalcardDef.Cost = 0;
            normalcardDef.Toughness = 10;
            normalcardDef.NumTurnsToCanAttack = 0;
            var testCardDef = SampleCards.DeadlyGoblin;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(SampleCards.CardsetName, new[] { normalcardDef, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);
            var test = await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                return testcard;
            });

            var (normal2, test2) = await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);
                var test2 = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // �X�e���X�Ȃ̂ōU���ł��Ȃ�
                var status = await g.AttackToCreature(pid, normal2.Id, test.Id);
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                return (normal2, test2);
            });

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // �K�E�Ȃ̂œ|����A����������
                var status = await g.AttackToCreature(pid, test.Id, normal2.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(ZoneName.Cemetery, normal2.Zone.ZoneName);
                Assert.Equal(ZoneName.Cemetery, test.Zone.ZoneName);

                return testcard;
            });
        }

        [Fact]
        public async Task MechanicGoblin()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var tokenDef = SampleCards.KarakuriGoblin;
            var testCardDef = SampleCards.MechanicGoblin;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(SampleCards.CardsetName, new[] { tokenDef, testCardDef, goblinDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            // testcard���o��
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforeHands = testGameMaster.PlayersById[pid].Hands.AllCards.Select(c => c.Id).ToArray();

                await g.DestroyCard(testcard);

                // ��D�Ƀg�[�N�����ꖇ������
                var addedHands = g.PlayersById[pid].Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
                Assert.Single(addedHands);
                Assert.Equal(tokenDef.Id, addedHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task NinjaGoblin()
        {
            var testCard = SampleCards.NinjaGoblin;
            testCard.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(SampleCards.CardsetName, new[] { testCard }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCard.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCard.Id, 40));

            await testGameMaster.Start(player1Id);
            await TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�2�̏o�Ă��āA�����testcard
            Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(testCard.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task GoblinsGreed()
        {
            var testCardDef = SampleCards.GoblinsGreed;
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

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 2, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                Assert.Equal(beforeNumOfHands + 2, afterNumOfHands);
            });
        }

        [Fact]
        public async Task GoblinsGreed_��D����̂Ă�()
        {
            var testCardDef = SampleCards.GoblinsGreed;
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
        public async Task ShamanGoblin()
        {
            var testCardDef = SampleCards.ShamanGoblin;
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
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeNumFields = g.PlayersById[player1Id].Field.Count;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterNumFields = g.PlayersById[player1Id].Field.Count;

                // 1���j�󂳂�Ă���͂�
                Assert.Equal(beforeNumFields - 1, afterNumFields);
            });
        }

        [Fact]
        public async Task HealGoblin()
        {
            var testCardDef = SampleCards.HealGoblin;
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
        public async Task FireGoblin()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            var testCardDef = SampleCards.FireGoblin;
            testCardDef.Cost = 0;

            //var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            //testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            PlayerId[] expectedAskPlayerLsit = default;
            Card[] expectedAskCardLsit = default;

            ValueTask<ChoiceResult> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                Assert.Equal(1, i);
                TestUtil.AssertCollection(expectedAskPlayerLsit, c.PlayerIdList);
                TestUtil.AssertCollection(
                    expectedAskCardLsit.Select(c => c.Id).ToArray(),
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceResult(
                    c.PlayerIdList.Take(1).ToArray(),
                    Array.Empty<CardId>(),
                    Array.Empty<CardDefId>()));
            }

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef }, TestUtil.GameMasterOptions(
                EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                ));

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            expectedAskPlayerLsit = new[] { player1Id };
            expectedAskCardLsit = new[] { goblin1, goblin2 };

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHp = g.GetOpponent(pId).CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHp = g.GetOpponent(pId).CurrentHp;

                Assert.Equal(beforeHp - 2, afterHp);
            });
        }

        [Fact]
        public async Task MadScientist()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2);

            var testCardDef = SampleCards.MadScientist;
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

        [Fact]
        public async Task BraveGoblin()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.NumTurnsToCanAttack = 0;

            var testCardDef = SampleCards.BraveGoblin;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �_���[�W�y�������
                await g.HitCreature(new(testcard, 3, testcard));
                Assert.Equal(testcard.BaseToughness - 1, testcard.Toughness);

                // �ق��N���[�`���[�̍U�������������
                var beforeHp = g.PlayersById[player2Id].CurrentHp;
                await g.AttackToPlayer(pId, goblin.Id, player2Id);
                var afterHp = g.PlayersById[player2Id].CurrentHp;
                Assert.Equal(beforeHp - (goblin.Power + 1), afterHp);

                // �����̍U���͋�������Ȃ�
                beforeHp = g.PlayersById[player2Id].CurrentHp;
                await g.AttackToPlayer(pId, testcard.Id, player2Id);
                afterHp = g.PlayersById[player2Id].CurrentHp;
                Assert.Equal(beforeHp - testcard.Power, afterHp);
            });
        }

        [Fact]
        public async Task GiantGoblin()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards.GiantGoblin;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = g.PlayersById[pId].CurrentHp;
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �����N���[�`���[�Ƀ_���[�W
                Assert.Equal(goblin.BaseToughness - 3, goblin.Toughness);
                Assert.Equal(goblin2.BaseToughness - 3, goblin2.Toughness);

                // �v���C���[�ɂ̓_���[�W�Ȃ�
                Assert.Equal(beforeHp, g.PlayersById[pId].CurrentHp);
            });
        }

        [Fact]
        public async Task LeaderGoblin()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
            var testCreatureDef = SampleCards.LeaderGoblin;
            testCreatureDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // �S�u�����Q�̏o���Ă�����ʃN���[�`���[���o��
            await testGameMaster.Start(player1Id);
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

                // �S�u������2�̂Ƃ�+1/+0 ����Ă���
                Assert.Equal(1, goblinCard.PowerBuff);
                Assert.Equal(0, goblinCard.ToughnessBuff);
                Assert.Equal(1, goblinCard2.PowerBuff);
                Assert.Equal(0, goblinCard2.ToughnessBuff);

                // �����������̓o�t����Ȃ�
                Assert.Equal(0, testCreatureCard.PowerBuff);
                Assert.Equal(0, testCreatureCard.ToughnessBuff);

                return (goblinCard, goblinCard2);
            });

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                // �S�u������2�̂Ƃ�+1/+0 �̂܂�
                Assert.Equal(1, goblin1.PowerBuff);
                Assert.Equal(0, goblin1.ToughnessBuff);
                Assert.Equal(1, goblin2.PowerBuff);
                Assert.Equal(0, goblin2.ToughnessBuff);
            });

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                // �S�u������2�̂Ƃ�+2/+0 �ɂȂ�
                Assert.Equal(2, goblin1.PowerBuff);
                Assert.Equal(0, goblin1.ToughnessBuff);
                Assert.Equal(2, goblin2.PowerBuff);
                Assert.Equal(0, goblin2.ToughnessBuff);
            });
        }

        [Fact]
        public async Task GoblinTyrant()
        {
            var testCardDef = SampleCards.TyrantGoblin;
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
        public async Task Sword()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = SampleCards.Sword;
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
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(1, goblinCard.PowerBuff);
                Assert.Equal(0, goblinCard.ToughnessBuff);
            });
        }

        [Fact]
        public async Task Shield()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = SampleCards.Shield;
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
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, goblinCard.PowerBuff);
                Assert.Equal(1, goblinCard.ToughnessBuff);
            });
        }

        [Fact]
        public async Task HolyShield()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, numTurnsToCanAttack: 0);

            var testCardDef = SampleCards.HolyShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // ��U
            var goblin3 = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �S�u�����œG���U��
                // �U���������̓_���[�W���󂯂Ȃ�
                await g.AttackToCreature(pId, goblin3.Id, goblin1.Id);
                Assert.Equal(0, goblin1.Toughness);
                Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);

                return goblin3;
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ���̃^�[���Ȃ̂ŁA���݂��Ƀ_���[�W���󂯂�
                await g.AttackToCreature(pId, goblin2.Id, goblin3.Id);
                Assert.Equal(0, goblin2.Toughness);
                Assert.Equal(0, goblin3.Toughness);
            });
        }

        [Fact]
        public async Task ChangeHands()
        {
            var testCardDef = SampleCards.ChangeHands;
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
        public async Task Slap()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Toughness = 10;
            var testCardDef = SampleCards.Slap;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            Card[] candidateCardList = default;
            CardId choiceCardId = default;
            ValueTask<ChoiceResult> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                Assert.Equal(1, i);
                Assert.Empty(c.PlayerIdList);
                TestUtil.AssertCollection(
                    candidateCardList.Select(c => c.Id).ToArray(),
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceResult(
                    Array.Empty<PlayerId>(),
                    new[] { choiceCardId },
                    Array.Empty<CardDefId>()));
            }

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                ));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblin1, goblin11) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin11);
            });

            candidateCardList = new[] { goblin1, goblin11 };

            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                choiceCardId = goblin1.Id;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���2�̂���̂�2�_���[�W
                Assert.Equal(goblin1.BaseToughness - 2, goblin1.Toughness);

                var goblin4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                choiceCardId = goblin11.Id;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���3�̂���̂�3�_���[�W
                Assert.Equal(goblin11.BaseToughness - 3, goblin11.Toughness);
            });
        }

        [Fact]
        public async Task GoblinCaptureJar()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            var notGoblinDef = SampleCards.Goblin;
            notGoblinDef.Name = "�X���C��";
            goblinDef.Cost = 0;
            var testCardDef = SampleCards.GoblinCaptureJar;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, notGoblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblin1, notGoblin1) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var notGoblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, notGoblinDef.Id);

                return (goblin1, notGoblin1);
            });

            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var notGoblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, notGoblinDef.Id);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���R���G�R���S�u�����͕���{�p���[1�ɂȂ�
                Assert.Contains(CreatureAbility.Sealed, goblin1.Abilities);
                Assert.Equal(1, goblin1.Power);
                Assert.Contains(CreatureAbility.Sealed, goblin2.Abilities);
                Assert.Equal(1, goblin2.Power);

                // �S�u�����ȊO�͂Ȃɂ��Ȃ�Ȃ�
                Assert.DoesNotContain(CreatureAbility.Sealed, notGoblin1.Abilities);
                Assert.Equal(notGoblin1.BasePower, notGoblin1.Power);
                Assert.DoesNotContain(CreatureAbility.Sealed, notGoblin2.Abilities);
                Assert.Equal(notGoblin2.BasePower, notGoblin2.Power);
            });
        }


        [Fact]
        public async Task FullAttack()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            var testCardDef = SampleCards.FullAttack;
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

            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(goblin1.BaseToughness - 1, goblin1.Toughness);
                Assert.Equal(goblin2.BaseToughness - 1, goblin1.Toughness);
                Assert.Equal(TestUtil.TestRuleBook.InitialPlayerHp - 1, g.PlayersById[player1Id].CurrentHp);
            });
        }

        [Fact]
        public async Task OldShield_�N���[�`���[�̖h�䎞()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // �N���[�`���[��1�_���[�W�����󂯂Ȃ�
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
                // �j�󂳂��
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_�v���C���[�̖h�䎞()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await testGameMaster.AttackToPlayer(pId, goblinCard2.Id, player1Id);

                // �v���C���[��2�_���[�W�󂯂�
                Assert.Equal(TestUtil.TestRuleBook.InitialPlayerHp - 2, g.PlayersById[player1Id].CurrentHp);
                // �j�󂳂�Ȃ�
                Assert.Equal(ZoneName.Field, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_�U����()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "a", "", 2, 2, 0);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var goblinCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // ���R�N���[�`���[���ΏۂɂȂ�
                Assert.Equal(goblinCard.BaseToughness - 2, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - 1, goblinCard2.Toughness);
                // �j�󂳂��
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_�v���C���[���U��()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var testcard = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = g.PlayersById[player1Id].CurrentHp;
                await testGameMaster.AttackToPlayer(pId, goblinCard.Id, player1Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(beforeHp - 1, g.PlayersById[player1Id].CurrentHp);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_�N���[�`���[���U��()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 2, 2, 0);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var (goblin, testcard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblin, testcard);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = g.PlayersById[player1Id].CurrentHp;
                await testGameMaster.AttackToCreature(pId, goblin2.Id, goblin.Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(goblin.BaseToughness - 1, goblin.Toughness);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_�U����()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "a", "", 2, 2, 0);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var (testGameMaster, player1Id, player2Id) = TestUtil.InitTest(new[] { goblinDef, testCardDef });

            await testGameMaster.Start(player1Id);

            // ��U
            var goblinCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // ���R�N���[�`���[���ΏۂɂȂ�
                Assert.Equal(goblinCard.BaseToughness - 2, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - 1, goblinCard2.Toughness);
                // �j�󂳂��
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task GoblinStatue()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 10, 0);

            var testCardDef = SampleCards.GoblinStatue;
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
            var goblin3 = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // 30����n�ɑ���
                foreach (var _ in Enumerable.Range(0, 30))
                {
                    var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                    await g.DestroyCard(goblin);
                }

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblin3;
            });

            // ����v���C���[�ƃN���[�`���[��6�_���[�W
            Assert.Equal(testGameMaster.RuleBook.InitialPlayerHp - 6, testGameMaster.PlayersById[player1Id].CurrentHp);
            Assert.Equal(goblin1.BaseToughness - 6, goblin1.Toughness);
            Assert.Equal(goblin2.BaseToughness - 6, goblin2.Toughness);

            // �����v���C���[�Ǝ����N���[�`���[�̓_���[�W���󂯂Ȃ�
            Assert.Equal(testGameMaster.RuleBook.InitialPlayerHp, testGameMaster.PlayersById[player2Id].CurrentHp);
            Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);
        }

        [Fact]
        public async Task HolyStatue()
        {
            var goblin = MessageObjectExtensions.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

            var testCardDef = SampleCards.HolyStatue;
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
                Assert.True(goblinCard.PowerBuff == 0 && goblinCard.ToughnessBuff == 1);
                Assert.True(goblinCard2.PowerBuff == 0 && goblinCard2.ToughnessBuff == 1);

                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                // ���Ƃɏ�ɏo���S�u�����������C�������
                Assert.True(goblinCard.PowerBuff == 0 && goblinCard.ToughnessBuff == 1);
                Assert.True(goblinCard2.PowerBuff == 0 && goblinCard2.ToughnessBuff == 1);
                Assert.True(goblinCard3.PowerBuff == 0 && goblinCard3.ToughnessBuff == 1);
            });
        }
    }
}
