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

            await testGameMaster.StartGame(player1Id);
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var status = await g.AttackToPlayer(pid, testcard.Id, player2Id);

                // 速攻持ちなので攻撃できる
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

            await testGameMaster.StartGame(player1Id);
            var (normal, test) = await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var normalCard = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                return (normalCard, testcard);
            });

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);

                // カバーがいるので攻撃できない
                var status = await g.AttackToCreature(pid, normal2.Id, normal.Id);
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                // カバーには攻撃できる
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

            await testGameMaster.StartGame(player1Id);
            var test = await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                return testcard;
            });

            var (normal2, test2) = await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pid, normalcardDef.Id);
                var test2 = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // ステルスなので攻撃できない
                var status = await g.AttackToCreature(pid, normal2.Id, test.Id);
                Assert.Equal(GameMasterStatusCode.CantAttack, status);

                return (normal2, test2);
            });

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                // 必殺なので倒せる、自分も死ぬ
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
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var tokenDef = SampleCards.KarakuriGoblin;
            var testCardDef = SampleCards.MechanicGoblin;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { tokenDef, testCardDef, goblinDef });

            // 先攻
            // testcardを出す
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCardDef.Id);

                var beforeHands = player1.Hands.AllCards.Select(c => c.Id).ToArray();

                await g.DestroyCard(testcard);

                // 手札にトークンが一枚増える
                var addedHands = player1.Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
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

            await testGameMaster.StartGame(player1Id);
            await TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // 場には2体出ていて、ぜんぶtestcard
            Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(testCard.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task SuperNinjaGoblin()
        {
            var testCard = SampleCards.SuperNinjaGoblin;
            testCard.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(SampleCards.CardsetName, new[] { testCard }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCard.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCard.Id, 40));

            await testGameMaster.StartGame(player1Id);
            await TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // 場には3体出ていて、ぜんぶtestcard
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
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

            await testGameMaster.StartGame(player1Id);

            // 先攻
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
        public async Task GoblinsGreed_手札から捨てる()
        {
            var testCardDef = SampleCards.GoblinsGreed;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeNumFields = player1.Field.Count;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterNumFields = player1.Field.Count;

                // 1枚破壊されているはず
                Assert.Equal(beforeNumFields - 1, afterNumFields);
            });
        }

        [Fact]
        public async Task HealGoblin()
        {
            var testCardDef = SampleCards.HealGoblin;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // 事前にダメージ
                player1.Damage(5);

                var beforeHp = player1.CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 2回復している
                Assert.Equal(beforeHp + 2, player1.CurrentHp);
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef }, TestUtil.GameMasterOptions(
                EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                ));

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            expectedAskPlayerLsit = new[] { player1.Id };
            expectedAskCardLsit = new[] { goblin1, goblin2 };

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHp = g.GetOpponent(pId).CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                var afterHp = g.GetOpponent(pId).CurrentHp;

                Assert.Equal(beforeHp - 2, afterHp);
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

            var (testGameMaster, player1, player2)
                = await TestUtil.InitTest(new[] { testCardDef, cost1Def, cost2Def });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
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
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2);

            var testCardDef = SampleCards.MadScientist;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(
                new[] { goblinDef, testCardDef },
                options: TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: TestUtil.TestPick))
                );

            // 先攻
            var goblin = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(1, player1.Field.Count);
                Assert.Equal(goblin.Id, player1.Field.AllCards[0].Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(1, player1.Field.Count);
                Assert.NotEqual(goblin.Id, player1.Field.AllCards[0].Id);
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ダメージ軽減される
                await g.HitCreature(new(testcard, 3, testcard));
                Assert.Equal(testcard.BaseToughness - 1, testcard.Toughness);

                // ほかクリーチャーの攻撃が強化される
                var beforeHp = player2.CurrentHp;
                await g.AttackToPlayer(pId, goblin.Id, player2.Id);
                var afterHp = player2.CurrentHp;
                Assert.Equal(beforeHp - (goblin.Power + 1), afterHp);

                // 自分の攻撃は強化されない
                beforeHp = player2.CurrentHp;
                await g.AttackToPlayer(pId, testcard.Id, player2.Id);
                afterHp = player2.CurrentHp;
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = player1.CurrentHp;
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 味方クリーチャーにダメージ
                Assert.Equal(goblin.BaseToughness - 3, goblin.Toughness);
                Assert.Equal(goblin2.BaseToughness - 3, goblin2.Toughness);

                // プレイヤーにはダメージなし
                Assert.Equal(beforeHp, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task LeaderGoblin()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var testCreatureDef = SampleCards.LeaderGoblin;
            testCreatureDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // ゴブリン２体出してから効果クリーチャーを出す
            await testGameMaster.StartGame(player1Id);
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pid) =>
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

            await TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                // ゴブリンが2体とも+1/+0 のまま
                Assert.Equal(1, goblin1.PowerBuff);
                Assert.Equal(0, goblin1.ToughnessBuff);
                Assert.Equal(1, goblin2.PowerBuff);
                Assert.Equal(0, goblin2.ToughnessBuff);
            });

            await TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                // ゴブリンが2体とも+2/+0 になる
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ↓で1枚増えるから
                var numHands = player1.Hands.AllCards.Count;
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(testCard.BasePower + numHands, testCard.Power);
                Assert.Equal(testCard.BaseToughness + numHands, testCard.Toughness);
            });
        }

        [Fact]
        public async Task TempRamp()
        {
            var testCardDef = SampleCards.TempRamp;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻1ターン目
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(1, player1.MaxMp);
                Assert.Equal(1, player1.CurrentMp);

                var c = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 最大値が１増える。未使用のMPも増える。
                Assert.Equal(2, player1.MaxMp);
                Assert.Equal(2, player1.CurrentMp);
            });

            // 最大値が１減る。未使用のMPも減る。
            Assert.Equal(1, player1.MaxMp);
            Assert.Equal(1, player1.CurrentMp);

            // 後攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 先攻2ターン目
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(2, player1.MaxMp);
                Assert.Equal(2, player1.CurrentMp);

                var c = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 最大値が１増える。未使用のMPも増える。
                Assert.Equal(3, player1.MaxMp);
                Assert.Equal(3, player1.CurrentMp);
            });

            // 最大値が１減る。未使用のMPも減る。
            Assert.Equal(2, player1.MaxMp);
            Assert.Equal(2, player1.CurrentMp);
        }

        [Fact]
        public async Task Sword()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = SampleCards.Sword;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // カードの選択処理のテスト
            static ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                return ValueTask.FromResult(new ChoiceAnswer(
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

            await testGameMaster.StartGame(player1Id);

            // 先攻
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
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = SampleCards.Shield;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // カードの選択処理のテスト
            static ValueTask<ChoiceAnswer> testAskCardAction(PlayerId _, ChoiceCandidates c, int i)
            {
                return ValueTask.FromResult(new ChoiceAnswer(
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

            await testGameMaster.StartGame(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(
                new[] { testCardDef1, SampleCards.Hit, SampleCards.Heal },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHandIds = player1.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = player1.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef1, goblin1Def, goblin2Def },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var c1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin2Def.Id);

                expectedChoiceCardIdList = new[] { c1.Id, c2.Id, c3.Id };
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHandIds = player2.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = player2.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef1, goblin1Def, goblin2Def },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var c1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin1Def.Id);
                var c3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin2Def.Id);

                expectedChoiceCardIdList = new[] { c1.Id, c2.Id, c3.Id };
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHandIds = player2.Hands.AllCards.Select(c => c.Id).ToArray();

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffHands = player2.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef1, testCardDef2 },
                TestUtil.GameMasterOptions(EventListener: TestUtil.GameEventListener(AskCardAction: testAskCardAction)));

            expectedChoicePlayerIdList = new[] { player2.Id };
            expectedChoiceCardList = Array.Empty<Card>();

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforeHandIds = player1.Hands.AllCards.Select(c => c.Id).ToArray();
                var beforeHp = player2.CurrentHp;

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef1.Id);

                var diffCards = player1.Hands.AllCards.Where(c => !beforeHandIds.Contains(c.Id)).ToArray();
                Assert.Single(diffCards);
                Assert.Equal(testCardDef2.Id, diffCards[0].CardDefId);

                Assert.Equal(beforeHp - 1, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task HolyShield()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, numTurnsToCanAttack: 0);

            var testCardDef = SampleCards.HolyShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // 後攻
            var goblin3 = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ゴブリンで敵を攻撃
                // 攻撃した方はダメージを受けない
                await g.AttackToCreature(pId, goblin3.Id, goblin1.Id);
                Assert.Equal(0, goblin1.Toughness);
                Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);

                return goblin3;
            });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // 次のターンなので、お互いにダメージを受ける
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

            await testGameMaster.StartGame(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
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

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(
                cardRepository: testCardFactory,
                EventListener: TestUtil.GameEventListener(AskCardAction: assertAskAction)
                ));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.StartGame(player1Id);

            // 先攻
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, notGoblinDef, testCardDef });

            // 先攻
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
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
                Assert.Equal(TestUtil.TestRuleBook.InitialPlayerHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task OldShield_クリーチャーの防御時()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // クリーチャーは1ダメージしか受けない
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
                // 破壊される
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_プレイヤーの防御時()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblinCard, goblinCard11, testCard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblinCard11 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblinCard, goblinCard11, testCard);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                await testGameMaster.AttackToPlayer(pId, goblinCard2.Id, player1.Id);

                // プレイヤーは2ダメージ受ける
                Assert.Equal(TestUtil.TestRuleBook.InitialPlayerHp - 2, player1.CurrentHp);
                // 破壊されない
                Assert.Equal(ZoneName.Field, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldShield_攻撃時()
        {
            var goblinDef = SampleCards.Creature(0, "a", "", 2, 2, 0);

            var testCardDef = SampleCards.OldShield;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var goblinCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 自軍クリーチャーも対象になる
                Assert.Equal(goblinCard.BaseToughness - 2, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - 1, goblinCard2.Toughness);
                // 破壊される
                Assert.Equal(ZoneName.Cemetery, testCard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_プレイヤーを攻撃()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var testcard = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = player1.CurrentHp;
                await testGameMaster.AttackToPlayer(pId, goblinCard.Id, player1.Id);

                // 1ダメージしか受けない
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_クリーチャーを攻撃()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblin, testcard) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testcard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return (goblin, testcard);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                var beforeHp = player1.CurrentHp;
                await testGameMaster.AttackToCreature(pId, goblin2.Id, goblin.Id);

                // 1ダメージしか受けない
                Assert.Equal(goblin.BaseToughness - 1, goblin.Toughness);
                Assert.Equal(ZoneName.Cemetery, testcard.Zone.ZoneName);
            });
        }

        [Fact]
        public async Task OldWall_攻撃時()
        {
            var goblinDef = SampleCards.Creature(0, "a", "", 2, 2, 0);

            var testCardDef = SampleCards.OldWall;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var goblinCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return goblinCard;
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 自軍クリーチャーも対象になる
                Assert.Equal(goblinCard.BaseToughness - 2, goblinCard.Toughness);
                Assert.Equal(goblinCard2.BaseToughness - 1, goblinCard2.Toughness);
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

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(
                new[] { goblin, testCardDef },
                Enumerable.Repeat(goblin, 40)
                );

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // 事前にダメージ
                player1.Damage(5);

                var beforeHp = player1.CurrentHp;
                var beforeHandIdList = player1.Hands.AllCards.Select(c => c.Id).ToArray();
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 手札が1枚減っている
                Assert.Equal(beforeHandIdList.Length - 1, player1.Hands.AllCards.Count);

                // 捨てたカードのコスト分ライフが回復している
                Assert.Equal(beforeHp + goblin.Cost, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task GoblinStatue()
        {
            var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 10, 0);

            var testCardDef = SampleCards.GoblinStatue;
            testCardDef.Cost = 0;

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // 後攻
            var goblin3 = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                // 30枚墓地に送る
                foreach (var _ in Enumerable.Range(0, 30))
                {
                    var goblin = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                    await g.DestroyCard(goblin);
                }

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblin3;
            });

            // 相手プレイヤーとクリーチャーに6ダメージ
            Assert.Equal(testGameMaster.RuleBook.InitialPlayerHp - 6, player1.CurrentHp);
            Assert.Equal(goblin1.BaseToughness - 6, goblin1.Toughness);
            Assert.Equal(goblin2.BaseToughness - 6, goblin2.Toughness);

            // 自分プレイヤーと自分クリーチャーはダメージを受けない
            Assert.Equal(testGameMaster.RuleBook.InitialPlayerHp, player2.CurrentHp);
            Assert.Equal(goblin3.BaseToughness, goblin3.Toughness);
        }

        [Fact]
        public async Task HolyStatue()
        {
            var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = SampleCards.HolyStatue;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.StartGame(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 先に場にいたゴブリン2体が修正される
                Assert.True(goblinCard.PowerBuff == 0 && goblinCard.ToughnessBuff == 1);
                Assert.True(goblinCard2.PowerBuff == 0 && goblinCard2.ToughnessBuff == 1);

                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                // あとに場に出たゴブリンだけが修正される
                Assert.True(goblinCard.PowerBuff == 0 && goblinCard.ToughnessBuff == 1);
                Assert.True(goblinCard2.PowerBuff == 0 && goblinCard2.ToughnessBuff == 1);
                Assert.True(goblinCard3.PowerBuff == 0 && goblinCard3.ToughnessBuff == 1);
            });
        }
    }
}
