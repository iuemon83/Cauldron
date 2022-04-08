using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Cauldron.Core_Test
{
    public class SampleCards_Test
    {
        private readonly ITestOutputHelper output;

        public SampleCards_Test(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task Quick()
        {
            var testCardDef = SampleCards1.Quick;
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
        public async Task Twin()
        {
            var testCardDef = SampleCards1.Twin;
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
        public async Task SlowStarter()
        {
            var testCardDef = SampleCards1.SlowStarter;
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
            var testCardDef = SampleCards2.DoubleStrikeGoblin;
            testCardDef.Cost = 0;
            testCardDef.NumTurnsToCanAttack = 0;

            var goblinT1Def = SampleCards1.Vanilla;
            goblinT1Def.Cost = 0;
            goblinT1Def.Toughness = 1;

            var goblinT3Def = SampleCards1.Vanilla;
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
                Assert.Equal(ZoneName.Cemetery, goblinT1.Zone.ZoneName);

                var testCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // 相手を倒しきれないのでダメージを受ける
                await g.AttackToCreature(pid, testCard2.Id, goblinT3.Id);

                Assert.Equal(ZoneName.Cemetery, testCard2.Zone.ZoneName);
                Assert.Equal(ZoneName.Cemetery, goblinT3.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task DoubleStrikeGoblin_攻撃される()
        {
            var testCardDef = SampleCards2.DoubleStrikeGoblin;
            testCardDef.Cost = 0;
            testCardDef.NumTurnsToCanAttack = 0;

            var goblinT1Def = SampleCards1.Vanilla;
            goblinT1Def.Cost = 0;
            goblinT1Def.Toughness = 1;

            var goblinT3Def = SampleCards1.Vanilla;
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

                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
                Assert.Equal(ZoneName.Cemetery, goblinT3.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task MagicBook()
        {
            var testCardDef = SampleCards1.MagicBook;
            testCardDef.Cost = 0;
            var sorceryDef = SampleCards1.Sword;

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
            var testCardDef = SampleCards2.GoblinFollower;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards2.Goblin;
            var goblinDef2 = SampleCards2.GoblinLover;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, goblinDef, goblinDef2 },
                Enumerable.Repeat(testCardDef, 10)
                    .Concat(Enumerable.Repeat(goblinDef, 15))
                    .Concat(Enumerable.Repeat(goblinDef2, 15))
                    );

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var beforeDeckCount = c.Player1.Deck.Count;
                Assert.Equal(0, c.Player1.Field.Count);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var afterFieldIdList = c.Player1.Field.AllCards;
                var diffFieldIdList = afterFieldIdList
                    // 自分で出したカード以外
                    .Where(c => c.Id != goblin.Id)
                    .ToArray();

                var afterDeckCount = c.Player1.Deck.Count;

                // 追加されたカードがある
                Assert.True(diffFieldIdList.Length > 1);
                // 追加されたカードはぜんぶゴブリンフォロワー
                Assert.True(diffFieldIdList.All(c => c.CardDefId == testCardDef.Id));
                // デッキからなくなる
                Assert.True(c.Player1.Deck.AllCards.All(c => c.CardDefId != testCardDef.Id));
            });
        }

        /// <summary>
        /// ゴブリンと関係ないカードで発動しないテスト
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GoblinFollower2()
        {
            var testCardDef = SampleCards2.GoblinFollower;
            testCardDef.Cost = 0;

            var notGoblinDef = SampleCards1.Vanilla;
            notGoblinDef.Cost = 0;

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
            var testCardDef = SampleCards2.GoblinFollower;
            testCardDef.Cost = 0;

            var notGoblinCreatureDef = SampleCards1.Artifact(0, "ゴブリンアーティファクト");
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
            var testCardDef = SampleCards1.Insector;
            testCardDef.Cost = 0;

            var tokenDef = SampleCards1.Parasite;

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
        public async Task MagicFighter()
        {
            var testCardDef = SampleCards2.MagicFighter;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
                await g.DestroyCard(p1TestCard, default);
                var p2TestCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforpOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.AssertGameAction(() => g.AttackToCreature(pid, p2TestCard.Id, p1Goblin.Id));

                var afterOpFieldIdList = op.Field.AllCards.Select(c => c.Id).ToArray();

                // 効果が発動しないので相手フィールド上のカードに変更はない
                TestUtil.AssertCollection(beforpOpFieldIdList, afterOpFieldIdList);
            });
        }

        [Fact]
        public async Task MagicFighter_攻撃カードが破壊された場合()
        {
            var testCardDef = SampleCards2.MagicFighter;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
        public async Task MagicFighter_自分が破壊された場合()
        {
            var testCardDef = SampleCards2.MagicFighter;
            testCardDef.Cost = 0;
            testCardDef.Toughness = 1;

            var goblinDef = SampleCards1.Vanilla;
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
        public async Task SuperMagicFighter()
        {
            var testCardDef = SampleCards2.SuperMagicFighter;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
                await g.DestroyCard(p1TestCard, default);
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
            var testCardDef = SampleCards2.GoblinsPet;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            var sorceryDef = SampleCards1.SelectDamage;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, goblinDef, sorceryDef },
                Enumerable.Repeat(sorceryDef, 10)
                );

            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var op = c.GameMaster.GetOpponent(pid);

                // 相手の手札にクリーチャーを追加しておく
                // 効果で場に出ることを確認するため
                var addHandCard = await g.GenerateNewCard(goblinDef.Id, new Zone(c.Player2.Id, ZoneName.Hand), default, default);

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

                // 相手の手札から場に1枚カードが出ている
                Assert.Single(diffOpFieldIdList);
                Assert.Single(diffOpHandsIdList);
                Assert.Equal(diffOpFieldIdList[0], diffOpHandsIdList[0]);
                Assert.Equal(addHandCard.Id, diffOpHandsIdList[0]);
            });
        }

        [Fact]
        public async Task Mechanic()
        {
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 1, 2);
            var tokenDef = SampleCards1.KarakuriDoll;
            var testCardDef = SampleCards1.Mechanic;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { tokenDef, testCardDef, goblinDef });

            // 先攻
            // testcardを出す
            await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforeHands = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                await g.DestroyCard(testcard, default);

                // 手札にトークンが一枚増える
                var addedHands = c.Player1.Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
                Assert.Single(addedHands);
                Assert.Equal(tokenDef.Id, addedHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task Ninja()
        {
            var testCard = SampleCards1.Ninja;
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
        public async Task Hope()
        {
            var testCardDef = SampleCards1.Hope;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                async ValueTask assert(int expectedNumDraws)
                {
                    var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                    var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                    await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                    var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                    Assert.Equal(beforeNumOfDecks - expectedNumDraws, afterNumOfDecks);

                    var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                    Assert.Equal(beforeNumOfHands + expectedNumDraws, afterNumOfHands);
                }

                // HP差がないのでドローできない
                await assert(0);

                // HP差があるのでドローできる
                await g.DamagePlayer(new(c.Player1.Hands.AllCards[0], 1, GuardPlayer: c.Player1), default);
                await assert(1);

                // HP差があっても相手HPのほうが低いとドローできない
                await g.DamagePlayer(new(c.Player1.Hands.AllCards[0], 2, GuardPlayer: c.Player2), default);
                await assert(0);
            });
        }

        [Fact]
        public async Task Greed()
        {
            var testCardDef = SampleCards1.Greed;
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
        public async Task Greed_手札から捨てる()
        {
            var testCardDef = SampleCards1.Greed;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                await TestUtil.AssertGameAction(() => g.Discard(pId, new[] { g.ActivePlayer.Hands.AllCards[0].Id }, default));

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 1, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                // 1枚減って1枚増える
                Assert.Equal(beforeNumOfHands, afterNumOfHands);
            });
        }

        [Fact]
        public async Task Shaman()
        {
            var testCardDef = SampleCards1.Shaman;
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
        public async Task Healer()
        {
            var testCardDef = SampleCards1.Healer;
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
            var testCardDef = SampleCards2.FireGoblin;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef }, this.output);

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHp = g.GetOpponent(pId).CurrentHp;

                c.TestAnswer.ExpectedPlayerIdList = new[] { c.Player1.Id };
                c.TestAnswer.ExpectedCardIdList = new[] { goblin1.Id, goblin2.Id };
                c.TestAnswer.ChoicePlayerIdList = new[] { c.Player1.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHp = g.GetOpponent(pId).CurrentHp;

                Assert.Equal(beforeHp - 2, afterHp);
            });
        }

        [Fact]
        public async Task DashGoblin()
        {
            var testCardDef = SampleCards2.DashGoblin;
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
                await g.GenerateNewCard(testCardDef.Id, new(pId, ZoneName.Field), default, default);
                Assert.Equal(beforeHp - 1, op.CurrentHp);
            });
        }

        [Fact]
        public async Task BeginnerSummoner()
        {
            var testCardDef = SampleCards1.BeginnerSummoner;
            testCardDef.Cost = 0;
            var cost1Def = SampleCards1.Vanilla;
            cost1Def.Cost = 1;
            var cost2Def = SampleCards1.Vanilla;
            cost2Def.Cost = 2;

            var c = await TestUtil.InitTest(new[] { testCardDef, cost1Def, cost2Def });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場にはtestCardが1体出ている
                Assert.Single(g.ActivePlayer.Field.AllCards);

                await g.DestroyCard(testCard, default);

                // 破壊されると2コストのカードが場に出る
                Assert.Single(g.ActivePlayer.Field.AllCards);
                Assert.Equal(cost2Def.Id, g.ActivePlayer.Field.AllCards[0].CardDefId);
            });
        }

        [Fact]
        public async Task MadScientist()
        {
            var testCardDef = SampleCards1.MadScientist;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 2, 2);

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef }, this.output);

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

                c.TestAnswer.ExpectedCardIdList = new[] { goblin.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(1, c.Player1.Field.Count);
                Assert.NotEqual(goblin.Id, c.Player1.Field.AllCards[0].Id);
            });
        }

        [Fact]
        public async Task BraveGoblin()
        {
            var goblinDef = SampleCards2.Goblin;
            goblinDef.Cost = 0;
            goblinDef.NumTurnsToCanAttack = 0;

            var testCardDef = SampleCards2.BraveGoblin;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ダメージ軽減される
                await g.DamageCreature(new(testcard, 3, testcard), default);
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
            var testCardDef = SampleCards1.MagicDragon;
            testCardDef.Cost = 0;

            var sorceryDef = SampleCards1.SelectDamage;

            var c = await TestUtil.InitTest(new[] { testCardDef, sorceryDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                var beforeHandsCount = c.Player1.Hands.Count;

                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHandsCount = c.Player1.Hands.Count;

                // 1枚ドローしている
                Assert.Equal(1, afterHandsCount - beforeHandsCount);

                var beforeOpHp = op.CurrentHp;

                c.TestAnswer.ChoicePlayerIdList = new[] { op.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterOpHp = op.CurrentHp;

                // ダメージが+1している
                Assert.Equal(2, beforeOpHp - afterOpHp);

            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                var beforeOpHp = op.CurrentHp;

                c.TestAnswer.ChoicePlayerIdList = new[] { op.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);

                var afterOpHp = op.CurrentHp;

                // 相手の魔法ではダメージが+1されない
                Assert.Equal(1, beforeOpHp - afterOpHp);
            });
        }

        [Fact]
        public async Task GiantGoblin()
        {
            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards2.GiantGoblin;
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
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 1, 2);
            var testCreatureDef = SampleCards2.LeaderGoblin;
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
            var testCardDef = SampleCards2.TyrantGoblin;
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
            var testCardDef = SampleCards2.MagicRider;
            testCardDef.Cost = 0;
            var tokenDef = SampleCards2.MagicSoldier;
            var sorceryDef = SampleCards1.RandomDamage;
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
        public async Task Bomber()
        {
            var testCardDef = SampleCards1.Bomber;
            testCardDef.Cost = 0;
            var addcardDef = SampleCards1.Bomb;

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
        public async Task Bomb()
        {
            var testCardDef = SampleCards1.Bomb;
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

                    await g.DestroyCard(testCard, default);

                    var damage = beforeOpHp - op.CurrentHp;

                    Assert.True(1 <= damage && damage <= 4);

                    op.GainHp(4);
                }
            });
        }

        [Fact]
        public async Task Firelord()
        {
            var testCardDef = SampleCards1.Firelord;
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
        public async Task Reaper()
        {
            var testCardDef = SampleCards1.Reaper;
            testCardDef.Cost = 0;
            var goblinDef = SampleCards1.Vanilla;
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
            var testCardDef = SampleCards1.TempRamp;

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
            var testCardDef = SampleCards1.Salvage;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Creature(0, "t", 1, 2);
            var spellDef = SampleCards1.SelectDeathDamage;
            spellDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef, spellDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // まずクリーチャーを破壊する
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
                var s1 = await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                Assert.Empty(c.Player1.Field.AllCards);
                Assert.Equal(ZoneName.Cemetery, goblin.Zone.ZoneName);

                var beforeHandCardIdList = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                // 自分のカードが対象
                c.TestAnswer.ExpectedCardIdList = new[] { goblin.Id, s1.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
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
        public async Task Sword()
        {
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 1, 2);

            var testCardDef = SampleCards1.Sword;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                c.TestAnswer.ExpectedCardIdList = new[] { goblinCard.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblinCard.Id };
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(1, goblinCard.PowerBuff);
                Assert.Equal(0, goblinCard.ToughnessBuff);
            });
        }

        [Fact]
        public async Task Shield()
        {
            var testCardDef = SampleCards1.Shield;
            testCardDef.Cost = 0;

            var goblin = SampleCards1.Creature(0, "ゴブリン", 1, 2);

            var c = await TestUtil.InitTest(new[] { goblin, testCardDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                c.TestAnswer.ExpectedCardIdList = new[] { goblinCard.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblinCard.Id };
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, goblinCard.PowerBuff);
                Assert.Equal(1, goblinCard.ToughnessBuff);
            });
        }

        [Fact]
        public async Task HitOrHeal()
        {
            var testCardDef1 = SampleCards1.HitOrHeal;
            testCardDef1.Cost = 0;

            var hitDef = SampleCards1.Hit;
            var healDef = SampleCards1.Heal;

            var c = await TestUtil.InitTest(
                new[] { testCardDef1, hitDef, healDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHandIds = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();

                c.TestAnswer.ExpectedCardDefIdList = new[] { hitDef.Id, healDef.Id };
                c.TestAnswer.ChoiceCardDefIdList = new[] { hitDef.Id };

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = c.Player1.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Single(diffHands);
                Assert.Equal(hitDef.Id, diffHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task Copy()
        {
            var testCardDef1 = SampleCards1.Copy;
            testCardDef1.Cost = 0;

            var goblin1Def = SampleCards1.Vanilla;
            goblin1Def.Cost = 0;
            var goblin2Def = SampleCards1.Quick;
            goblin2Def.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef1, goblin1Def, goblin2Def }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var c1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin2Def.Id);

                c.TestAnswer.ExpectedCardIdList = new[] { c1.Id, c2.Id, c3.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { c1.Id, };
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var beforeHandIds = c.Player2.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = c.Player2.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Single(diffHands);
                Assert.Equal(goblin1Def.Id, diffHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task FirstAttack()
        {
            var testCardDef1 = SampleCards2.FirstSpell;
            testCardDef1.Cost = 0;

            var testCardDef2 = SampleCards2.SecondSpell;

            var creatureDef = SampleCards1.Creature(0, "t", 1, 1);

            var c = await TestUtil.InitTest(new[] { testCardDef1, testCardDef2, creatureDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                var beforeHandIds = c.Player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var beforeHp = c.Player2.CurrentHp;

                c.TestAnswer.ExpectedPlayerIdList = new[] { c.Player2.Id };
                c.TestAnswer.ExpectedCardIdList = new[] { creature.Id };
                c.TestAnswer.ChoicePlayerIdList = new[] { c.Player2.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffCards = c.Player1.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Single(diffCards);
                Assert.Equal(testCardDef2.Id, diffCards[0].CardDefId);

                Assert.Equal(beforeHp - 1, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task ChangeHands()
        {
            var testCardDef = SampleCards1.ChangeHands;
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
            var testCardDef = SampleCards1.Ramp;
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
            var testCardDef = SampleCards1.BounceHand;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, }, this.output);

            // 自分のカードを戻す
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforeHandsIdList = p.Hands.AllCards.Select(c => c.Id).ToArray();

                c.TestAnswer.ExpectedCardIdList = new[] { goblin.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
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

                var beforeHandsIdList = op.Hands.AllCards.Select(c => c.Id).ToArray();

                c.TestAnswer.ExpectedCardIdList = new[] { goblin.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
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
            var testCardDef = SampleCards1.BounceDeck;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, }, this.output);

            // 自分のカードを戻す
            var p1Goblin = await TestUtil.Turn(c.GameMaster, async (g, pid) =>
            {
                var (_, p) = g.playerRepository.TryGet(pid);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);

                var beforeDeckIdList = p.Deck.AllCards.Select(c => c.Id).ToArray();

                c.TestAnswer.ExpectedCardIdList = new[] { goblin.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
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

                var beforeDeckIdList = op.Deck.AllCards.Select(c => c.Id).ToArray();

                c.TestAnswer.ExpectedCardIdList = new[] { p1Goblin.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { p1Goblin.Id };
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
            var testCardDef = SampleCards1.Slap;
            testCardDef.Cost = 0;

            var creatureDef = SampleCards1.Creature(0, "t", 1, 10);

            var c = await TestUtil.InitTest(new[] { creatureDef, testCardDef }, this.output);

            // 先攻
            var (goblin1, goblin11) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);
                var goblin11 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                return (goblin1, goblin11);
            });

            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                c.TestAnswer.ExpectedCardIdList = new[] { goblin1.Id, goblin11.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin1.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場に2体いるので2ダメージ
                Assert.Equal(goblin1.BaseToughness - 2, goblin1.Toughness);

                var goblin4 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                c.TestAnswer.ExpectedCardIdList = new[] { goblin1.Id, goblin11.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin11.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場に3体いるので3ダメージ
                Assert.Equal(goblin11.BaseToughness - 3, goblin11.Toughness);
            });
        }

        [Fact]
        public async Task FullAttack()
        {
            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;
            var testCardDef = SampleCards1.FullAttack;
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
            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 4;
            var testCardDef = SampleCards1.Search;
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
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards1.OldShield;
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
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards1.OldShield;
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
            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;
            goblinDef.Power = 2;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards1.OldShield;
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
            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;
            goblinDef.Power = 1;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards1.OldShield;
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
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards1.OldWall;
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
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 2, 2);

            var testCardDef = SampleCards1.OldWall;
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
            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;
            goblinDef.Power = 2;
            goblinDef.Toughness = 5;

            var testCardDef = SampleCards1.OldWall;
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
            var goblin = SampleCards1.Vanilla;
            goblin.Cost = 2;

            var testCardDef = SampleCards1.EmergencyFood;
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
        public async Task StatueOfCurse()
        {
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 1, 10);

            var testCardDef = SampleCards1.StatueOfCurse;
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
                    await g.DestroyCard(goblin, default);
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
        public async Task StatueOfHoly()
        {
            var goblinDef = SampleCards1.Creature(0, "ゴブリン", 1, 2);

            var testCardDef = SampleCards1.StatueOfHoly;
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
        public async Task VictoryRoad()
        {
            var testCardDef = SampleCards1.VictoryRoad;
            testCardDef.Cost = 0;

            var tokenCardDef = SampleCards1.VictoryStatue;

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
        public async Task VictoryRoad_Bounce()
        {
            var testCardDef = SampleCards1.VictoryRoad;
            testCardDef.Cost = 0;

            var tokenCardDef = SampleCards1.VictoryStatue;

            var c = await TestUtil.InitTest(new[] { testCardDef, tokenCardDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // バウンスしてもう一度出す
                await g.MoveCard(testCard.Id, new Core.Entities.Effect.MoveCardContext(
                    new Zone(pId, ZoneName.Field),
                    new Zone(pId, ZoneName.Hand)),
                    default
                    );

                await g.PlayFromHand(pId, testCard.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 先攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // 1枚しか出ない
                Assert.Single(c.Player1.Field.AllCards);
                Assert.Equal(tokenCardDef.Id, c.Player1.Field.AllCards[0].CardDefId);
            });
        }

        [Fact]
        public async Task VictoryStatue()
        {
            var testCardDef = SampleCards1.VictoryStatue;
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
                Assert.Equal(pId, g.WinnerId);
            });
        }

        [Fact]
        public async Task Faceless()
        {
            var testCardDef = SampleCards1.Faceless;
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
            var testCardDef = SampleCards1.Prophet;
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
        public async Task DoubleShield_攻撃によるダメージ()
        {
            var testCardDef = SampleCards1.DoubleShield;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
        public async Task DoubleShield_攻撃以外によるダメージ()
        {
            var testCardDef = SampleCards1.DoubleShield;
            testCardDef.Cost = 0;

            var spellDef = SampleCards1.SelectDamage;
            spellDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, spellDef }, this.output);

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                c.TestAnswer.ChoiceCardIdList = new[] { testCard.Id };
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
            var testCardDef = SampleCards1.Nightmare;
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
            var testCardDef = SampleCards1.Disaster;
            testCardDef.Cost = 0;

            var tokenDef = SampleCards1.Gnoll;

            var spellDef = SampleCards1.SelectDamage;
            spellDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, tokenDef, spellDef }, this.output);

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // この時点では場に1枚
                Assert.Equal(1, c.Player1.Field.Count);

                c.TestAnswer.ChoiceCardIdList = new[] { testCard.Id };
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
            var testCardDef = SampleCards1.Virus;
            testCardDef.Cost = 0;

            var goblinDefp3 = SampleCards1.Vanilla;
            goblinDefp3.Cost = 0;
            goblinDefp3.Power = 3;

            var goblinDefp4 = SampleCards1.Vanilla;
            goblinDefp4.Cost = 0;
            goblinDefp4.Power = 4;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDefp3, goblinDefp4 }, this.output);

            // 先攻
            var (goblinP3, goblinP4, goblinP3_hand, goblinP4_hand) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblinP3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDefp3.Id);
                var goblinP4 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDefp4.Id);

                // 手札にも追加する
                var goblinP3_hand = await c.GameMaster.GenerateNewCard(
                    goblinDefp3.Id,
                    new Zone(c.Player1.Id, ZoneName.Hand),
                    new InsertCardPosition(InsertCardPosition.PositionTypeValue.Bottom),
                    default);

                var goblinP4_hand = await c.GameMaster.GenerateNewCard(
                    goblinDefp4.Id,
                    new Zone(c.Player1.Id, ZoneName.Hand),
                    new InsertCardPosition(InsertCardPosition.PositionTypeValue.Bottom),
                    default);

                return (goblinP3, goblinP4, goblinP3_hand, goblinP4_hand);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 相手の場に2体
                Assert.Equal(2, c.Player1.Field.Count);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 相手の場のパワー4以上のクリーチャーだけ破壊
                Assert.Equal(ZoneName.Field, goblinP3.Zone.ZoneName);
                Assert.Equal(ZoneName.Cemetery, goblinP4.Zone.ZoneName);

                // 手札のカードも破壊されてる
                Assert.Equal(ZoneName.Hand, goblinP3_hand.Zone.ZoneName);
                Assert.Equal(ZoneName.Cemetery, goblinP4_hand.Zone.ZoneName);
            });

            // 次にドローするカードをパワー4以上にする
            var nextDrawCard = await c.GameMaster.GenerateNewCard(
                goblinDefp4.Id,
                new Zone(c.Player1.Id, ZoneName.Deck),
                new InsertCardPosition(InsertCardPosition.PositionTypeValue.Top),
                default);

            // 先攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ドローしたカードのパワーが4以上なので破壊する
                Assert.Equal(ZoneName.Cemetery, nextDrawCard.Zone.ZoneName);
            });
            // 後攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 次にドローするカードをパワー3にする
            var nextDrawCard2 = await c.GameMaster.GenerateNewCard(
                goblinDefp3.Id,
                new Zone(c.Player1.Id, ZoneName.Deck),
                new InsertCardPosition(InsertCardPosition.PositionTypeValue.Top),
                default);

            // 先攻3
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ドローしたカードのパワーが3なので破壊しない
                Assert.Equal(ZoneName.Hand, nextDrawCard2.Zone.ZoneName);
            });
            // 後攻3
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 次にドローするカードをパワー4以上にする
            var nextDrawCard3 = await c.GameMaster.GenerateNewCard(
                goblinDefp4.Id,
                new Zone(c.Player1.Id, ZoneName.Deck),
                new InsertCardPosition(InsertCardPosition.PositionTypeValue.Top),
                default
                );

            // 先攻4
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ターン経過したのでもう破壊されない
                Assert.Equal(ZoneName.Hand, nextDrawCard3.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task Exclude()
        {
            var testCardDef = SampleCards1.Exclude;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // 除外済みカードなし
                Assert.Single(p.Field.AllCards);
                Assert.Empty(p.Excludes);

                c.TestAnswer.ExpectedCardIdList = new[] { goblin.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Empty(p.Field.AllCards);
                Assert.Single(p.Excludes);
                Assert.Equal(goblinDef.Id, p.Excludes[0].Id);
            });
        }

        [Fact]
        public async Task DDObserver()
        {
            var testCardDef = SampleCards2.DDObserver;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 場に出る以前に除外されていても影響なし
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin, default);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // 攻撃力はもとのまま
                Assert.Equal(testCard.BasePower, testCard.Power);

                // 場のカードが除外されたので攻撃力が1上がる
                await g.ExcludeCard(goblin2, default);
                Assert.Equal(testCard.BasePower + 1, testCard.Power);

                // 場以外のカードが除外されても攻撃力はあがらない
                await g.ExcludeCard(c.Player1.Hands.AllCards[0], default);
                Assert.Equal(testCard.BasePower + 1, testCard.Power);

                return testCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // まだ攻撃力はもとのまま
                Assert.Equal(testCard.BasePower + 1, testCard.Power);

                await g.ExcludeCard(goblin, default);

                // 相手のカードが除外されても攻撃力が上がる
                Assert.Equal(testCard.BasePower + 2, testCard.Power);
            });
        }

        [Fact]
        public async Task DDVisitor()
        {
            var testCardDef = SampleCards2.DDVisitor;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 除外済み:0 なので、ベースのまま
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard.BasePower, testCard.Power);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin, default);

                // 除外済み:1 なので、+1されている
                var testCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(testCard2.BasePower + 1, testCard2.Power);

                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await g.ExcludeCard(goblin3, default);

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
            var testCardDef = SampleCards2.ReturnFromDD;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
                await g.ExcludeCard(goblin, default);

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
            var testCardDef = SampleCards2.DDTransaction;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            goblinDef.Cost = 0;

            var goblinC2Def = SampleCards1.Vanilla;
            goblinC2Def.Cost = 2;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, goblinC2Def });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                // 手札を2コストのカードだけにする（除外されるカードを固定するため）
                await g.Discard(p.Id, p.Hands.AllCards.Select(c => c.Id).ToArray(), default);
                await g.GenerateNewCard(goblinC2Def.Id, new Zone(pId, ZoneName.Hand), default, default);

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
        public async Task DDFighter_攻撃する()
        {
            var testCardDef = SampleCards2.DDFighter;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
        public async Task DDFighter_攻撃される()
        {
            var testCardDef = SampleCards2.DDFighter;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
        public async Task DDFighter_攻撃されて相手が死ぬ()
        {
            var testCardDef = SampleCards2.DDFighter;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
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
        public async Task DDFighter_プレイヤーへ攻撃する()
        {
            var testCardDef = SampleCards2.DDFighter;
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
        public async Task SearchRight()
        {
            var testCardDef = SampleCards1.SearchRight;
            testCardDef.Cost = 0;

            var stealthCardDef = SampleCards1.Vanilla;
            stealthCardDef.Cost = 0;
            stealthCardDef.Abilities = new[] { CreatureAbility.Stealth };

            var noStealthCardDef = SampleCards1.Vanilla;
            noStealthCardDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, stealthCardDef, noStealthCardDef });

            // 先攻
            var (cover, cover2, nocover, nocover2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var coverCard = await TestUtil.NewCardAndPlayFromHand(g, pId, stealthCardDef.Id);
                var coverCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, stealthCardDef.Id);
                var noCoverCard = await TestUtil.NewCardAndPlayFromHand(g, pId, noStealthCardDef.Id);
                var noCoverCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, noStealthCardDef.Id);

                return (coverCard, coverCard2, noCoverCard, noCoverCard2);
            });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var mycover = await TestUtil.NewCardAndPlayFromHand(g, pId, stealthCardDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 相手の場のカバー持ちからカバーがなくなっている
                Assert.Empty(cover.Abilities);
                Assert.Empty(cover2.Abilities);

                // 自分の場のカバーはそのまま
                Assert.Contains(CreatureAbility.Stealth, mycover.Abilities);
                Assert.Equal(mycover.BaseToughness, mycover.Toughness);
            });
        }

        [Fact]
        public async Task MagicObject()
        {
            var testCardDef = SampleCards2.MagicObject;
            testCardDef.Cost = 0;

            var sorceryDef = SampleCards1.Sorcery(0, "");

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
                await g.ModifyCounter(testcard, "魔導", -1, default);
                Assert.Equal(testcard.BasePower, testcard.Power);

                // カウンターが乗ったら+1/+0される
                await g.ModifyCounter(testcard, "魔導", 1, default);
                Assert.Equal(testcard.BasePower + 1, testcard.Power);

                // 2つカウンターが乗ったら+2/+0される
                await g.ModifyCounter(testcard, "魔導", 2, default);
                Assert.Equal(testcard.BasePower + 3, testcard.Power);

                // 2つカウンターが減ったら+2/+0される
                await g.ModifyCounter(testcard, "魔導", -2, default);
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
            var testCardDef = SampleCards2.MagicMonster;

            var sorceryDef = SampleCards1.Sorcery(0, "");

            var c = await TestUtil.InitTest(new[] { testCardDef, sorceryDef });

            // 先攻
            var testcard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var testcard = await g.GenerateNewCard(testCardDef.Id, new Zone(pId, ZoneName.Hand), null, default);
                Assert.Equal(0, testcard.GetCounter("魔導"));
                Assert.Equal(testcard.BasePower, testcard.Power);

                // 魔法を使うとカウンターが1つ乗る。
                await TestUtil.NewCardAndPlayFromHand(g, pId, sorceryDef.Id);
                Assert.Equal(1, testcard.GetCounter("魔導"));

                // カウンターが乗ったらコストが-1
                Assert.Equal(testcard.BaseCost - 1, testcard.Cost);

                // 魔法を使わなくてもカウンターが乗ったらコストが-1
                await g.ModifyCounter(testcard, "魔導", 1, default);
                Assert.Equal(testcard.BaseCost - 2, testcard.Cost);

                // 2つカウンターが乗ったらコストが-2
                await g.ModifyCounter(testcard, "魔導", 2, default);
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
            var testCardDef = SampleCards2.BeginnerSorcerer;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Creature(0, "t", 1, 1, annotations: new[] { ":魔導" });

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef }, this.output);

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                Assert.Equal(0, goblin.GetCounter("魔導"));

                c.TestAnswer.ExpectedCardIdList = new[] { goblin.Id };
                c.TestAnswer.ChoiceCardIdList = new[] { goblin.Id };
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // カウンターが乗っている
                Assert.Equal(2, goblin.GetCounter("魔導"));
            });
        }

        [Fact]
        public async Task GreatSorcerer()
        {
            var testCardDef = SampleCards2.GreatSorcerer;
            var testCardDef_cost0 = SampleCards2.GreatSorcerer;
            testCardDef_cost0.Name = "コスト0";
            testCardDef_cost0.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, testCardDef_cost0 },
                Enumerable.Repeat(testCardDef, 20).ToArray(),
                this.output
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
                // 新たな手札すべてのコストが-5されている
                Assert.True(p.Hands.AllCards.All(c => c.Cost == c.BaseCost - 5));
            });
        }

        [Fact]
        public async Task UltraMagic()
        {
            var testCardDef = SampleCards2.UltraMagic;
            testCardDef.Cost = 0;
            var goblinDef = SampleCards1.Vanilla;
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

                await g.ModifyCounter(goblin1, "魔導", 1, default);

                beforeOpHp = op.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // カウンターが1個なので相手に2ダメージ
                Assert.Equal(beforeOpHp - 2, op.CurrentHp);
                // カウンターがなくなっている
                Assert.Equal(0, goblin1.GetCounter("魔導"));

                await g.ModifyCounter(goblin1, "魔導", 1, default);
                await g.ModifyCounter(goblin2, "魔導", 1, default);

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
            var testCardDef = SampleCards1.Investment;
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

        [Fact]
        public async Task GoblinLover()
        {
            var testCardDef = SampleCards2.GoblinLover;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards2.Goblin;
            goblinDef.Cost = 0;

            var notGoblinDef = SampleCards1.Vanilla;
            notGoblinDef.Cost = 0;

            var c = await TestUtil.InitTest(new[] { testCardDef, goblinDef, notGoblinDef });

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場にカードがないので修整されない
                Assert.Equal(testcard.BasePower, testcard.Power);
                Assert.Equal(testcard.BaseToughness, testcard.Toughness);

                await TestUtil.NewCardAndPlayFromHand(g, pId, notGoblinDef.Id);
                var testcard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場にゴブリンと名のつかないカードしかないので修整されない
                Assert.Equal(testcard2.BasePower, testcard2.Power);
                Assert.Equal(testcard2.BaseToughness, testcard2.Toughness);

                await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場にゴブリンと名のつくカードがあるので修整される
                Assert.Equal(testcard3.BasePower + 1, testcard3.Power);
                Assert.Equal(testcard3.BaseToughness + 1, testcard3.Toughness);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 相手の場にゴブリンカードあっても修整されない
                Assert.Equal(testcard.BasePower, testcard.Power);
                Assert.Equal(testcard.BaseToughness, testcard.Toughness);
            });
        }

        [Fact]
        public async Task SealedGoblin()
        {
            var testCardDef = SampleCards1.SealedGoblin;
            var key1 = SampleCards1.Key1;
            var key2 = SampleCards1.Key2;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, key1, key2 },
                Enumerable.Repeat(testCardDef, 40)
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // 手札に3枚そろってないので勝利しない
                Assert.False(g.GameOver);

                // 手札に3枚そろってないので勝利しない
                await g.GenerateNewCard(key1.Id, new Zone(pId, ZoneName.Hand), null, default);
                Assert.False(g.GameOver);

                // 手札に3枚そろったのでゲームに勝利する
                await g.GenerateNewCard(key1.Id, new Zone(pId, ZoneName.Hand), null, default);
                await g.GenerateNewCard(key2.Id, new Zone(pId, ZoneName.Hand), null, default);
                Assert.True(g.GameOver);
                Assert.Equal(pId, g.WinnerId);
            });
        }

        [Fact]
        public async Task Emergency()
        {
            var testCardDef = SampleCards1.Emergency;
            testCardDef.Cost = 0;

            var goblinDef = SampleCards1.Vanilla;
            var shieldGoblinDef = SampleCards1.Shielder;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, goblinDef, shieldGoblinDef }
                );

            // 先攻
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                Assert.Empty(p.Field.AllCards);

                // 相手の場にカードがないため、発動しない
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Empty(p.Field.AllCards);

                return await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);

                Assert.Empty(p.Field.AllCards);

                // 相手の場にカードがあるため、発動する
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(2, p.Field.AllCards.Count);

                // 自分の場にカードがあるため、発動しない
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(2, p.Field.AllCards.Count);
            });
        }

        [Fact]
        public async Task HealOrDamage()
        {
            var testCardDef = SampleCards1.HealOrDamage;
            testCardDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, }
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var p = g.Get(pId);
                var op = g.GetOpponent(pId);

                // 自分のHPが5以上なので、相手にダメージ
                Assert.Equal(10, op.CurrentHp);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(10 - 3, op.CurrentHp);
                // 自分は回復しない
                Assert.Equal(10, p.CurrentHp);

                // 自分のHPが5以下なので、自分が回復
                await g.DamagePlayer(new Core.Entities.Effect.DamageContext(
                    default,
                    5,
                    GuardPlayer: p,
                    IsBattle: false),
                    default
                    );
                Assert.Equal(5, p.CurrentHp);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                Assert.Equal(10, p.CurrentHp);
                // 相手にダメージなし
                Assert.Equal(10 - 3, op.CurrentHp);
            });
        }

        [Fact]
        public async Task RevengeGoblin()
        {
            var testCardDef = SampleCards2.RevengeGoblin;
            testCardDef.Cost = 0;

            var spellDef = SampleCards1.SelectDamage;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, spellDef }, this.output
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var op = g.GetOpponent(pId);

                var test = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // カードを破壊する
                c.TestAnswer.ChoiceCardIdList = new[] { test.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                // まだ0ダメージ
                Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp, op.CurrentHp);
            });

            // ターン終了したので、相手に1ダメージ
            Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp - 1, c.Player2.CurrentHp);

            // 後攻
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 1度だけなのでもう発動しない
            Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp - 1, c.Player2.CurrentHp);

            // 先攻2
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 1度だけなのでもう発動しない
            Assert.Equal(c.GameMaster.RuleBook.InitialPlayerHp - 1, c.Player2.CurrentHp);
        }

        [Fact]
        public async Task ZombiesStatue()
        {
            var testCardDef = SampleCards2.ZombiesStatue;
            testCardDef.Cost = 0;

            var zombieDef = SampleCards1.Creature(0, "z", 1, 1, annotations: new[] { ":ゾンビ" });
            var nozombieDef = SampleCards1.Creature(0, "z", 1, 1);

            var spellDef = SampleCards1.SelectDamage;
            spellDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, zombieDef, nozombieDef, spellDef }, this.output
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // ゾンビを破壊すると墓地がプラス1される
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var z1 = await TestUtil.NewCardAndPlayFromHand(g, pId, zombieDef.Id);

                var beforeCemeterCount = c.Player1.Cemetery.Count;

                // カードを破壊する
                c.TestAnswer.ChoiceCardIdList = new[] { z1.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                var afterCemeterCount = c.Player1.Cemetery.Count;

                // 効果によって1枚追加で墓地にある
                // 破壊されたカード+スペル+追加のカードで+3される
                Assert.Equal(beforeCemeterCount + 3, afterCemeterCount);

                // ゾンビ以外を破壊しても墓地はプラスされない
                var nz1 = await TestUtil.NewCardAndPlayFromHand(g, pId, nozombieDef.Id);

                beforeCemeterCount = c.Player1.Cemetery.Count;

                // カードを破壊する
                c.TestAnswer.ChoiceCardIdList = new[] { nz1.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, spellDef.Id);

                afterCemeterCount = c.Player1.Cemetery.Count;

                Assert.Equal(beforeCemeterCount + 2, afterCemeterCount);
            });
        }

        [Fact]
        public async Task SelectDeathDamage()
        {
            var testCardDef = SampleCards1.SelectDeathDamage;
            testCardDef.Cost = 0;

            var creatureDef = SampleCards1.Creature(0, "z", 1, 3);

            var c = await TestUtil.InitTest(
                new[] { testCardDef, creatureDef }, this.output
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                // まだ生きてる
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);

                c.TestAnswer.ChoiceCardIdList = new[] { creature.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 破壊される
                Assert.Equal(ZoneName.Cemetery, creature.Zone.ZoneName);
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // バフされてる場合のテスト
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);
                creature.ToughnessBuff = 1;

                // まだ生きてる
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);

                c.TestAnswer.ChoiceCardIdList = new[] { creature.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // バフの分残るので破壊されない
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);
                Assert.Equal(1, creature.Toughness);
            });
        }

        [Fact]
        public async Task RunawayMagic()
        {
            var testCardDef = SampleCards2.RunawayMagic;
            testCardDef.Cost = 0;

            var creatureDef = SampleCards1.Creature(0, "z", 1, 3);

            var c = await TestUtil.InitTest(
                new[] { testCardDef, creatureDef }, this.output
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                // まだ生きてる
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 魔導カウンターが乗っていないのでダメージを受けない
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);
                Assert.Equal(creature.BaseToughness, creature.Toughness);

                var creature2 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);
                await g.ModifyCounter(creature2, "魔導", 1, default);

                // まだ生きてる
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 魔導カウンターが乗っていないのでダメージを受けない
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);
                Assert.Equal(creature.BaseToughness, creature.Toughness);

                // 魔導カウンターが1個のっているので1ダメージ
                Assert.Equal(ZoneName.Field, creature2.Zone.ZoneName);
                Assert.Equal(creature2.BaseToughness - 1, creature2.Toughness);

                var creature3 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);
                await g.ModifyCounter(creature3, "魔導", 2, default);

                // まだ生きてる
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 魔導カウンターが乗っていないのでダメージを受けない
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);
                Assert.Equal(creature.BaseToughness, creature.Toughness);

                // 魔導カウンターが1個のっているのでさらに1ダメージ
                Assert.Equal(ZoneName.Field, creature2.Zone.ZoneName);
                Assert.Equal(creature2.BaseToughness - 2, creature2.Toughness);

                // 魔導カウンターが2個のっているので2ダメージ
                Assert.Equal(ZoneName.Field, creature3.Zone.ZoneName);
                Assert.Equal(creature3.BaseToughness - 2, creature3.Toughness);
            });
        }

        [Fact]
        public async Task ZombieMaster()
        {
            var testCardDef = SampleCards2.ZombieMaster;
            testCardDef.Cost = 0;

            var creatureDef = SampleCards1.Creature(0, "z", 1, 3, annotations: new[] { ":ゾンビ" });
            var damageSpellDef = SampleCards1.SelectDeathDamage;
            damageSpellDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, creatureDef, damageSpellDef }, this.output
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                c.TestAnswer.ChoiceCardIdList = new[] { creature.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, damageSpellDef.Id);

                // 墓地にいる
                Assert.Equal(ZoneName.Cemetery, creature.Zone.ZoneName);

                c.TestAnswer.ChoiceCardIdList = new[] { creature.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場に戻る
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);
                // タフネスは元に戻っている
                Assert.Equal(creature.BaseToughness, creature.Toughness);
            });
        }

        [Fact]
        public async Task LivingDead()
        {
            var testCardDef = SampleCards2.LivingDead;
            testCardDef.Cost = 0;

            var creatureDef = SampleCards1.Creature(0, "z", 1, 3, annotations: new[] { ":ゾンビ" });
            var damageSpellDef = SampleCards1.SelectDeathDamage;
            damageSpellDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, creatureDef, damageSpellDef }, this.output
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                c.TestAnswer.ChoiceCardIdList = new[] { creature.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, damageSpellDef.Id);

                // 墓地にいる
                Assert.Equal(ZoneName.Cemetery, creature.Zone.ZoneName);

                c.TestAnswer.ChoiceCardIdList = new[] { creature.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 場に戻る
                Assert.Equal(ZoneName.Field, creature.Zone.ZoneName);
                // タフネスは1になる
                Assert.Equal(1, creature.Toughness);
            });
        }

        [Fact]
        public async Task PrinceZombie()
        {
            var testCardDef = SampleCards2.PrinceZombie;
            testCardDef.Cost = 0;

            var token = SampleCards2.ZombieToken;

            var creatureDef = SampleCards1.Creature(0, "z", 1, 3);
            var damageSpellDef = SampleCards1.SelectDeathDamage;
            damageSpellDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, token, creatureDef, damageSpellDef }, this.output
                );

            // 先攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ----場のほかのクリーチャーが破壊されたら、場にゾンビトークンを追加する
                // 場には2体だけ
                Assert.Equal(2, c.Player1.Field.Count);

                c.TestAnswer.ChoiceCardIdList = new[] { creature.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, damageSpellDef.Id);

                // 墓地にいる
                Assert.Equal(ZoneName.Cemetery, creature.Zone.ZoneName);
                // 1体追加されるので場に2体いる
                Assert.Equal(2, c.Player1.Field.Count);

                // ----場のゾンビトークン破壊されたら、場にゾンビトークンを追加する
                // 場には2体だけ
                Assert.Equal(2, c.Player1.Field.Count);

                var token = c.Player1.Field.AllCards.First(c => c.Name != testCard.Name);

                c.TestAnswer.ChoiceCardIdList = new[] { token.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, damageSpellDef.Id);

                // 墓地にいる
                Assert.Equal(ZoneName.Cemetery, token.Zone.ZoneName);
                // 1体追加されるので場に2体いる
                Assert.Equal(2, c.Player1.Field.Count);

                // ----自身が破壊されたら、場にゾンビトークは追加されない
                // 場には2体だけ
                Assert.Equal(2, c.Player1.Field.Count);

                c.TestAnswer.ChoiceCardIdList = new[] { testCard.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, damageSpellDef.Id);

                // 墓地にいる
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
                // 1体追加されないので場には1体だけ
                Assert.Equal(1, c.Player1.Field.Count);
            });
        }

        [Fact]
        public async Task MagicBarrier()
        {
            var testCardDef = SampleCards2.MagicBarrier;
            testCardDef.Cost = 0;

            var creatureDef = SampleCards1.Creature(0, "z", 3, 3);
            var damageSpellDef = SampleCards1.SelectDamage;
            damageSpellDef.Cost = 0;

            var c = await TestUtil.InitTest(
                new[] { testCardDef, creatureDef, damageSpellDef }, this.output
                );

            // 先攻
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // プレイしたときカウンターが乗る
                Assert.Equal(5, testCard.CountersByName[":魔導"]);

                return testCard;
            });

            // 後攻
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // クリーチャーに攻撃されたとき
                var creature = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                var beforeHp = c.Player1.CurrentHp;

                await g.AttackToPlayer(pId, creature.Id, c.Player1.Id);

                // 攻撃されたがダメージは0になる
                Assert.Equal(beforeHp, c.Player1.CurrentHp);
                // カウンターがその分減る
                Assert.Equal(2, testCard.CountersByName[":魔導"]);

                // 攻撃以外のダメージを受けたとき
                c.TestAnswer.ChoicePlayerIdList = new[] { c.Player1.Id };
                await TestUtil.NewCardAndPlayFromHand(g, pId, damageSpellDef.Id);

                // 攻撃されたがダメージは0になる
                Assert.Equal(beforeHp, c.Player1.CurrentHp);
                // カウンターがその分減る
                Assert.Equal(1, testCard.CountersByName[":魔導"]);

                // カウンターが0になると破壊される
                // 超過ダメージはうける
                var creature2 = await TestUtil.NewCardAndPlayFromHand(g, pId, creatureDef.Id);

                await g.AttackToPlayer(pId, creature2.Id, c.Player1.Id);

                // 超過ダメージはうける
                Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
                // カウンターがその分減る
                Assert.False(testCard.CountersByName.ContainsKey(":魔導"));
                // カウンターが0になるので破壊される
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }
    }
}
