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

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // ���U�����Ȃ̂ōU���ł���
                Assert.Equal(GameMasterStatusCode.OK, status);
            });
        }

        [Fact]
        public async Task TwinGoblin()
        {
            var testCardDef = SampleCards.TwinGoblin;
            testCardDef.Cost = 0;
            testCardDef.NumTurnsToCanAttack = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 1�^�[����2��܂ōU���ł���
                Assert.Equal(GameMasterStatusCode.OK, status);

                status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 1�^�[����2��܂ōU���ł���
                Assert.Equal(GameMasterStatusCode.OK, status);

                status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 3��ڂ͎��s
                Assert.Equal(GameMasterStatusCode.CantAttack, status);
            });
        }

        [Fact]
        public async Task SlowGoblin()
        {
            var testCardDef = SampleCards.SlowGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            var testcard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 2�^�[���o�߂���܂ōU���ł��Ȃ�
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                return testcard;
            });

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 2�^�[���o�߂���܂ōU���ł��Ȃ�
                Assert.Equal(GameMasterStatusCode.CantAttack, status);
            });

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 2�^�[���o�߂����̂ōU���ł���
                Assert.Equal(GameMasterStatusCode.OK, status);
            });
        }

        [Fact]
        public async Task DoubleStrikeGoblin_�U������()
        {
            var testCardDef = SampleCards.DoubleStrikeGoblin;
            testCardDef.Cost = 0;
            testCardDef.NumTurnsToCanAttack = 0;

            var goblinT1Def = SampleCards.Goblin;
            goblinT1Def.Cost = 0;
            goblinT1Def.Toughness = 1;

            var goblinT3Def = SampleCards.Goblin;
            goblinT3Def.Cost = 0;
            goblinT3Def.Toughness = 3;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinT1Def, goblinT3Def });

            var (goblinT1, goblinT3) = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var goblinT1 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinT1Def.Id);
                var goblinT3 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinT3Def.Id);

                return (goblinT1, goblinT3);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // �搧�U���œ|���؂�̂Ŏ����̓_���[�W���󂯂Ȃ�
                await g.AttackToCreature(pid, testCard.Id, goblinT1.Id);

                // �����̓_���[�W�Ȃ�
                Assert.Equal(testCard.BaseToughness, testCard.Toughness);
                // ����͓|��
                Assert.Equal(0, goblinT1.Toughness);

                var testCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // �����|������Ȃ��̂Ń_���[�W���󂯂�
                await g.AttackToCreature(pid, testCard2.Id, goblinT3.Id);

                Assert.Equal(0, testCard2.Toughness);
                Assert.Equal(0, goblinT3.Toughness);
            });
        }

        [Fact]
        public async Task DoubleStrikeGoblin_�U�������()
        {
            var testCardDef = SampleCards.DoubleStrikeGoblin;
            testCardDef.Cost = 0;
            testCardDef.NumTurnsToCanAttack = 0;

            var goblinT1Def = SampleCards.Goblin;
            goblinT1Def.Cost = 0;
            goblinT1Def.Toughness = 1;

            var goblinT3Def = SampleCards.Goblin;
            goblinT3Def.Cost = 0;
            goblinT3Def.Toughness = 3;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinT1Def, goblinT3Def });

            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var goblinT1 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinT1Def.Id);
                var goblinT3 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinT3Def.Id);

                // �搧�U���œ|���؂�̂Ŏ����̓_���[�W���󂯂Ȃ�
                await g.AttackToCreature(pid, goblinT1.Id, testCard.Id);

                Assert.Equal(testCard.BaseToughness, testCard.Toughness);

                // �����|������Ȃ��̂Ń_���[�W���󂯂�
                await g.AttackToCreature(pid, goblinT3.Id, testCard.Id);

                Assert.Equal(0, testCard.Toughness);
                Assert.Equal(0, goblinT3.Toughness);
            });
        }

        [Fact]
        public async Task MagicBook()
        {
            var testCardDef = SampleCards.MagicBook;
            testCardDef.Cost = 0;
            var sorceryDef = SampleCards.Sword;

            var c = await TestUtil.InitTest(new[] { testCardDef, sorceryDef });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var beforeHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var afterHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandIdList = afterHandIdList.Except(beforeHandIdList).ToArray();

                Assert.Single(diffHandIdList);

                var (_, diffCard) = c.CardRepository.TryGetById(diffHandIdList[0]);
                Assert.Equal(sorceryDef.Id, diffCard.CardDefId);
            });
        }

        [Fact]
        public async Task GoblinFollower()
        {
            var testCardDef = SampleCards.GoblinFollower;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            var goblinDef2 = SampleCards.QuickGoblin;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, goblinDef, goblinDef2 },
                Enumerable.Repeat(testCardDef, 4)
                    .Concat(Enumerable.Repeat(goblinDef, 20))
                    .Concat(Enumerable.Repeat(goblinDef2, 16))
                    );

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var beforeFieldIdList = c.Player1.Field.AllCards.Select(c => c.Id).ToArray();
                var beforeDeckCount = c.Player1.Deck.Count;

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var afterFieldIdList = c.Player1.Field.AllCards.Select(c => c.Id).ToArray();
                var diffFieldIdList = afterFieldIdList
                    .Except(beforeFieldIdList)
                    // �����ŏo�����J�[�h�ȊO
                    .Where(id => id != goblin.Id)
                    .ToArray();

                var afterDeckCount = c.Player1.Deck.Count;

                //TODO ���܂���2���Ƃ���D�ɗ��Ă�ƃe�X�g�����s���邼�I�I
                Assert.True(diffFieldIdList.Length > 1);
                Assert.Equal(beforeDeckCount - afterDeckCount, diffFieldIdList.Length);

                foreach (var diffId in diffFieldIdList)
                {
                    var (_, diffCard) = c.CardRepository.TryGetById(diffId);
                    Assert.Equal(testCardDef.Id, diffCard.CardDefId);
                }
            });
        }

        /// <summary>
        /// �S�u�����Ɗ֌W�Ȃ��J�[�h�Ŕ������Ȃ��e�X�g
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GoblinFollower2()
        {
            var testCardDef = SampleCards.GoblinFollower;
            testCardDef.Cost = 0;

            var notGoblinDef = SampleCards.Goblin;
            notGoblinDef.Cost = 0;
            notGoblinDef.Name = "�e�X�g";

            var c = await TestUtil.InitTest(
                new[] { testCardDef, notGoblinDef },
                Enumerable.Repeat(testCardDef, 4)
                    .Concat(Enumerable.Repeat(notGoblinDef, 36))
                    );

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var beforeFieldIdList = c.Player1.Field.AllCards.Select(c => c.Id).ToArray();
                var beforeDeckCount = c.Player1.Deck.Count;

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, notGoblinDef.Id);

                var afterFieldIdList = c.Player1.Field.AllCards.Select(c => c.Id).ToArray();
                var diffFieldIdList = afterFieldIdList
                    .Except(beforeFieldIdList)
                    // �����ŏo�����J�[�h�ȊO
                    .Where(id => id != goblin.Id)
                    .ToArray();

                var afterDeckCount = c.Player1.Deck.Count;

                Assert.Empty(diffFieldIdList);
                Assert.Equal(0, beforeDeckCount - afterDeckCount);
            });
        }

        /// <summary>
        /// ���O�ɃS�u�������܂܂�邪�N���[�`���[�łȂ��J�[�h�Ŕ������Ȃ��e�X�g
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GoblinFollower3()
        {
            var testCardDef = SampleCards.GoblinFollower;
            testCardDef.Cost = 0;

            var notGoblinCreatureDef = SampleCards.GoblinCaptureJar;
            notGoblinCreatureDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, notGoblinCreatureDef },
                Enumerable.Repeat(testCardDef, 4)
                    .Concat(Enumerable.Repeat(notGoblinCreatureDef, 36))
                    );

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var beforeFieldIdList = c.Player1.Field.AllCards.Select(c => c.Id).ToArray();
                var beforeDeckCount = c.Player1.Deck.Count;

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, notGoblinCreatureDef.Id);

                var afterFieldIdList = c.Player1.Field.AllCards.Select(c => c.Id).ToArray();
                var diffFieldIdList = afterFieldIdList
                    .Except(beforeFieldIdList)
                    // �����ŏo�����J�[�h�ȊO
                    .Where(id => id != goblin.Id)
                    .ToArray();

                var afterDeckCount = c.Player1.Deck.Count;

                Assert.Empty(diffFieldIdList);
                Assert.Equal(beforeDeckCount, afterDeckCount);
            });
        }

        [Fact]
        public async Task Insector()
        {
            var testCardDef = SampleCards.Insector;
            testCardDef.Cost = 0;

            var tokenDef = SampleCards.Parasite;

            var c = await TestUtil.InitTest(new[] { testCardDef, tokenDef });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var op = g.GetOpponent(pid);

                var beforeOpDeckIdList = op.Deck.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var afterOpDeckIdList = op.Deck.AllCards.Select(c => c.Id).ToArray();
                var diffOpDeckIdList = afterOpDeckIdList
                    .Except(beforeOpDeckIdList)
                    .ToArray();

                Assert.Single(diffOpDeckIdList);

                var (_, diffCard) = c.CardRepository.TryGetById(diffOpDeckIdList[0]);
                Assert.Equal(tokenDef.Id, diffCard.CardDefId);

                var deckTopCard = op.Deck.AllCards[0];
                Assert.Equal(tokenDef.Id, deckTopCard.CardDefId);
            });
        }

        [Fact]
        public async Task MagicShieldGoblin()
        {
            var testCardDef = SampleCards.MagicShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, });

            var (p1Goblin, p1TestCard) = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                return (goblin, testCard);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);
                var op = g.GetOpponent(pid);

                // �U�������ƍU�������J�[�h��������̎�D�ɖ߂�
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforpHandsIdList)
                    .ToArray();

                // �U�������J�[�h����D�ɖ߂��Ă���
                Assert.Empty(p.Field.AllCards);

                Assert.Single(diffHandsIdList);
                Assert.Equal(p2Goblin.Id, diffHandsIdList[0]);

                // �U�������Ƃ��͌��ʂ��������Ȃ�
                await g.DestroyCard(p1TestCard);
                var p2TestCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforpOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2TestCard.Id, p1Goblin.Id));

                var afterOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                // ���ʂ��������Ȃ��̂ő���t�B�[���h��̃J�[�h�ɕύX�͂Ȃ�
                TestUtil.AssertCollection(beforpOpFieldIdList, afterOpFieldIdList);
            });
        }

        [Fact]
        public async Task MagicShieldGoblin_�U���J�[�h���j�󂳂ꂽ�ꍇ()
        {
            var testCardDef = SampleCards.MagicShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Toughness = 1;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, });

            var p1TestCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);
                var op = g.GetOpponent(pid);

                // �U�������ƍU�������J�[�h��������̎�D�ɖ߂�
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforpHandsIdList)
                    .ToArray();

                // �U�������J�[�h�͔j�󂳂��n�ɍs��
                Assert.Empty(p.Field.AllCards);
                Assert.Single(p.Cemetery.AllCards);

                // ��D�ɂ͖߂�Ȃ�
                Assert.Empty(diffHandsIdList);
            });
        }

        [Fact]
        public async Task MagicShieldGoblin_�������j�󂳂ꂽ�ꍇ()
        {
            var testCardDef = SampleCards.MagicShieldGoblin;
            testCardDef.Cost = 0;
            testCardDef.Toughness = 1;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Power = 2;
            goblinDef.Toughness = 5;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, });

            var p1TestCard = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);
                var op = g.GetOpponent(pid);

                // �U������Ă��A�������j�󂳂ꂽ�ꍇ�͑���͎�D�ɖ߂�Ȃ�
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforpHandsIdList)
                    .ToArray();

                // �j�󂳂��n�ɍs��
                Assert.Empty(op.Field.AllCards);
                Assert.Single(op.Cemetery.AllCards);

                // �U�����͎�D�ɂ͖߂�Ȃ�
                Assert.Empty(diffHandsIdList);
            });
        }

        [Fact]
        public async Task SuperMagicShieldGoblin()
        {
            var testCardDef = SampleCards.SuperMagicShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, });

            var (p1Goblin, p1TestCard) = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                return (goblin, testCard);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);
                var op = g.GetOpponent(pid);

                // �U�������ƍU�������J�[�h��������̎�D�ɖ߂�
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpDeckIdList = p.Deck.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterDeckIdList = p.Deck.AllCards.Select(c => c.Id).ToArray();
                var diffDeckIdList = afterDeckIdList
                    .Except(beforpDeckIdList)
                    .ToArray();

                Assert.Empty(p.Field.AllCards);

                // �f�b�L�Ɉړ����Ă���
                Assert.Single(diffDeckIdList);
                Assert.Equal(p2Goblin.Id, diffDeckIdList[0]);

                // �f�b�L�̈�ԏ�
                Assert.Equal(p2Goblin.Id, afterDeckIdList[0]);

                // �U�������Ƃ��͌��ʂ��������Ȃ�
                await g.DestroyCard(p1TestCard);
                var p2TestCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforpOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2TestCard.Id, p1Goblin.Id));

                var afterOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                // ���ʂ��������Ȃ��̂ő���t�B�[���h��̃J�[�h�ɕύX�͂Ȃ�
                TestUtil.AssertCollection(beforpOpFieldIdList, afterOpFieldIdList);
            });
        }

        [Fact]
        public async Task GoblinsPet()
        {
            var testCardDef = SampleCards.GoblinsPet;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            var sorceryDef = SampleCards.SelectDamage;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, goblinDef, sorceryDef },
                Enumerable.Repeat(goblinDef, 10)
                    .Concat(Enumerable.Repeat(sorceryDef, 10))
                    );

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var op = c.GameMaster.GetOpponent(pid);

                var beforeOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();
                var beforeOpHandsIdList = op.Hands.AllCards.Select(c => c.Id).ToArray();

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var afterOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();
                var diffOpFieldIdList = afterOpFieldIdList
                    .Except(beforeOpFieldIdList)
                    .ToArray();

                var afterOpHandsIdList = op.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffOpHandsIdList = beforeOpHandsIdList
                    .Except(afterOpHandsIdList)
                    .ToArray();

                Assert.Single(diffOpFieldIdList);
                Assert.Single(diffOpHandsIdList);

                Assert.Equal(diffOpFieldIdList[0], diffOpFieldIdList[0]);

                var (_, diffCard) = c.CardRepository.TryGetById(diffOpFieldIdList[0]);
                Assert.Equal(goblinDef.Id, diffCard.CardDefId);
            });
        }

        [Fact]
        public async Task MechanicGoblin()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 1, 2);
            var tokenDef = SampleCards.KarakuriGoblin;
            var testCardDef = SampleCards.MechanicGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { tokenDef, testCardDef, goblinDef });

            // ��U
            // testcard���o��
            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforeHands = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                await g.DestroyCard(testcard);

                // ��D�Ƀg�[�N�����ꖇ������
                var addedHands = c.Player1.Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
                Assert.Single(addedHands);
                Assert.Equal(tokenDef.Id, addedHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task NinjaGoblin()
        {
            var testCard = SampleCards.NinjaGoblin;
            testCard.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCard });

            await TestUtil.AssertGameAction(() =>
                c.GameMaster.PlayFromHand(c.Player1.Id, c.GameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�2�̏o�Ă��āA�����testcard
            Assert.Equal(2, c.GameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in c.GameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(testCard.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task SuperNinjaGoblin()
        {
            var testCard = SampleCards.SuperNinjaGoblin;
            testCard.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCard });

            await TestUtil.AssertGameAction(() =>
                c.GameMaster.PlayFromHand(c.Player1.Id, c.GameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�3�̏o�Ă��āA�����testcard
            Assert.Equal(3, c.GameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in c.GameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(testCard.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task GoblinsGreed()
        {
            var testCardDef = SampleCards.GoblinsGreed;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
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

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
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

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeNumFields = c.Player1.Field.Count;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterNumFields = c.Player1.Field.Count;

                // 1���j�󂳂�Ă���͂�
                Assert.Equal(beforeNumFields - 1, afterNumFields);
            });
        }

        [Fact]
        public async Task HealGoblin()
        {
            var testCardDef = SampleCards.HealGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ���O�Ƀ_���[�W
                c.Player1.Damage(5);

                var beforeHp = c.Player1.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 2�񕜂��Ă���
                Assert.Equal(beforeHp + 2, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task FireGoblin()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            var testCardDef = SampleCards.FireGoblin;
            testCardDef.Cost = 0;

            PlayerId[] expectedAskPlayerLsit = default;
            Card[] expectedAskCardLsit = default;

            ValueTask<ChoiceAnswer> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                Assert.Equal(1, i);
                TestUtil.AssertCollection(expectedAskPlayerLsit, c.PlayerIdList);
                TestUtil.AssertCollection(
                    expectedAskCardLsit.Select(c => c.Id).ToArray(),
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceAnswer(
                    c.PlayerIdList.Take(1).ToArray(),
                    Array.Empty<CardId>(),
                    Array.Empty<CardDefId>()));
            }

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef }, TestUtil.GameMasterOptions(
                EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                ));

            // ��U
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            expectedAskPlayerLsit = new[] { c.Player1.Id };
            expectedAskCardLsit = new[] { goblin1, goblin2 };

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHp = g.GetOpponent(pId).CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHp = g.GetOpponent(pId).CurrentHp;

                Assert.Equal(beforeHp - 2, afterHp);
            });
        }

        [Fact]
        public async Task DashGoblin()
        {
            var testCardDef = SampleCards.DashGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                // �v���C����Ƒ����1�_���[�W
                var beforeHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp - 1, op.CurrentHp);

                // �v���C�ȊO�̕��@�ŏ�ɏo�Ă������1�_���[�W
                beforeHp = op.CurrentHp;
                await g.GenerateNewCard(testCardDef.Id, new(pId, ZoneName.Field), default);
                Assert.Equal(beforeHp - 1, op.CurrentHp);
            });
        }

        [Fact]
        public async Task BeginnerSummoner()
        {
            var testCardDef = SampleCards.BeginnerSummoner;
            testCardDef.Cost = 0;
            var cost1Def = SampleCards.Goblin;
            cost1Def.Cost = 1;
            var cost2Def = SampleCards.Goblin;
            cost2Def.Cost = 2;

            var c = await TestUtil.InitTest(new[] { testCardDef, cost1Def, cost2Def });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ��ɂ�testCard��1�̏o�Ă���
                Assert.Single(g.ActivePlayer.Field.AllCards);

                await g.DestroyCard(testCard);

                // �j�󂳂���2�R�X�g�̃J�[�h����ɏo��
                Assert.Single(g.ActivePlayer.Field.AllCards);
                Assert.Equal(cost2Def.Id, g.ActivePlayer.Field.AllCards[0].CardDefId);
            });
        }

        [Fact]
        public async Task MadScientist()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 2, 2);
            var testCardDef = SampleCards.MadScientist;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef },
                options: TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick))
                );

            // ��U
            var goblin = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(1, c.Player1.Field.Count);
                Assert.Equal(goblin.Id, c.Player1.Field.AllCards[0].Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(1, c.Player1.Field.Count);
                Assert.NotEqual(goblin.Id, c.Player1.Field.AllCards[0].Id);
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

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �_���[�W�y�������
                await g.HitCreature(new(testcard, 3, testcard));
                Assert.Equal(testcard.BaseToughness - 1, testcard.Toughness);

                // �ق��N���[�`���[�̍U�������������
                var beforeHp = c.Player2.CurrentHp;
                await g.AttackToPlayer(pId, goblin.Id, c.Player2.Id);
                var afterHp = c.Player2.CurrentHp;
                Assert.Equal(beforeHp - (goblin.Power + 1), afterHp);

                // �����̍U���͋�������Ȃ�
                beforeHp = c.Player2.CurrentHp;
                await g.AttackToPlayer(pId, testcard.Id, c.Player2.Id);
                afterHp = c.Player2.CurrentHp;
                Assert.Equal(beforeHp - testcard.Power, afterHp);
            });
        }

        [Fact]
        public async Task MagicDragon()
        {
            var testCardDef = SampleCards.MagicDragon;
            testCardDef.Cost = 0;

            var sorceryDef = SampleCards.SelectDamage;

            PlayerId[] choicePlayerIdList = default;
            var c = await TestUtil.InitTest(new[] { testCardDef, sorceryDef },
                options: TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(
                        AskCardAction: (_, c, _) =>
                        {
                            return ValueTask.FromResult(new ChoiceAnswer(
                                choicePlayerIdList,
                                Array.Empty<CardId>(),
                                Array.Empty<CardDefId>()
                                ));
                        })));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                choicePlayerIdList = new[] { op.Id };

                var beforeHandsCount = c.Player1.Hands.Count;

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandsCount = c.Player1.Hands.Count;

                // 1���h���[���Ă���
                Assert.Equal(1, afterHandsCount - beforeHandsCount);

                var beforeOpHp = op.CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterOpHp = op.CurrentHp;

                // �_���[�W��+1���Ă���
                Assert.Equal(2, beforeOpHp - afterOpHp);

            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                choicePlayerIdList = new[] { op.Id };

                var beforeOpHp = op.CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterOpHp = op.CurrentHp;

                // ����̖��@�ł̓_���[�W��+1����Ȃ�
                Assert.Equal(1, beforeOpHp - afterOpHp);

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

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = c.Player1.CurrentHp;
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �����N���[�`���[�Ƀ_���[�W
                Assert.Equal(goblin.BaseToughness - 3, goblin.Toughness);
                Assert.Equal(goblin2.BaseToughness - 3, goblin2.Toughness);

                // �v���C���[�ɂ̓_���[�W�Ȃ�
                Assert.Equal(beforeHp, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task LeaderGoblin()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 1, 2);
            var testCreatureDef = SampleCards.LeaderGoblin;
            testCreatureDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCreatureDef });

            // �S�u�����Q�̏o���Ă�����ʃN���[�`���[���o��
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
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

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
                // �S�u������2�̂Ƃ�+1/+0 �̂܂�
                Assert.Equal(1, goblin1.PowerBuff);
                Assert.Equal(0, goblin1.ToughnessBuff);
                Assert.Equal(1, goblin2.PowerBuff);
                Assert.Equal(0, goblin2.ToughnessBuff);
            });

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
                // �S�u������2�̂Ƃ�+2/+0 �ɂȂ�
                Assert.Equal(2, goblin1.PowerBuff);
                Assert.Equal(0, goblin1.ToughnessBuff);
                Assert.Equal(2, goblin2.PowerBuff);
                Assert.Equal(0, goblin2.ToughnessBuff);
            });
        }

        [Fact]
        public async Task TyrantGoblin()
        {
            var testCardDef = SampleCards.TyrantGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ����1�������邩��
                var numHands = c.Player1.Hands.AllCards.Count;
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(testCard.BasePower + numHands, testCard.Power);
                Assert.Equal(testCard.BaseToughness + numHands, testCard.Toughness);
            });
        }

        [Fact]
        public async Task RiderGoblin()
        {
            var testCardDef = SampleCards.RiderGoblin;
            testCardDef.Cost = 0;
            var tokenDef = SampleCards.WarGoblin;
            var sorceryDef = SampleCards.RandomDamage;
            sorceryDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, tokenDef, sorceryDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var (_, p) = g.playerRepository.TryGet(pId);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(p.Field.AllCards);

                var beforeFieldCardIdList = p.Field.AllCards.Select(c => c.Id).ToArray();

                // ���@�J�[�h���v���C����
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterFieldCardIdList = p.Field.AllCards.Select(c => c.Id).ToArray();
                var diffFieldCardIdList = afterFieldCardIdList.Except(beforeFieldCardIdList).ToArray();

                // �����̏��1���ǉ������
                Assert.Single(diffFieldCardIdList);

                // �ǉ����ꂽ�J�[�h�̓g�[�N��
                var diffCard = p.Field.AllCards.First(c => c.Id == diffFieldCardIdList[0]);
                Assert.Equal(tokenDef.Id, diffCard.CardDefId);
            });
        }

        [Fact]
        public async Task Doctor()
        {
            var testCardDef = SampleCards.Doctor;
            testCardDef.Cost = 0;
            var addcardDef = SampleCards.DoctorBomb;

            var c = await TestUtil.InitTest(new[] { testCardDef, addcardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ��ɂ�3�̂���
                Assert.Equal(3, c.Player1.Field.Count);
                TestUtil.AssertCollection(
                    c.Player1.Field.AllCards.Select(c => c.CardDefId).ToArray(),
                    new[] { testCardDef.Id, addcardDef.Id, addcardDef.Id }
                    );
            });
        }

        [Fact]
        public async Task DoctorBomb()
        {
            var testCardDef = SampleCards.DoctorBomb;
            testCardDef.IsToken = false;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                for (var i = 0; i < 10; i++)
                {
                    var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                    var beforeOpHp = op.CurrentHp;

                    await g.DestroyCard(testCard);

                    var damage = beforeOpHp - op.CurrentHp;

                    Assert.True(1 <= damage && damage <= 4);

                    op.GainHp(4);
                }
            });
        }

        [Fact]
        public async Task Firelord()
        {
            var testCardDef = SampleCards.Firelord;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            Player op = default;
            int beforeOpHp = default;

            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                op = g.GetOpponent(pId);
                beforeOpHp = op.CurrentHp;
            });

            var afterOpHp = op.CurrentHp;

            Assert.Equal(8, beforeOpHp - afterOpHp);
        }

        [Fact]
        public async Task Death()
        {
            var testCardDef = SampleCards.Death;
            testCardDef.Cost = 0;
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var (_, p) = g.playerRepository.TryGet(pId);
                var op = g.GetOpponent(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHandIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                // ���݂��̏�̃J�[�h��1������
                Assert.Single(op.Field.AllCards);
                Assert.Single(p.Field.AllCards);

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandIdList = beforeHandIdList.Except(afterHandIdList).ToArray();

                // ���̃J�[�h�ȊO�̂��ׂẴN���[�`���[���j�󂳂��
                Assert.Empty(op.Field.AllCards);
                Assert.Single(p.Field.AllCards);

                // �j�󂵂�����������D���̂Ă�
                Assert.Equal(2, diffHandIdList.Length);
            });
        }

        [Fact]
        public async Task TempRamp()
        {
            var testCardDef = SampleCards.TempRamp;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U1�^�[����
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(1, c.Player1.MaxMp);
                Assert.Equal(1, c.Player1.CurrentMp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �ő�l���P������B���g�p��MP��������B
                Assert.Equal(2, c.Player1.MaxMp);
                Assert.Equal(2, c.Player1.CurrentMp);
            });

            // �ő�l���P����B���g�p��MP������B
            Assert.Equal(1, c.Player1.MaxMp);
            Assert.Equal(1, c.Player1.CurrentMp);

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ��U2�^�[����
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(2, c.Player1.MaxMp);
                Assert.Equal(2, c.Player1.CurrentMp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �ő�l���P������B���g�p��MP��������B
                Assert.Equal(3, c.Player1.MaxMp);
                Assert.Equal(3, c.Player1.CurrentMp);
            });

            // �ő�l���P����B���g�p��MP������B
            Assert.Equal(2, c.Player1.MaxMp);
            Assert.Equal(2, c.Player1.CurrentMp);
        }

        [Fact]
        public async Task Salvage()
        {
            var goblinDef = SampleCards.MechanicGoblin;
            goblinDef.Cost = 0;

            var testCardDef = SampleCards.Salvage;
            testCardDef.Cost = 0;

            CardId[] expectedChoiceCardIdList = default;

            // �J�[�h�̑I�������̃e�X�g
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                Assert.Empty(c.PlayerIdList);
                TestUtil.AssertCollection(expectedChoiceCardIdList,
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                ));
            }

            var c = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef, SampleCards.DeadlyGoblin },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                expectedChoiceCardIdList = new[] { goblin.Id };

                await g.HitCreature(new Core.Entities.Effect.DamageContext(goblin, 5, goblin));

                Assert.Empty(c.Player1.Field.AllCards);
                Assert.Equal(0, goblin.Toughness);

                var beforeHandCardIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                // �����̃J�[�h���Ώ�
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandIdList = afterHandIdList.Except(beforeHandCardIdList).ToArray();

                // ��n�̃J�[�h����D�Ɉړ����Ă���
                Assert.Single(diffHandIdList);

                // �^�t�l�X���񕜂��Ă���
                var (_, diffHandCard) = c.CardRepository.TryGetById(diffHandIdList[0]);
                Assert.Equal(goblin.Id, diffHandCard.Id);
                Assert.Equal(goblin.BaseToughness, diffHandCard.Toughness);
            });
        }

        [Fact]
        public async Task Recycle()
        {
            var goblinDef = SampleCards.MechanicGoblin;
            goblinDef.Cost = 0;

            var testCardDef = SampleCards.Recycle;
            testCardDef.Cost = 0;

            CardId[] expectedChoiceCardIdList = default;

            // �J�[�h�̑I�������̃e�X�g
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                Assert.Empty(c.PlayerIdList);
                TestUtil.AssertCollection(expectedChoiceCardIdList,
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                ));
            }

            var c = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef, SampleCards.DeadlyGoblin },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                expectedChoiceCardIdList = new[] { goblin.Id };

                await g.HitCreature(new Core.Entities.Effect.DamageContext(goblin, 5, goblin));

                Assert.Empty(c.Player1.Field.AllCards);
                Assert.Equal(0, goblin.Toughness);

                var beforeHandCardIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                // �����̃J�[�h���Ώ�
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandIdList = afterHandIdList.Except(beforeHandCardIdList).ToArray();

                // ��n�̃J�[�h����D�Ɉړ����Ă���
                Assert.Single(diffHandIdList);

                // �R�s�[����D�ɉ����
                var (_, diffHandCard) = c.CardRepository.TryGetById(diffHandIdList[0]);
                Assert.NotEqual(goblin.Id, diffHandCard.Id);
                Assert.Equal(goblin.CardDefId, diffHandCard.CardDefId);
            });
        }

        [Fact]
        public async Task SimpleReborn()
        {
            var goblinDef = SampleCards.MechanicGoblin;
            goblinDef.Cost = 0;
            goblinDef.Toughness = 2;
            goblinDef.Abilities = new[] { CreatureAbility.Cover };

            var testCardDef = SampleCards.SimpleReborn;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef, SampleCards.DeadlyGoblin });

            // ��U
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                await g.DestroyCard(goblin);

                Assert.Empty(c.Player1.Field.AllCards);

                // �����̃J�[�h���Ώ�
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(c.Player1.Field.AllCards);
                Assert.Equal(goblinDef.Id, c.Player1.Field.AllCards[0].CardDefId);
                Assert.Equal(1, c.Player1.Field.AllCards[0].Toughness);

                return goblin;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await g.DestroyCard(goblin);

                Assert.Empty(c.Player2.Field.AllCards);

                // ����̃J�[�h���Ώ�
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(c.Player2.Field.AllCards);
                Assert.Equal(goblinDef.Id, c.Player2.Field.AllCards[0].CardDefId);
                Assert.Equal(1, c.Player2.Field.AllCards[0].Toughness);
            });
        }

        [Fact]
        public async Task Sword()
        {
            var goblin = SampleCards.Creature(0, "�S�u����", 1, 2);

            var testCardDef = SampleCards.Sword;
            testCardDef.Cost = 0;

            // �J�[�h�̑I�������̃e�X�g
            static ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    c.CardList.Select(c => c.Id).Take(1).ToArray(),
                    Array.Empty<CardDefId>()
                ));
            }

            var c = await TestUtil.InitTest(new[] { goblin, testCardDef },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
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
            var goblin = SampleCards.Creature(0, "�S�u����", 1, 2);

            var testCardDef = SampleCards.Shield;
            testCardDef.Cost = 0;

            // �J�[�h�̑I�������̃e�X�g
            static ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    c.CardList.Select(c => c.Id).Take(1).ToArray(),
                    Array.Empty<CardDefId>()
                ));
            }

            var c = await TestUtil.InitTest(new[] { goblin, testCardDef },
                TestUtil.GameMasterOptions(
                EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, goblinCard.PowerBuff);
                Assert.Equal(1, goblinCard.ToughnessBuff);
            });
        }

        [Fact]
        public async Task HitOrHeal()
        {
            var testCardDef1 = SampleCards.HitOrHeal;
            testCardDef1.Cost = 0;

            var expectedChoiceCardDefList = new[] { SampleCards.Hit, SampleCards.Heal };
            CardDefId choiceCardDefId = default;

            // �J�[�h�̑I�������̃e�X�g
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                Assert.Empty(c.PlayerIdList);
                Assert.Empty(c.CardList);
                TestUtil.AssertCollection(
                    expectedChoiceCardDefList.Select(c => c.Name).ToArray(),
                    c.CardDefList.Select(c => c.Name).ToArray());

                choiceCardDefId = c.CardDefList[0].Id;

                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    Array.Empty<CardId>(),
                    new[] { choiceCardDefId }
                ));
            }

            var c = await TestUtil.InitTest(
                new[] { testCardDef1, SampleCards.Hit, SampleCards.Heal },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHandIds = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = c.Player1.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Single(diffHands);
                Assert.Equal(choiceCardDefId, diffHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task Copy()
        {
            var testCardDef1 = SampleCards.Copy;
            testCardDef1.Cost = 0;

            var goblin1Def = SampleCards.Goblin;
            goblin1Def.Cost = 0;
            var goblin2Def = SampleCards.QuickGoblin;
            goblin2Def.Cost = 0;

            CardId[] expectedChoiceCardIdList = default;
            CardDefId answerCardDefId = default;

            // �J�[�h�̑I�������̃e�X�g
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                TestUtil.AssertCollection(expectedChoiceCardIdList, c.CardList.Select(v => v.Id).ToArray());

                answerCardDefId = c.CardList[0].CardDefId;

                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                ));
            }

            var c = await TestUtil.InitTest(new[] { testCardDef1, goblin1Def, goblin2Def },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var c1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin2Def.Id);

                expectedChoiceCardIdList = new[] { c1.Id, c2.Id, c3.Id };
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHandIds = c.Player2.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = c.Player2.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Single(diffHands);
                Assert.Equal(answerCardDefId, diffHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task DoubleCopy()
        {
            var testCardDef1 = SampleCards.DoubleCopy;
            testCardDef1.Cost = 0;

            var goblin1Def = SampleCards.Goblin;
            goblin1Def.Cost = 0;
            var goblin2Def = SampleCards.QuickGoblin;
            goblin2Def.Cost = 0;

            CardId[] expectedChoiceCardIdList = default;
            CardDefId answerCardDefId = default;

            // �J�[�h�̑I�������̃e�X�g
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                TestUtil.AssertCollection(expectedChoiceCardIdList, c.CardList.Select(v => v.Id).ToArray());

                answerCardDefId = c.CardList[0].CardDefId;

                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { c.CardList[0].Id },
                    Array.Empty<CardDefId>()
                ));
            }

            var c = await TestUtil.InitTest(new[] { testCardDef1, goblin1Def, goblin2Def },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var c1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin2Def.Id);

                expectedChoiceCardIdList = new[] { c1.Id, c2.Id, c3.Id };
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHandIds = c.Player2.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = c.Player2.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Equal(2, diffHands.Length);
                Assert.Equal(answerCardDefId, diffHands[0].CardDefId);
                Assert.Equal(answerCardDefId, diffHands[1].CardDefId);
            });
        }

        [Fact]
        public async Task FirstAttack()
        {
            var testCardDef1 = SampleCards.FirstAttack;
            testCardDef1.Cost = 0;

            var testCardDef2 = SampleCards.SecondAttack;

            PlayerId[] expectedChoicePlayerIdList = default;
            Card[] expectedChoiceCardList = default;

            // �J�[�h�̑I�������̃e�X�g
            ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                TestUtil.AssertCollection(expectedChoicePlayerIdList, c.PlayerIdList);
                TestUtil.AssertCollection(expectedChoiceCardList, c.CardList);

                return ValueTask.FromResult(new ChoiceAnswer(
                    c.PlayerIdList.Take(1).ToArray(),
                    Array.Empty<CardId>(),
                    Array.Empty<CardDefId>()
                ));
            }

            var c = await TestUtil.InitTest(new[] { testCardDef1, testCardDef2 },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            expectedChoicePlayerIdList = new[] { c.Player2.Id };
            expectedChoiceCardList = Array.Empty<Card>();

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHandIds = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var beforeHp = c.Player2.CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffCards = c.Player1.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Single(diffCards);
                Assert.Equal(testCardDef2.Id, diffCards[0].CardDefId);

                Assert.Equal(beforeHp - 1, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task HolyShield()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 2, 3, numTurnsToCanAttack: 0);

            var testCardDef = SampleCards.HolyShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // ��U
            var goblin3 = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �S�u�����œG���U��
                // �U���������̓_���[�W���󂯂Ȃ�
                await g.AttackToCreature(pId, goblin3.Id, goblin1.Id);
                Assert.Equal(goblinDef.Toughness - goblinDef.Power, goblin1.Toughness);
                Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);

                return goblin3;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ���̃^�[���Ȃ̂ŁA���݂��Ƀ_���[�W���󂯂�
                await g.AttackToCreature(pId, goblin2.Id, goblin3.Id);
                Assert.Equal(goblinDef.Toughness - goblinDef.Power, goblin2.Toughness);
                Assert.Equal(goblinDef.Toughness - goblinDef.Power, goblin3.Toughness);
            });
        }

        [Fact]
        public async Task ChangeHands()
        {
            var testCardDef = SampleCards.ChangeHands;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
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
        public async Task Ramp()
        {
            var testCardDef = SampleCards.Ramp;
            testCardDef.Cost = 1;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(1, g.ActivePlayer.MaxMp);
                Assert.Equal(1, g.ActivePlayer.CurrentMp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �ő�l���P������B���g�p��MP�͑����Ȃ��B
                Assert.Equal(2, g.ActivePlayer.MaxMp);
                Assert.Equal(0, g.ActivePlayer.CurrentMp);
            });
        }

        [Fact]
        public async Task BounceHand()
        {
            var testCardDef = SampleCards.BounceHand;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            CardId choiceCardId = default;
            ValueTask<ChoiceAnswer> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { choiceCardId },
                    Array.Empty<CardDefId>()));
            }

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                    ));

            // �����̃J�[�h��߂�
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                choiceCardId = goblin.Id;

                var beforeHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var afterHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforeHandsIdList)
                    .ToArray();

                Assert.Empty(p.Field.AllCards);

                Assert.Single(diffHandsIdList);
                Assert.Equal(goblin.Id, diffHandsIdList[0]);

                return await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
            });

            // ����̃J�[�h��߂�
            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var op = g.GetOpponent(pid);

                choiceCardId = goblin.Id;

                var beforeHandsIdList = op.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var afterHandsIdList = op.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforeHandsIdList)
                    .ToArray();

                Assert.Empty(op.Field.AllCards);

                Assert.Single(diffHandsIdList);
                Assert.Equal(goblin.Id, diffHandsIdList[0]);
            });
        }

        [Fact]
        public async Task BounceDeck()
        {
            var testCardDef = SampleCards.BounceDeck;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            CardId choiceCardId = default;
            ValueTask<ChoiceAnswer> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { choiceCardId },
                    Array.Empty<CardDefId>()));
            }

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                    ));

            // �����̃J�[�h��߂�
            var p1Goblin = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                choiceCardId = goblin.Id;

                var beforeDeckIdList = p.Deck.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var afterDeckIdList = p.Deck.AllCards.Select(c => c.Id).ToArray();
                var diffDeckIdList = afterDeckIdList
                    .Except(beforeDeckIdList)
                    .ToArray();

                Assert.Empty(p.Field.AllCards);

                // �f�b�L�Ɉړ����Ă���
                Assert.Single(diffDeckIdList);
                Assert.Equal(goblin.Id, diffDeckIdList[0]);

                return await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
            });

            // ����̃J�[�h��߂�
            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var op = g.GetOpponent(pid);

                choiceCardId = p1Goblin.Id;

                var beforeDeckIdList = op.Deck.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var afterDeckIdList = op.Deck.AllCards.Select(c => c.Id).ToArray();
                var diffDeckIdList = afterDeckIdList
                    .Except(beforeDeckIdList)
                    .ToArray();

                Assert.Empty(op.Field.AllCards);

                // �f�b�L�Ɉړ����Ă���
                Assert.Single(diffDeckIdList);
                Assert.Equal(p1Goblin.Id, diffDeckIdList[0]);
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

            Card[] candidateCardList = default;
            CardId choiceCardId = default;
            ValueTask<ChoiceAnswer> assertAskAction(PlayerId _, ChoiceCandidates c, int i)
            {
                Assert.Equal(1, i);
                Assert.Empty(c.PlayerIdList);
                TestUtil.AssertCollection(
                    candidateCardList.Select(c => c.Id).ToArray(),
                    c.CardList.Select(c => c.Id).ToArray());
                Assert.Empty(c.CardDefList);

                return ValueTask.FromResult(new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { choiceCardId },
                    Array.Empty<CardDefId>()));
            }

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                    ));

            // ��U
            var (goblin1, goblin11) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin11);
            });

            candidateCardList = new[] { goblin1, goblin11 };

            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
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

            var c = await TestUtil.InitTest(new[] { goblinDef, notGoblinDef, testCardDef });

            // ��U
            var (goblin1, notGoblin1) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var notGoblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, notGoblinDef.Id);

                return (goblin1, notGoblin1);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
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

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(goblin1.BaseToughness - 1, goblin1.Toughness);
                Assert.Equal(goblin2.BaseToughness - 1, goblin1.Toughness);
                Assert.Equal(TestUtil.TestRuleBook.InitialPlayerHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task Search()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 4;
            var testCardDef = SampleCards.Search;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef },
                Enumerable.Repeat(goblinDef, TestUtil.TestRuleBook.MaxNumDeckCards));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHands = c.Player1.Hands.AllCards.ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHands = c.Player1.Hands.AllCards.ToArray();
                var diffHands = afterHands.Where(a => !beforeHands.Any(b => b.Id == a.Id)).ToArray();

                Assert.Single(diffHands);
                Assert.Equal(goblinDef.Id, diffHands[0].CardDefId);
                Assert.Equal(goblinDef.Cost * 0.5, diffHands[0].Cost);
            });
        }

        [Fact]
        public async Task OldShield_�N���[�`���[�̖h�䎞()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 2, 2);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // �N���[�`���[��1�_���[�W�����󂯂Ȃ�
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
                // �j�󂳂��
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_�v���C���[�̖h�䎞()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 2, 2);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await c.GameMaster.AttackToPlayer(pId, goblinCard2.Id, c.Player1.Id);

                // �v���C���[��2�_���[�W�󂯂�
                Assert.Equal(TestUtil.TestRuleBook.InitialPlayerHp - 2, c.Player1.CurrentHp);
                // �j�󂳂�Ȃ�
                Assert.Equal(ZoneName.Field, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_�U����()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Power = 2;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var goblinCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // ���R�N���[�`���[���ΏۂɂȂ�
                Assert.Equal(goblinCard.BaseToughness - goblinDef.Power, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - (goblinDef.Power - 1), goblinCard2.Toughness);
                // �j�󂳂��
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_0�_���[�W()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Power = 1;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var goblinCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblinCard;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 1���͔j�󂳂�邪�A����1���͔j�󂳂�Ȃ�
                Assert.Equal(2, op.Field.Count);
            });
        }

        [Fact]
        public async Task OldWall_�v���C���[���U��()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 2, 2);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var testcard = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = c.Player1.CurrentHp;
                await c.GameMaster.AttackToPlayer(pId, goblinCard.Id, c.Player1.Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_�N���[�`���[���U��()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 2, 2);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var (goblin, testcard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblin, testcard);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = c.Player1.CurrentHp;
                await c.GameMaster.AttackToCreature(pId, goblin2.Id, goblin.Id);

                // 1�_���[�W�����󂯂Ȃ�
                Assert.Equal(goblin.BaseToughness - 1, goblin.Toughness);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_�U����()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Power = 2;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var goblinCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // �U�������ΏۂɂȂ�
                Assert.Equal(goblinCard.BaseToughness - goblinDef.Power, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - (goblinDef.Power - 1), goblinCard2.Toughness);
                // �j�󂳂��
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task EmergencyFood()
        {
            var goblin = SampleCards.Goblin;
            goblin.Cost = 2;

            var testCardDef = SampleCards.EmergencyFood;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { goblin, testCardDef },
                Enumerable.Repeat(goblin, 40)
                );

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ���O�Ƀ_���[�W
                c.Player1.Damage(5);

                var beforeHp = c.Player1.CurrentHp;
                var beforeHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ��D��1�������Ă���
                Assert.Equal(beforeHandIdList.Length - 1, c.Player1.Hands.AllCards.Count);

                // �̂Ă��J�[�h�̃R�X�g�����C�t���񕜂��Ă���
                Assert.Equal(beforeHp + goblin.Cost, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task GoblinStatue()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 1, 10);

            var testCardDef = SampleCards.GoblinStatue;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // ��U
            var goblin3 = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // 30����n�ɑ���
                foreach (var _ in Enumerable.Range(0, 30))
                {
                    var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                    await g.DestroyCard(goblin);
                }

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblin3;
            });

            // ����v���C���[�ƃN���[�`���[��6�_���[�W
            Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp - 6, c.Player1.CurrentHp);
            Assert.Equal(goblin1.BaseToughness - 6, goblin1.Toughness);
            Assert.Equal(goblin2.BaseToughness - 6, goblin2.Toughness);

            // �����v���C���[�Ǝ����N���[�`���[�̓_���[�W���󂯂Ȃ�
            Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp, c.Player2.CurrentHp);
            Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);
        }

        [Fact]
        public async Task HolyStatue()
        {
            var goblinDef = SampleCards.Creature(0, "�S�u����", 1, 2);

            var testCardDef = SampleCards.HolyStatue;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ��ɏ�ɂ����S�u����2�̂��C�������
                Assert.True(goblinCard.PowerBuff == 0 && goblinCard.ToughnessBuff == 1);
                Assert.True(goblinCard2.PowerBuff == 0 && goblinCard2.ToughnessBuff == 1);

                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // ���Ƃɏ�ɏo���S�u�����������C�������
                Assert.True(goblinCard.PowerBuff == 0 && goblinCard.ToughnessBuff == 1);
                Assert.True(goblinCard2.PowerBuff == 0 && goblinCard2.ToughnessBuff == 1);
                Assert.True(goblinCard3.PowerBuff == 0 && goblinCard3.ToughnessBuff == 1);
            });
        }

        [Fact]
        public async Task WarStatue()
        {
            var testCardDef = SampleCards.VictoryRoad;
            testCardDef.Cost = 0;

            var tokenCardDef = SampleCards.VictoryStatue;

            var c = await TestUtil.InitTest(new[] { testCardDef, tokenCardDef });

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            Assert.Contains(testCard, c.Player1.Field.AllCards);

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Single(c.Player1.Field.AllCards);
                Assert.Equal(tokenCardDef.Id, c.Player1.Field.AllCards[0].CardDefId);
            });
        }

        [Fact]
        public async Task VictoryRoad()
        {
            var testCardDef = SampleCards.VictoryStatue;
            testCardDef.Cost = 0;
            testCardDef.IsToken = false;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.False(g.GameOver);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.False(g.GameOver);
            });

            Assert.False(c.GameMaster.GameOver);

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.True(g.GameOver);
                Assert.Equal(pId, g.GetWinner().Id);
            });
        }

        [Fact]
        public async Task Faceless()
        {
            var testCardDef = SampleCards.Faceless;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ��U2
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ��U3
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // �c��MP��-2����Ă���
                Assert.Equal(2, c.Player1.UsedMp);
            });
        }

        [Fact]
        public async Task Prophet()
        {
            var testCardDef = SampleCards.Prophet;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ���̃^�[��������܂ł͂O�̂܂�
            Assert.Equal(0, testCard.Power);

            // ��U2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // �^�[���J�n����7�ɂȂ�
                Assert.Equal(7, testCard.Power);
            });
        }

        [Fact]
        public async Task Psycho_�U���ɂ��_���[�W()
        {
            var testCardDef = SampleCards.Psycho;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.AttackToCreature(pId, goblin.Id, testCard.Id);

                // �U���ɂ��_���[�W��0�ɂ���
                Assert.Equal(testCard.BaseToughness, testCard.Toughness);

                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.AttackToCreature(pId, goblin2.Id, testCard.Id);

                // 2�x�ڂ�0�ɂł��Ȃ�
                Assert.Equal(testCard.BaseToughness - 1, testCard.Toughness);
            });
        }

        [Fact]
        public async Task Psycho_�U���ȊO�ɂ��_���[�W()
        {
            var testCardDef = SampleCards.Psycho;
            testCardDef.Cost = 0;

            var spellDef = SampleCards.SelectDamage;
            spellDef.Cost = 0;

            CardId choiceCardId = default;
            ValueTask<ChoiceAnswer> askCardAction(PlayerId playerId, ChoiceCandidates choiceCandidates, int n)
            {
                var answer = new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { choiceCardId },
                    Array.Empty<CardDefId>()
                    );

                return ValueTask.FromResult(answer);
            }

            var c = await TestUtil.InitTest(new[] { testCardDef, spellDef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(
                        AskCardAction: askCardAction)));

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            choiceCardId = testCard.Id;

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                // �U���ɂ��_���[�W��0�ɂ���
                Assert.Equal(testCard.BaseToughness, testCard.Toughness);

                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                // 2�x�ڂ�0�ɂł��Ȃ�
                Assert.Equal(testCard.BaseToughness - 1, testCard.Toughness);
            });
        }

        [Fact]
        public async Task Nightmare()
        {
            var testCardDef = SampleCards.Nightmare;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ���̃^�[��������܂ł͂��Ƃ̂܂�
            Assert.Equal(testCard.BasePower, testCard.Power);

            // ��U2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // �^�[���J�n����2�{�ɂȂ�
                Assert.Equal(testCard.BasePower * 2, testCard.Power);
            });

            // ��U2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ���̃^�[��������܂ł͂��Ƃ̂܂�
            Assert.Equal(testCard.BasePower * 2, testCard.Power);

            // ��U3
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // �^�[���J�n���ɂ����2�{�ɂȂ�
                Assert.Equal(testCard.BasePower * 2 * 2, testCard.Power);
            });
        }

        [Fact]
        public async Task Disaster()
        {
            var testCardDef = SampleCards.Disaster;
            testCardDef.Cost = 0;

            var tokenDef = SampleCards.Gnoll;

            var spellDef = SampleCards.SelectDamage;
            spellDef.Cost = 0;

            CardId choiceCardId = default;
            ValueTask<ChoiceAnswer> askCardAction(PlayerId playerId, ChoiceCandidates choiceCandidates, int n)
            {
                var answer = new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { choiceCardId },
                    Array.Empty<CardDefId>()
                    );

                return ValueTask.FromResult(answer);
            }

            var c = await TestUtil.InitTest(new[] { testCardDef, tokenDef, spellDef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(
                        AskCardAction: askCardAction)));

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            choiceCardId = testCard.Id;

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ���̎��_�ł͏��1��
                Assert.Equal(1, c.Player1.Field.Count);

                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                // �_���[�W���󂯂�ƁA�g�[�N���������̏�ɒǉ�����
                Assert.Equal(2, c.Player1.Field.Count);
                TestUtil.AssertCollection(
                    new[] { testCardDef.Id, tokenDef.Id },
                    c.Player1.Field.AllCards.Select(c => c.CardDefId).ToArray());
            });
        }

        [Fact]
        public async Task Virus()
        {
            var testCardDef = SampleCards.Virus;
            testCardDef.Cost = 0;

            var goblinDefp3 = SampleCards.Goblin;
            goblinDefp3.Cost = 0;
            goblinDefp3.Power = 3;

            var goblinDefp4 = SampleCards.Goblin;
            goblinDefp4.Cost = 0;
            goblinDefp4.Power = 4;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDefp3, goblinDefp4 });

            // ��U
            var (goblinP3, goblinP4) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinP3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDefp3.Id);
                var goblinP4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDefp4.Id);

                return (goblinP3, goblinP4);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ����̏��2��
                Assert.Equal(2, c.Player1.Field.Count);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ����̏�̃p���[4�ȏ�̃N���[�`���[�����j��
                Assert.Equal(1, c.Player1.Field.Count);
                Assert.Equal(goblinDefp3.Id, c.Player1.Field.AllCards[0].CardDefId);
            });

            // ��U2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // �h���[�����J�[�h�̃p���[��4�ȏ�Ȃ�j�󂷂�
            });
        }

        [Fact]
        public async Task Exclude()
        {
            var testCardDef = SampleCards.Exclude;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            CardId choiceCardId = default;
            ValueTask<ChoiceAnswer> askCardAction(PlayerId playerId, ChoiceCandidates choiceCandidates, int n)
            {
                var answer = new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    new[] { choiceCardId },
                    Array.Empty<CardDefId>()
                    );

                return ValueTask.FromResult(answer);
            }

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(
                        AskCardAction: askCardAction)));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                choiceCardId = goblin.Id;

                // ���O�ς݃J�[�h�Ȃ�
                Assert.Single(p.Field.AllCards);
                Assert.Empty(p.Excludes);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Empty(p.Field.AllCards);
                Assert.Single(p.Excludes);
                Assert.Equal(goblinDef.Id, p.Excludes[0].Id);
            });
        }

        [Fact]
        public async Task DDObserver()
        {
            var testCardDef = SampleCards.DDObserver;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ��ɏo��ȑO�ɏ��O����Ă��Ă��e���Ȃ�
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // �U���͂͂��Ƃ̂܂�
                Assert.Equal(testCard.BasePower, testCard.Power);

                await g.ExcludeCard(goblin2);

                // �J�[�h�����O���ꂽ�̂ōU���͂�1�オ��
                Assert.Equal(testCard.BasePower + 1, testCard.Power);

                return testCard;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // �܂��U���͂͂��Ƃ̂܂�
                Assert.Equal(testCard.BasePower + 1, testCard.Power);

                await g.ExcludeCard(goblin);

                // ����̃J�[�h�����O����Ă��U���͂��オ��
                Assert.Equal(testCard.BasePower + 2, testCard.Power);
            });
        }

        [Fact]
        public async Task DDVisitor()
        {
            var testCardDef = SampleCards.DDVisitor;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ���O�ς�:0 �Ȃ̂ŁA�x�[�X�̂܂�
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard.BasePower, testCard.Power);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin);

                // ���O�ς�:1 �Ȃ̂ŁA+1����Ă���
                var testCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard2.BasePower + 1, testCard2.Power);

                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin3);

                // ���O�ς�:2 �Ȃ̂ŁA+2����Ă���
                var testCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard3.BasePower + 2, testCard3.Power);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ����̏��O�ςݖ����͉e���Ȃ�
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard.BasePower, testCard.Power);
            });
        }

        [Fact]
        public async Task ReturnFromDD()
        {
            var testCardDef = SampleCards.ReturnFromDD;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                // �Ȃɂ����O����Ă��Ȃ��̂ŁA�Ȃɂ��N���Ȃ�
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Empty(p.Field.AllCards);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin);

                Assert.Empty(p.Field.AllCards);

                // ���O�ς݃J�[�h�̃R�s�[����ɒǉ������
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Single(p.Field.AllCards);
                Assert.Equal(goblinDef.Id, p.Field.AllCards[0].CardDefId);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                // �����̏��O�ς݃J�[�h�������ΏۂȂ̂ŁA�Ȃɂ��N���Ȃ�
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Empty(p.Field.AllCards);
            });
        }

        [Fact]
        public async Task DDTransaction()
        {
            var testCardDef = SampleCards.DDTransaction;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var goblinC2Def = SampleCards.Goblin;
            goblinC2Def.Cost = 2;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, goblinC2Def });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                // ��D��2�R�X�g�̃J�[�h�����ɂ���i���O�����J�[�h���Œ肷�邽�߁j
                await g.Discard(p.Id, p.Hands.AllCards.Select(c => c.Id).ToArray());
                await g.GenerateNewCard(goblinC2Def.Id, new Zone(pId, ZoneName.Hand), default);

                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                Assert.Equal(4, p.Field.AllCards.Count);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ��D����1�����O�����
                Assert.Empty(p.Hands.AllCards);

                // ��̃J�[�h��2���j�󂳂��i���O�����J�[�h���R�X�g=2�Ȃ̂Łj
                Assert.Equal(2, p.Field.AllCards.Count);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_�U������()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(p.Field.AllCards);
                Assert.Single(op.Field.AllCards);

                Assert.Empty(p.Excludes);
                Assert.Empty(op.Excludes);

                var status = await g.AttackToCreature(pId, testCard.Id, goblin.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);

                // �U�������������ꂽ�����A�������O�����B
                Assert.Empty(p.Field.AllCards);
                Assert.Empty(op.Field.AllCards);

                Assert.Single(p.Excludes);
                Assert.Single(op.Excludes);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_�U�������()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                Assert.Single(p.Field.AllCards);
                Assert.Single(op.Field.AllCards);

                Assert.Empty(p.Excludes);
                Assert.Empty(op.Excludes);

                var status = await g.AttackToCreature(pId, goblin.Id, testCard.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);

                // �U�������������ꂽ�����A�������O�����B
                Assert.Empty(p.Field.AllCards);
                Assert.Empty(op.Field.AllCards);

                Assert.Single(p.Excludes);
                Assert.Single(op.Excludes);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_�U������đ��肪����()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Toughness = 1;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                Assert.Single(p.Field.AllCards);
                Assert.Single(op.Field.AllCards);

                Assert.Empty(p.Excludes);
                Assert.Empty(op.Excludes);

                var status = await g.AttackToCreature(pId, goblin.Id, testCard.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);

                // �U�������������ꂽ�����A�������O�����B
                Assert.Empty(p.Field.AllCards);
                Assert.Empty(op.Field.AllCards);

                Assert.Single(p.Excludes);
                Assert.Single(op.Excludes);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_�v���C���[�֍U������()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(p.Field.AllCards);
                Assert.Empty(p.Excludes);

                var status = await g.AttackToPlayer(pId, testCard.Id, op.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);

                // �v���C���[�֍U�������ꍇ�́A���O����Ȃ�
                Assert.Single(p.Field.AllCards);
                Assert.Empty(p.Excludes);
            });
        }

        [Fact]
        public async Task BreakCover()
        {
            var testCardDef = SampleCards.BreakCover;
            testCardDef.Cost = 0;

            var coverCardDef = SampleCards.Goblin;
            coverCardDef.Cost = 0;
            coverCardDef.Abilities = new[] { CreatureAbility.Cover };

            var noCoverCardDef = SampleCards.Goblin;
            noCoverCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, coverCardDef, noCoverCardDef });

            // ��U
            var (cover, cover2, nocover, nocover2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var coverCard = await TestUtil.NewCardAndPlayFromHand(g, pId, coverCardDef.Id);
                var coverCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, coverCardDef.Id);
                var noCoverCard = await TestUtil.NewCardAndPlayFromHand(g, pId, noCoverCardDef.Id);
                var noCoverCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, noCoverCardDef.Id);

                return (coverCard, coverCard2, noCoverCard, noCoverCard2);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var mycover = await TestUtil.NewCardAndPlayFromHand(g, pId, coverCardDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ����̏�̃J�o�[��������J�o�[���Ȃ��Ȃ��Ă���
                Assert.Empty(cover.Abilities);
                Assert.Empty(cover2.Abilities);

                // ����̏�̃J�o�[����������1�_���[�W
                Assert.Equal(cover.BaseToughness - 1, cover.Toughness);
                Assert.Equal(cover2.BaseToughness - 1, cover2.Toughness);
                Assert.Equal(nocover.BaseToughness, nocover.Toughness);
                Assert.Equal(nocover2.BaseToughness, nocover2.Toughness);

                // �����̏�̃J�o�[�͂��̂܂�
                Assert.Contains(CreatureAbility.Cover, mycover.Abilities);
                Assert.Equal(mycover.BaseToughness, mycover.Toughness);
            });
        }

        [Fact]
        public async Task MagicObject()
        {
            var testCardDef = SampleCards.MagicObject;
            testCardDef.Cost = 0;

            var sorceryDef = SampleCards.Sorcery(0, "", "");

            var c = await TestUtil.InitTest(new[] { testCardDef, sorceryDef });

            // ��U
            var testcard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(0, testcard.GetCounter("����"));
                Assert.Equal(testcard.BasePower, testcard.Power);

                // ���@���g���ƃJ�E���^�[��1���B
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(1, testcard.GetCounter("����"));

                // �J�E���^�[���������+1/+0�����
                Assert.Equal(testcard.BasePower + 1, testcard.Power);

                // �J�E���^�[����������-1/+0�����
                await g.ModifyCounter(testcard, "����", -1);
                Assert.Equal(testcard.BasePower, testcard.Power);

                // �J�E���^�[���������+1/+0�����
                await g.ModifyCounter(testcard, "����", 1);
                Assert.Equal(testcard.BasePower + 1, testcard.Power);

                // 2�J�E���^�[���������+2/+0�����
                await g.ModifyCounter(testcard, "����", 2);
                Assert.Equal(testcard.BasePower + 3, testcard.Power);

                // 2�J�E���^�[����������+2/+0�����
                await g.ModifyCounter(testcard, "����", -2);
                Assert.Equal(testcard.BasePower + 1, testcard.Power);

                return testcard;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ���肪���@���g���Ă��J�E���^�[�����
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(2, testcard.GetCounter("����"));
            });
        }

        [Fact]
        public async Task MagicMonster()
        {
            var testCardDef = SampleCards.MagicMonster;

            var sorceryDef = SampleCards.Sorcery(0, "", "");

            var c = await TestUtil.InitTest(new[] { testCardDef, sorceryDef });

            // ��U
            var testcard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testcard = await g.GenerateNewCard(testCardDef.Id, new Zone(pId, ZoneName.Hand), null);
                Assert.Equal(0, testcard.GetCounter("����"));
                Assert.Equal(testcard.BasePower, testcard.Power);

                // ���@���g���ƃJ�E���^�[��1���B
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(1, testcard.GetCounter("����"));

                // �J�E���^�[���������R�X�g��-1
                Assert.Equal(testcard.BaseCost - 1, testcard.Cost);

                // ���@���g��Ȃ��Ă��J�E���^�[���������R�X�g��-1
                await g.ModifyCounter(testcard, "����", 1);
                Assert.Equal(testcard.BaseCost - 2, testcard.Cost);

                // 2�J�E���^�[���������R�X�g��-2
                await g.ModifyCounter(testcard, "����", 2);
                Assert.Equal(testcard.BaseCost - 4, testcard.Cost);

                return testcard;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ���肪���@���g�����ꍇ�̓J�E���^�[�����Ȃ�
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(4, testcard.GetCounter("����"));
            });
        }

        [Fact]
        public async Task BeginnerSorcerer()
        {
            var testCardDef = SampleCards.BeginnerSorcerer;
            testCardDef.Cost = 0;
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Annotations = new[] { ":����" };

            CardId[] expectedAskCardLsit = default;

            ValueTask<ChoiceAnswer> assertAskAction(PlayerId _, ChoiceCandidates c, int __)
            {
                return ValueTask.FromResult(new ChoiceAnswer(
                    c.PlayerIdList.Take(1).ToArray(),
                    expectedAskCardLsit,
                    Array.Empty<CardDefId>()));
            }

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef },
                TestUtil.GameMasterOptions(
                    EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                    ));

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                Assert.Equal(0, goblin.GetCounter("����"));

                expectedAskCardLsit = new[] { goblin.Id };

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �J�E���^�[������Ă���
                Assert.Equal(2, goblin.GetCounter("����"));
            });
        }

        [Fact]
        public async Task GreatSorcerer()
        {
            var testCardDef = SampleCards.GreatSorcerer;
            var testCardDef_cost0 = SampleCards.GreatSorcerer;
            testCardDef_cost0.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, testCardDef_cost0 },
                Enumerable.Repeat(testCardDef_cost0, 20).ToArray()
                );

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var beforeHandIdList = p.Hands.AllCards.ToArray();

                var testcard_cost0 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef_cost0.Id);

                // ���Ƃ̎�D�����ׂď��O����Ă���
                Assert.True(beforeHandIdList.All(c => c.Zone.ZoneName == ZoneName.Excluded));

                // �V���Ɏ�D��5�������Ă���
                Assert.Equal(5, p.Hands.Count);

                // �V���Ȏ�D���ׂĂɃJ�E���^�[��5�u����Ă���
                Assert.True(p.Hands.AllCards.All(c => c.GetCounter("����") == 5));
            });
        }

        [Fact]
        public async Task UltraMagic()
        {
            var testCardDef = SampleCards.UltraMagic;
            testCardDef.Cost = 0;
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeOpHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �J�E���^�[���Ȃ��̂ő����1�_���[�W
                Assert.Equal(beforeOpHp - 1, op.CurrentHp);

                await g.ModifyCounter(goblin1, "����", 1);

                beforeOpHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �J�E���^�[��1�Ȃ̂ő����2�_���[�W
                Assert.Equal(beforeOpHp - 2, op.CurrentHp);
                // �J�E���^�[���Ȃ��Ȃ��Ă���
                Assert.Equal(0, goblin1.GetCounter("����"));

                await g.ModifyCounter(goblin1, "����", 1);
                await g.ModifyCounter(goblin2, "����", 1);

                beforeOpHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // �J�E���^�[��2�Ȃ̂ő����3�_���[�W
                Assert.Equal(beforeOpHp - 3, op.CurrentHp);
                // �J�E���^�[���Ȃ��Ȃ��Ă���
                Assert.Equal(0, goblin1.GetCounter("����"));
                Assert.Equal(0, goblin2.GetCounter("����"));
            });
        }

        [Fact]
        public async Task Investment()
        {
            var testCardDef = SampleCards.Investment;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, });

            // ��U
            var beforeNumHands = 0;
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                beforeNumHands = c.Player1.Hands.Count;
            });

            // �܂��h���[�ł��Ȃ�
            Assert.Equal(beforeNumHands, c.Player1.Hands.Count);

            await TestUtil.Turn(c.GameMaster, (g, pId) => { });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                beforeNumHands = c.Player1.Hands.Count;
            });

            // �����Ńh���[�ł���
            Assert.Equal(beforeNumHands + 2, c.Player1.Hands.Count);

            await TestUtil.Turn(c.GameMaster, (g, pId) => { });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                beforeNumHands = c.Player1.Hands.Count;
            });

            // �����h���[�ł��Ȃ�
            Assert.Equal(beforeNumHands, c.Player1.Hands.Count);
        }
    }
}
