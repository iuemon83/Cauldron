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

                // 速攻持ちなので攻撃できる
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

                // 1ターンに2回まで攻撃できる
                Assert.Equal(GameMasterStatusCode.OK, status);

                status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 1ターンに2回まで攻撃できる
                Assert.Equal(GameMasterStatusCode.OK, status);

                status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 3回目は失敗
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

                // 2ターン経過するまで攻撃できない
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                return testcard;
            });

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 2ターン経過するまで攻撃できない
                Assert.Equal(GameMasterStatusCode.CantAttack, status);
            });

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
            });

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var status = await g.AttackToPlayer(pid, testcard.Id, c.Player2.Id);

                // 2ターン経過したので攻撃できる
                Assert.Equal(GameMasterStatusCode.OK, status);
            });
        }

        [Fact]
        public async Task DoubleStrikeGoblin_攻撃する()
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

                // 先制攻撃で倒し切るので自分はダメージを受けない
                await g.AttackToCreature(pid, testCard.Id, goblinT1.Id);

                // 自分はダメージなし
                Assert.Equal(testCard.BaseToughness, testCard.Toughness);
                // 相手は倒す
                Assert.Equal(0, goblinT1.Toughness);

                var testCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // 相手を倒しきれないのでダメージを受ける
                await g.AttackToCreature(pid, testCard2.Id, goblinT3.Id);

                Assert.Equal(0, testCard2.Toughness);
                Assert.Equal(0, goblinT3.Toughness);
            });
        }

        [Fact]
        public async Task DoubleStrikeGoblin_攻撃される()
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

                // 先制攻撃で倒し切るので自分はダメージを受けない
                await g.AttackToCreature(pid, goblinT1.Id, testCard.Id);

                Assert.Equal(testCard.BaseToughness, testCard.Toughness);

                // 相手を倒しきれないのでダメージを受ける
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
                    // 自分で出したカード以外
                    .Where(id => id != goblin.Id)
                    .ToArray();

                var afterDeckCount = c.Player1.Deck.Count;

                //TODO たまたま2枚とも手札に来てるとテストが失敗するぞ！！
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
        /// ゴブリンと関係ないカードで発動しないテスト
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GoblinFollower2()
        {
            var testCardDef = SampleCards.GoblinFollower;
            testCardDef.Cost = 0;

            var notGoblinDef = SampleCards.Goblin;
            notGoblinDef.Cost = 0;
            notGoblinDef.Name = "テスト";

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
                    // 自分で出したカード以外
                    .Where(id => id != goblin.Id)
                    .ToArray();

                var afterDeckCount = c.Player1.Deck.Count;

                Assert.Empty(diffFieldIdList);
                Assert.Equal(0, beforeDeckCount - afterDeckCount);
            });
        }

        /// <summary>
        /// 名前にゴブリンが含まれるがクリーチャーでないカードで発動しないテスト
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
                    // 自分で出したカード以外
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

                // 攻撃されると攻撃したカードを持ち主の手札に戻す
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforpHandsIdList)
                    .ToArray();

                // 攻撃したカードが手札に戻っている
                Assert.Empty(p.Field.AllCards);

                Assert.Single(diffHandsIdList);
                Assert.Equal(p2Goblin.Id, diffHandsIdList[0]);

                // 攻撃したときは効果が発動しない
                await g.DestroyCard(p1TestCard);
                var p2TestCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforpOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2TestCard.Id, p1Goblin.Id));

                var afterOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                // 効果が発動しないので相手フィールド上のカードに変更はない
                TestUtil.AssertCollection(beforpOpFieldIdList, afterOpFieldIdList);
            });
        }

        [Fact]
        public async Task MagicShieldGoblin_攻撃カードが破壊された場合()
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

                // 攻撃されると攻撃したカードを持ち主の手札に戻す
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforpHandsIdList)
                    .ToArray();

                // 攻撃したカードは破壊され墓地に行く
                Assert.Empty(p.Field.AllCards);
                Assert.Single(p.Cemetery.AllCards);

                // 手札には戻らない
                Assert.Empty(diffHandsIdList);
            });
        }

        [Fact]
        public async Task MagicShieldGoblin_自分が破壊された場合()
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

                // 攻撃されても、自分が破壊された場合は相手は手札に戻らない
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandsIdList = afterHandsIdList
                    .Except(beforpHandsIdList)
                    .ToArray();

                // 破壊され墓地に行く
                Assert.Empty(op.Field.AllCards);
                Assert.Single(op.Cemetery.AllCards);

                // 攻撃側は手札には戻らない
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

                // 攻撃されると攻撃したカードを持ち主の手札に戻す
                var p2Goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforpDeckIdList = p.Deck.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2Goblin.Id, p1TestCard.Id));

                var afterDeckIdList = p.Deck.AllCards.Select(c => c.Id).ToArray();
                var diffDeckIdList = afterDeckIdList
                    .Except(beforpDeckIdList)
                    .ToArray();

                Assert.Empty(p.Field.AllCards);

                // デッキに移動している
                Assert.Single(diffDeckIdList);
                Assert.Equal(p2Goblin.Id, diffDeckIdList[0]);

                // デッキの一番上
                Assert.Equal(p2Goblin.Id, afterDeckIdList[0]);

                // 攻撃したときは効果が発動しない
                await g.DestroyCard(p1TestCard);
                var p2TestCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforpOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2TestCard.Id, p1Goblin.Id));

                var afterOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                // 効果が発動しないので相手フィールド上のカードに変更はない
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
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 1, 2);
            var tokenDef = SampleCards.KarakuriGoblin;
            var testCardDef = SampleCards.MechanicGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { tokenDef, testCardDef, goblinDef });

            // 先攻
            // testcardを出す
            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforeHands = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                await g.DestroyCard(testcard);

                // 手札にトークンが一枚増える
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

            // 場には2体出ていて、ぜんぶtestcard
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

            // 場には3体出ていて、ぜんぶtestcard
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

            // 先攻
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
        public async Task GoblinsGreed_手札から捨てる()
        {
            var testCardDef = SampleCards.GoblinsGreed;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                await TestUtil.AssertGameAction(() => g.Discard(pId, new[] { g.ActivePlayer.Hands.AllCards[0].Id }));

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 1, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                // 1枚減って1枚増える
                Assert.Equal(beforeNumOfHands, afterNumOfHands);
            });
        }

        [Fact]
        public async Task ShamanGoblin()
        {
            var testCardDef = SampleCards.ShamanGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeNumFields = c.Player1.Field.Count;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterNumFields = c.Player1.Field.Count;

                // 1枚破壊されているはず
                Assert.Equal(beforeNumFields - 1, afterNumFields);
            });
        }

        [Fact]
        public async Task HealGoblin()
        {
            var testCardDef = SampleCards.HealGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 事前にダメージ
                c.Player1.Damage(5);

                var beforeHp = c.Player1.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 2回復している
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

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            expectedAskPlayerLsit = new[] { c.Player1.Id };
            expectedAskCardLsit = new[] { goblin1, goblin2 };

            // 後攻
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                // プレイすると相手に1ダメージ
                var beforeHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp - 1, op.CurrentHp);

                // プレイ以外の方法で場に出ても相手に1ダメージ
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場にはtestCardが1体出ている
                Assert.Single(g.ActivePlayer.Field.AllCards);

                await g.DestroyCard(testCard);

                // 破壊されると2コストのカードが場に出る
                Assert.Single(g.ActivePlayer.Field.AllCards);
                Assert.Equal(cost2Def.Id, g.ActivePlayer.Field.AllCards[0].CardDefId);
            });
        }

        [Fact]
        public async Task MadScientist()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 2, 2);
            var testCardDef = SampleCards.MadScientist;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef },
                options: TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick))
                );

            // 先攻
            var goblin = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // 後攻
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ダメージ軽減される
                await g.HitCreature(new(testcard, 3, testcard));
                Assert.Equal(testcard.BaseToughness - 1, testcard.Toughness);

                // ほかクリーチャーの攻撃が強化される
                var beforeHp = c.Player2.CurrentHp;
                await g.AttackToPlayer(pId, goblin.Id, c.Player2.Id);
                var afterHp = c.Player2.CurrentHp;
                Assert.Equal(beforeHp - (goblin.Power + 1), afterHp);

                // 自分の攻撃は強化されない
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                choicePlayerIdList = new[] { op.Id };

                var beforeHandsCount = c.Player1.Hands.Count;

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandsCount = c.Player1.Hands.Count;

                // 1枚ドローしている
                Assert.Equal(1, afterHandsCount - beforeHandsCount);

                var beforeOpHp = op.CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterOpHp = op.CurrentHp;

                // ダメージが+1している
                Assert.Equal(2, beforeOpHp - afterOpHp);

            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                choicePlayerIdList = new[] { op.Id };

                var beforeOpHp = op.CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterOpHp = op.CurrentHp;

                // 相手の魔法ではダメージが+1されない
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = c.Player1.CurrentHp;
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 味方クリーチャーにダメージ
                Assert.Equal(goblin.BaseToughness - 3, goblin.Toughness);
                Assert.Equal(goblin2.BaseToughness - 3, goblin2.Toughness);

                // プレイヤーにはダメージなし
                Assert.Equal(beforeHp, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task LeaderGoblin()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 1, 2);
            var testCreatureDef = SampleCards.LeaderGoblin;
            testCreatureDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCreatureDef });

            // ゴブリン２体出してから効果クリーチャーを出す
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

                // ゴブリンが2体とも+1/+0 されている
                Assert.Equal(1, goblinCard.PowerBuff);
                Assert.Equal(0, goblinCard.ToughnessBuff);
                Assert.Equal(1, goblinCard2.PowerBuff);
                Assert.Equal(0, goblinCard2.ToughnessBuff);

                // ただし自分はバフされない
                Assert.Equal(0, testCreatureCard.PowerBuff);
                Assert.Equal(0, testCreatureCard.ToughnessBuff);

                return (goblinCard, goblinCard2);
            });

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
                // ゴブリンが2体とも+1/+0 のまま
                Assert.Equal(1, goblin1.PowerBuff);
                Assert.Equal(0, goblin1.ToughnessBuff);
                Assert.Equal(1, goblin2.PowerBuff);
                Assert.Equal(0, goblin2.ToughnessBuff);
            });

            await TestUtil.Turn(c.GameMaster, (g, pid) =>
            {
                // ゴブリンが2体とも+2/+0 になる
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ↓で1枚増えるから
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var (_, p) = g.playerRepository.TryGet(pId);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(p.Field.AllCards);

                var beforeFieldCardIdList = p.Field.AllCards.Select(c => c.Id).ToArray();

                // 魔法カードをプレイする
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterFieldCardIdList = p.Field.AllCards.Select(c => c.Id).ToArray();
                var diffFieldCardIdList = afterFieldCardIdList.Except(beforeFieldCardIdList).ToArray();

                // 自分の場に1枚追加される
                Assert.Single(diffFieldCardIdList);

                // 追加されたカードはトークン
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場には3体いる
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

            // 先攻
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

            // 先攻
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var (_, p) = g.playerRepository.TryGet(pId);
                var op = g.GetOpponent(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHandIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                // お互いの場のカードが1枚ずつ
                Assert.Single(op.Field.AllCards);
                Assert.Single(p.Field.AllCards);

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandIdList = beforeHandIdList.Except(afterHandIdList).ToArray();

                // このカード以外のすべてのクリーチャーが破壊される
                Assert.Empty(op.Field.AllCards);
                Assert.Single(p.Field.AllCards);

                // 破壊した枚数だけ手札を捨てる
                Assert.Equal(2, diffHandIdList.Length);
            });
        }

        [Fact]
        public async Task TempRamp()
        {
            var testCardDef = SampleCards.TempRamp;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻1ターン目
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(1, c.Player1.MaxMp);
                Assert.Equal(1, c.Player1.CurrentMp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 最大値が１増える。未使用のMPも増える。
                Assert.Equal(2, c.Player1.MaxMp);
                Assert.Equal(2, c.Player1.CurrentMp);
            });

            // 最大値が１減る。未使用のMPも減る。
            Assert.Equal(1, c.Player1.MaxMp);
            Assert.Equal(1, c.Player1.CurrentMp);

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 先攻2ターン目
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(2, c.Player1.MaxMp);
                Assert.Equal(2, c.Player1.CurrentMp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 最大値が１増える。未使用のMPも増える。
                Assert.Equal(3, c.Player1.MaxMp);
                Assert.Equal(3, c.Player1.CurrentMp);
            });

            // 最大値が１減る。未使用のMPも減る。
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

            // カードの選択処理のテスト
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                expectedChoiceCardIdList = new[] { goblin.Id };

                await g.HitCreature(new Core.Entities.Effect.DamageContext(goblin, 5, goblin));

                Assert.Empty(c.Player1.Field.AllCards);
                Assert.Equal(0, goblin.Toughness);

                var beforeHandCardIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                // 自分のカードが対象
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandIdList = afterHandIdList.Except(beforeHandCardIdList).ToArray();

                // 墓地のカードが手札に移動している
                Assert.Single(diffHandIdList);

                // タフネスが回復している
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

            // カードの選択処理のテスト
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                expectedChoiceCardIdList = new[] { goblin.Id };

                await g.HitCreature(new Core.Entities.Effect.DamageContext(goblin, 5, goblin));

                Assert.Empty(c.Player1.Field.AllCards);
                Assert.Equal(0, goblin.Toughness);

                var beforeHandCardIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                // 自分のカードが対象
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var diffHandIdList = afterHandIdList.Except(beforeHandCardIdList).ToArray();

                // 墓地のカードが手札に移動している
                Assert.Single(diffHandIdList);

                // コピーが手札に加わる
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

            // 先攻
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                await g.DestroyCard(goblin);

                Assert.Empty(c.Player1.Field.AllCards);

                // 自分のカードが対象
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(c.Player1.Field.AllCards);
                Assert.Equal(goblinDef.Id, c.Player1.Field.AllCards[0].CardDefId);
                Assert.Equal(1, c.Player1.Field.AllCards[0].Toughness);

                return goblin;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await g.DestroyCard(goblin);

                Assert.Empty(c.Player2.Field.AllCards);

                // 相手のカードが対象
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(c.Player2.Field.AllCards);
                Assert.Equal(goblinDef.Id, c.Player2.Field.AllCards[0].CardDefId);
                Assert.Equal(1, c.Player2.Field.AllCards[0].Toughness);
            });
        }

        [Fact]
        public async Task Sword()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);

            var testCardDef = SampleCards.Sword;
            testCardDef.Cost = 0;

            // カードの選択処理のテスト
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

            // 先攻
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
            var goblin = SampleCards.Creature(0, "ゴブリン", 1, 2);

            var testCardDef = SampleCards.Shield;
            testCardDef.Cost = 0;

            // カードの選択処理のテスト
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

            // 先攻
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

            // カードの選択処理のテスト
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

            // 先攻
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

            // カードの選択処理のテスト
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var c1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin2Def.Id);

                expectedChoiceCardIdList = new[] { c1.Id, c2.Id, c3.Id };
            });

            // 後攻
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

            // カードの選択処理のテスト
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var c1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin2Def.Id);

                expectedChoiceCardIdList = new[] { c1.Id, c2.Id, c3.Id };
            });

            // 後攻
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

            // カードの選択処理のテスト
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

            // 先攻
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
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 2, 3, numTurnsToCanAttack: 0);

            var testCardDef = SampleCards.HolyShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // 後攻
            var goblin3 = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ゴブリンで敵を攻撃
                // 攻撃した方はダメージを受けない
                await g.AttackToCreature(pId, goblin3.Id, goblin1.Id);
                Assert.Equal(goblinDef.Toughness - goblinDef.Power, goblin1.Toughness);
                Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);

                return goblin3;
            });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 次のターンなので、お互いにダメージを受ける
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                var beforeHandIdList = g.ActivePlayer.Hands.AllCards.Select(c => c.Id).ToArray();
                Assert.True(beforeNumOfHands != 0);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 捨てた枚数と同じ枚数ドローしてる
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(1, g.ActivePlayer.MaxMp);
                Assert.Equal(1, g.ActivePlayer.CurrentMp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 最大値が１増える。未使用のMPは増えない。
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

            // 自分のカードを戻す
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

            // 相手のカードを戻す
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

            // 自分のカードを戻す
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

                // デッキに移動している
                Assert.Single(diffDeckIdList);
                Assert.Equal(goblin.Id, diffDeckIdList[0]);

                return await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
            });

            // 相手のカードを戻す
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

                // デッキに移動している
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

            // 先攻
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

                // 場に2体いるので2ダメージ
                Assert.Equal(goblin1.BaseToughness - 2, goblin1.Toughness);

                var goblin4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                choiceCardId = goblin11.Id;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場に3体いるので3ダメージ
                Assert.Equal(goblin11.BaseToughness - 3, goblin11.Toughness);
            });
        }

        [Fact]
        public async Task GoblinCaptureJar()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            var notGoblinDef = SampleCards.Goblin;
            notGoblinDef.Name = "スライム";
            goblinDef.Cost = 0;
            var testCardDef = SampleCards.GoblinCaptureJar;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, notGoblinDef, testCardDef });

            // 先攻
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

                // 自軍も敵軍もゴブリンは封印＋パワー1になる
                Assert.Contains(CreatureAbility.Sealed, goblin1.Abilities);
                Assert.Equal(1, goblin1.Power);
                Assert.Contains(CreatureAbility.Sealed, goblin2.Abilities);
                Assert.Equal(1, goblin2.Power);

                // ゴブリン以外はなにもならない
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

            // 先攻
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

            // 先攻
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
        public async Task OldShield_クリーチャーの防御時()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // クリーチャーは1ダメージしか受けない
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
                // 破壊される
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_プレイヤーの防御時()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await c.GameMaster.AttackToPlayer(pId, goblinCard2.Id, c.Player1.Id);

                // プレイヤーは2ダメージ受ける
                Assert.Equal(TestUtil.TestRuleBook.InitialPlayerHp - 2, c.Player1.CurrentHp);
                // 破壊されない
                Assert.Equal(ZoneName.Field, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_攻撃時()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Power = 2;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var goblinCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 自軍クリーチャーも対象になる
                Assert.Equal(goblinCard.BaseToughness - goblinDef.Power, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - (goblinDef.Power - 1), goblinCard2.Toughness);
                // 破壊される
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_0ダメージ()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Power = 1;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var goblinCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblinCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 1枚は破壊されるが、もう1枚は破壊されない
                Assert.Equal(2, op.Field.Count);
            });
        }

        [Fact]
        public async Task OldWall_プレイヤーを攻撃()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var testcard = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = c.Player1.CurrentHp;
                await c.GameMaster.AttackToPlayer(pId, goblinCard.Id, c.Player1.Id);

                // 1ダメージしか受けない
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_クリーチャーを攻撃()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblin, testcard) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblin, testcard);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = c.Player1.CurrentHp;
                await c.GameMaster.AttackToCreature(pId, goblin2.Id, goblin.Id);

                // 1ダメージしか受けない
                Assert.Equal(goblin.BaseToughness - 1, goblin.Toughness);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_攻撃時()
        {
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Power = 2;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var goblinCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await c.GameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 攻撃時も対象になる
                Assert.Equal(goblinCard.BaseToughness - goblinDef.Power, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - (goblinDef.Power - 1), goblinCard2.Toughness);
                // 破壊される
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 事前にダメージ
                c.Player1.Damage(5);

                var beforeHp = c.Player1.CurrentHp;
                var beforeHandIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 手札が1枚減っている
                Assert.Equal(beforeHandIdList.Length - 1, c.Player1.Hands.AllCards.Count);

                // 捨てたカードのコスト分ライフが回復している
                Assert.Equal(beforeHp + goblin.Cost, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task GoblinStatue()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 1, 10);

            var testCardDef = SampleCards.GoblinStatue;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // 後攻
            var goblin3 = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // 30枚墓地に送る
                foreach (var _ in Enumerable.Range(0, 30))
                {
                    var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                    await g.DestroyCard(goblin);
                }

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblin3;
            });

            // 相手プレイヤーとクリーチャーに6ダメージ
            Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp - 6, c.Player1.CurrentHp);
            Assert.Equal(goblin1.BaseToughness - 6, goblin1.Toughness);
            Assert.Equal(goblin2.BaseToughness - 6, goblin2.Toughness);

            // 自分プレイヤーと自分クリーチャーはダメージを受けない
            Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp, c.Player2.CurrentHp);
            Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);
        }

        [Fact]
        public async Task HolyStatue()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", 1, 2);

            var testCardDef = SampleCards.HolyStatue;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 先に場にいたゴブリン2体が修正される
                Assert.True(goblinCard.PowerBuff == 0 && goblinCard.ToughnessBuff == 1);
                Assert.True(goblinCard2.PowerBuff == 0 && goblinCard2.ToughnessBuff == 1);

                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // あとに場に出たゴブリンだけが修正される
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

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            Assert.Contains(testCard, c.Player1.Field.AllCards);

            // 先攻
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.False(g.GameOver);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.False(g.GameOver);
            });

            Assert.False(c.GameMaster.GameOver);

            // 先攻
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 先攻2
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 先攻3
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // 残りMPが-2されている
                Assert.Equal(2, c.Player1.UsedMp);
            });
        }

        [Fact]
        public async Task Prophet()
        {
            var testCardDef = SampleCards.Prophet;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 次のターンが来るまでは０のまま
            Assert.Equal(0, testCard.Power);

            // 先攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ターン開始時に7になる
                Assert.Equal(7, testCard.Power);
            });
        }

        [Fact]
        public async Task Psycho_攻撃によるダメージ()
        {
            var testCardDef = SampleCards.Psycho;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.AttackToCreature(pId, goblin.Id, testCard.Id);

                // 攻撃によるダメージを0にする
                Assert.Equal(testCard.BaseToughness, testCard.Toughness);

                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.AttackToCreature(pId, goblin2.Id, testCard.Id);

                // 2度目は0にできない
                Assert.Equal(testCard.BaseToughness - 1, testCard.Toughness);
            });
        }

        [Fact]
        public async Task Psycho_攻撃以外によるダメージ()
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

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            choiceCardId = testCard.Id;

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                // 攻撃によるダメージを0にする
                Assert.Equal(testCard.BaseToughness, testCard.Toughness);

                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                // 2度目は0にできない
                Assert.Equal(testCard.BaseToughness - 1, testCard.Toughness);
            });
        }

        [Fact]
        public async Task Nightmare()
        {
            var testCardDef = SampleCards.Nightmare;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 次のターンが来るまではもとのまま
            Assert.Equal(testCard.BasePower, testCard.Power);

            // 先攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ターン開始時に2倍になる
                Assert.Equal(testCard.BasePower * 2, testCard.Power);
            });

            // 後攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 次のターンが来るまではもとのまま
            Assert.Equal(testCard.BasePower * 2, testCard.Power);

            // 先攻3
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ターン開始時にさらに2倍になる
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

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            choiceCardId = testCard.Id;

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // この時点では場に1枚
                Assert.Equal(1, c.Player1.Field.Count);

                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                // ダメージを受けると、トークンを自分の場に追加する
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

            // 先攻
            var (goblinP3, goblinP4) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinP3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDefp3.Id);
                var goblinP4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDefp4.Id);

                return (goblinP3, goblinP4);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 相手の場に2体
                Assert.Equal(2, c.Player1.Field.Count);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 相手の場のパワー4以上のクリーチャーだけ破壊
                Assert.Equal(1, c.Player1.Field.Count);
                Assert.Equal(goblinDefp3.Id, c.Player1.Field.AllCards[0].CardDefId);
            });

            // 先攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ドローしたカードのパワーが4以上なら破壊する
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                choiceCardId = goblin.Id;

                // 除外済みカードなし
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

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 場に出る以前に除外されていても影響なし
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // 攻撃力はもとのまま
                Assert.Equal(testCard.BasePower, testCard.Power);

                await g.ExcludeCard(goblin2);

                // カードが除外されたので攻撃力が1上がる
                Assert.Equal(testCard.BasePower + 1, testCard.Power);

                return testCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // まだ攻撃力はもとのまま
                Assert.Equal(testCard.BasePower + 1, testCard.Power);

                await g.ExcludeCard(goblin);

                // 相手のカードが除外されても攻撃力が上がる
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 除外済み:0 なので、ベースのまま
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard.BasePower, testCard.Power);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin);

                // 除外済み:1 なので、+1されている
                var testCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard2.BasePower + 1, testCard2.Power);

                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin3);

                // 除外済み:2 なので、+2されている
                var testCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard3.BasePower + 2, testCard3.Power);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 相手の除外済み枚数は影響なし
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                // なにも除外されていないので、なにも起きない
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Empty(p.Field.AllCards);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin);

                Assert.Empty(p.Field.AllCards);

                // 除外済みカードのコピーが場に追加される
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Single(p.Field.AllCards);
                Assert.Equal(goblinDef.Id, p.Field.AllCards[0].CardDefId);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                // 自分の除外済みカードだけが対象なので、なにも起きない
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                // 手札を2コストのカードだけにする（除外されるカードを固定するため）
                await g.Discard(p.Id, p.Hands.AllCards.Select(c => c.Id).ToArray());
                await g.GenerateNewCard(goblinC2Def.Id, new Zone(pId, ZoneName.Hand), default);

                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                Assert.Equal(4, p.Field.AllCards.Count);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 手札から1枚除外される
                Assert.Empty(p.Hands.AllCards);

                // 場のカードが2枚破壊される（除外したカードがコスト=2なので）
                Assert.Equal(2, p.Field.AllCards.Count);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_攻撃する()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // 先攻
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // 後攻
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

                // 攻撃した方もされた方も、両方除外される。
                Assert.Empty(p.Field.AllCards);
                Assert.Empty(op.Field.AllCards);

                Assert.Single(p.Excludes);
                Assert.Single(op.Excludes);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_攻撃される()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
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

                // 攻撃した方もされた方も、両方除外される。
                Assert.Empty(p.Field.AllCards);
                Assert.Empty(op.Field.AllCards);

                Assert.Single(p.Excludes);
                Assert.Single(op.Excludes);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_攻撃されて相手が死ぬ()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Toughness = 1;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
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

                // 攻撃した方もされた方も、両方除外される。
                Assert.Empty(p.Field.AllCards);
                Assert.Empty(op.Field.AllCards);

                Assert.Single(p.Excludes);
                Assert.Single(op.Excludes);
            });
        }

        [Fact]
        public async Task DDShieldGoblin_プレイヤーへ攻撃する()
        {
            var testCardDef = SampleCards.DDShieldGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Single(p.Field.AllCards);
                Assert.Empty(p.Excludes);

                var status = await g.AttackToPlayer(pId, testCard.Id, op.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);

                // プレイヤーへ攻撃した場合は、除外されない
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

            // 先攻
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var mycover = await TestUtil.NewCardAndPlayFromHand(g, pId, coverCardDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 相手の場のカバー持ちからカバーがなくなっている
                Assert.Empty(cover.Abilities);
                Assert.Empty(cover2.Abilities);

                // 相手の場のカバー持ちだけに1ダメージ
                Assert.Equal(cover.BaseToughness - 1, cover.Toughness);
                Assert.Equal(cover2.BaseToughness - 1, cover2.Toughness);
                Assert.Equal(nocover.BaseToughness, nocover.Toughness);
                Assert.Equal(nocover2.BaseToughness, nocover2.Toughness);

                // 自分の場のカバーはそのまま
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

            // 先攻
            var testcard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(0, testcard.GetCounter("魔導"));
                Assert.Equal(testcard.BasePower, testcard.Power);

                // 魔法を使うとカウンターが1つ乗る。
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(1, testcard.GetCounter("魔導"));

                // カウンターが乗ったら+1/+0される
                Assert.Equal(testcard.BasePower + 1, testcard.Power);

                // カウンターが減ったら-1/+0される
                await g.ModifyCounter(testcard, "魔導", -1);
                Assert.Equal(testcard.BasePower, testcard.Power);

                // カウンターが乗ったら+1/+0される
                await g.ModifyCounter(testcard, "魔導", 1);
                Assert.Equal(testcard.BasePower + 1, testcard.Power);

                // 2つカウンターが乗ったら+2/+0される
                await g.ModifyCounter(testcard, "魔導", 2);
                Assert.Equal(testcard.BasePower + 3, testcard.Power);

                // 2つカウンターが減ったら+2/+0される
                await g.ModifyCounter(testcard, "魔導", -2);
                Assert.Equal(testcard.BasePower + 1, testcard.Power);

                return testcard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 相手が魔法を使ってもカウンターが乗る
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(2, testcard.GetCounter("魔導"));
            });
        }

        [Fact]
        public async Task MagicMonster()
        {
            var testCardDef = SampleCards.MagicMonster;

            var sorceryDef = SampleCards.Sorcery(0, "", "");

            var c = await TestUtil.InitTest(new[] { testCardDef, sorceryDef });

            // 先攻
            var testcard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testcard = await g.GenerateNewCard(testCardDef.Id, new Zone(pId, ZoneName.Hand), null);
                Assert.Equal(0, testcard.GetCounter("魔導"));
                Assert.Equal(testcard.BasePower, testcard.Power);

                // 魔法を使うとカウンターが1つ乗る。
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(1, testcard.GetCounter("魔導"));

                // カウンターが乗ったらコストが-1
                Assert.Equal(testcard.BaseCost - 1, testcard.Cost);

                // 魔法を使わなくてもカウンターが乗ったらコストが-1
                await g.ModifyCounter(testcard, "魔導", 1);
                Assert.Equal(testcard.BaseCost - 2, testcard.Cost);

                // 2つカウンターが乗ったらコストが-2
                await g.ModifyCounter(testcard, "魔導", 2);
                Assert.Equal(testcard.BaseCost - 4, testcard.Cost);

                return testcard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 相手が魔法を使った場合はカウンターが乗らない
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(4, testcard.GetCounter("魔導"));
            });
        }

        [Fact]
        public async Task BeginnerSorcerer()
        {
            var testCardDef = SampleCards.BeginnerSorcerer;
            testCardDef.Cost = 0;
            var goblinDef = SampleCards.Goblin;
            goblinDef.Cost = 0;
            goblinDef.Annotations = new[] { ":魔導" };

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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                Assert.Equal(0, goblin.GetCounter("魔導"));

                expectedAskCardLsit = new[] { goblin.Id };

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // カウンターが乗っている
                Assert.Equal(2, goblin.GetCounter("魔導"));
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var beforeHandIdList = p.Hands.AllCards.ToArray();

                var testcard_cost0 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef_cost0.Id);

                // もとの手札がすべて除外されている
                Assert.True(beforeHandIdList.All(c => c.Zone.ZoneName == ZoneName.Excluded));

                // 新たに手札を5枚引いている
                Assert.Equal(5, p.Hands.Count);

                // 新たな手札すべてにカウンターが5個置かれている
                Assert.True(p.Hands.AllCards.All(c => c.GetCounter("魔導") == 5));
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

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeOpHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // カウンターがないので相手に1ダメージ
                Assert.Equal(beforeOpHp - 1, op.CurrentHp);

                await g.ModifyCounter(goblin1, "魔導", 1);

                beforeOpHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // カウンターが1個なので相手に2ダメージ
                Assert.Equal(beforeOpHp - 2, op.CurrentHp);
                // カウンターがなくなっている
                Assert.Equal(0, goblin1.GetCounter("魔導"));

                await g.ModifyCounter(goblin1, "魔導", 1);
                await g.ModifyCounter(goblin2, "魔導", 1);

                beforeOpHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // カウンターが2個なので相手に3ダメージ
                Assert.Equal(beforeOpHp - 3, op.CurrentHp);
                // カウンターがなくなっている
                Assert.Equal(0, goblin1.GetCounter("魔導"));
                Assert.Equal(0, goblin2.GetCounter("魔導"));
            });
        }

        [Fact]
        public async Task Investment()
        {
            var testCardDef = SampleCards.Investment;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, });

            // 先攻
            var beforeNumHands = 0;
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                beforeNumHands = c.Player1.Hands.Count;
            });

            // まだドローできない
            Assert.Equal(beforeNumHands, c.Player1.Hands.Count);

            await TestUtil.Turn(c.GameMaster, (g, pId) => { });

            // 先攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                beforeNumHands = c.Player1.Hands.Count;
            });

            // ここでドローできる
            Assert.Equal(beforeNumHands + 2, c.Player1.Hands.Count);

            await TestUtil.Turn(c.GameMaster, (g, pId) => { });

            // 先攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                beforeNumHands = c.Player1.Hands.Count;
            });

            // もうドローできない
            Assert.Equal(beforeNumHands, c.Player1.Hands.Count);
        }
    }
}
