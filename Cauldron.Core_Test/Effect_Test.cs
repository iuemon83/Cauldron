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
        public async Task 召喚時にクリーチャーを一体出す能力()
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

            // 場には2体出ていて、ぜんぶスライム
            Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task 召喚時にクリーチャーを2体出す能力()
        {
            var slime = MessageObjectExtensions.Creature(0, "スライム", "テストクリーチャー", 1, 1, 1,
                effects: new[]
                {
                    // 召喚時、スライムを2体召喚
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
                                                new TextValue($"Test.スライム"),
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

            // 場には3体出ていて、ぜんぶスライム
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task 死亡時に相手プレイヤーに1ダメージ()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
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

                // 攻撃側はゴブリンが一体だけ
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
                Assert.Equal(goblin.Id, g.ActivePlayer.Field.AllCards[0].Id);

                // 防御側はフィールドが空
                Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

                // 攻撃プレイヤーに一点ダメージ
                Assert.Equal(beforeHp - 1, g.ActivePlayer.CurrentHp);
            });
        }

        [Fact]
        public async Task 破壊時にフェアリー１枚を手札に加える()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var fairyDef = TestCards.fairy;
            var waterFairyDef = TestCards.waterFairy;
            waterFairyDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet(TestCards.CardsetName, new[] { fairyDef, waterFairyDef, goblinDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(waterFairyDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(waterFairyDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            // ウォーターフェアリーを出す
            var waterFairy = await TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pid, waterFairyDef.Id);
            });

            var beforeHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Select(c => c.Id).ToArray();

            // 後攻
            // ゴブリン出してウォーターフェアリーに攻撃して破壊する
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                await TestUtil.AssertGameAction(() => testGameMaster.AttackToCreature(player2Id,
                    goblin.Id,
                    waterFairy.Id
                    ));

                // 攻撃側はゴブリンが一体だけ
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
                foreach (var card in g.ActivePlayer.Field.AllCards)
                {
                    Assert.Equal(goblinDef.Id, card.CardDefId);
                }

                // 防御側はフィールドが空
                Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

                // 防御側の手札にフェアリーが一枚増える
                var addedHands = g.PlayersById[player1Id].Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
                Assert.Single(addedHands);
                Assert.Equal(fairyDef.Id, addedHands[0].CardDefId);
            });
        }

        [Fact]
        public async Task 召喚時に自分のクリーチャーをランダムで一体を修正()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var testCreatureDef = TestCards.whiteGeneral;
            testCreatureDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // ゴブリン出してから効果クリーチャーを出す
            await testGameMaster.Start(player1Id);

            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

                // 攻撃側は2体
                Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
                foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
                {
                    Assert.Contains(card.CardDefId, new[] { goblinDef.Id, testCreatureDef.Id });
                }

                // power+2 のバフされてる
                Assert.Equal(2, goblinCard.PowerBuff);
                Assert.Equal(0, goblinCard.ToughnessBuff);

                // ただし自分はバフされない
                Assert.Equal(0, testCreatureCard.PowerBuff);
                Assert.Equal(0, testCreatureCard.ToughnessBuff);
            });
        }

        [Fact]
        public async Task 召喚時に自分のクリーチャーすべてを修正()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var testCreatureDef = TestCards.commander;
            testCreatureDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // ゴブリン２体出してから効果クリーチャーを出す
            await testGameMaster.Start(player1Id);
            await TestUtil.Turn(testGameMaster, async (g, pid) =>
            {
                //testGameMaster.StartTurn();
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

                // 攻撃側は3体
                Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
                foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
                {
                    Assert.Contains(card.CardDefId, new[] { goblinDef.Id, testCreatureDef.Id });
                }

                // ゴブリンが2体とも+1/+2 されている
                Assert.Equal(1, goblinCard.PowerBuff);
                Assert.Equal(2, goblinCard.ToughnessBuff);
                Assert.Equal(1, goblinCard2.PowerBuff);
                Assert.Equal(2, goblinCard2.ToughnessBuff);

                // ただし自分はバフされない
                Assert.Equal(0, testCreatureCard.PowerBuff);
                Assert.Equal(0, testCreatureCard.ToughnessBuff);
            });
        }

        [Fact]
        public async Task ターン終了時にランダムな相手クリーチャー1体に1ダメージ_その後このカードを破壊()
        {
            var goblin = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.devil;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            // ゴブリン２体出す
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard, goblinCard2 };
            });

            // 後攻
            // テストカードを出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // フィールドには1枚
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);

                // まだゴブリンはノーダメ
                Assert.Equal(2, cards.goblinCard.Toughness);
                Assert.Equal(2, cards.goblinCard2.Toughness);
            });

            // 破壊されるのでフィールドには0体
            Assert.Equal(0, testGameMaster.PlayersById[player2Id].Field.AllCards.Count);

            // どちらかのゴブリンが1ダメージ
            Assert.True(
                cards.goblinCard.Toughness == goblin.Toughness - 1
                || cards.goblinCard2.Toughness == goblin.Toughness - 1);
        }

        [Fact]
        public async Task 相手かランダムな相手クリーチャー一体に2ダメージ()
        {
            var goblin = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.shock;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            // ゴブリン２体出す
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard };
            });

            // 後攻
            // テストカードを出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // フィールドには0枚
                Assert.Equal(0, g.ActivePlayer.Field.AllCards.Count);

                // ゴブリンか相手プレイヤーにダメージ
                Assert.True(
                    cards.goblinCard.Toughness == goblin.Toughness - 2
                    || testGameMaster.PlayersById[player1Id].CurrentHp == testGameMaster.RuleBook.MaxPlayerHp - 2);
            });
        }

        [Fact]
        public async Task 対象の自分クリーチャーを修正()
        {
            var goblin = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.buf;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            // カードの選択処理のテスト
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

            // 先攻
            // ゴブリン２体出す
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.True(goblinCard.PowerBuff == 2 && goblinCard.ToughnessBuff == 2);
            });
        }

        [Fact]
        public async Task 使用時にすべての自分クリーチャーを修正かつ自分クリーチャーのプレイ時に修正()
        {
            var goblin = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.flag;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 先に場にいたゴブリン2体が修正される
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);

                var goblinCard3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                // あとに場に出たゴブリンだけが修正される
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);
                Assert.True(goblinCard3.PowerBuff == 1 && goblinCard3.ToughnessBuff == 0);
            });
        }

        [Fact]
        public async Task 自分クリーチャーの被ダメージを軽減する()
        {
            var goblin = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = TestCards.shield;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            var goblinCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblinCard;
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                await testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 1ダメージしか受けない
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
            });
        }

        [Fact]
        public async Task 自分プレイヤーの被ダメージを軽減する()
        {
            var goblin = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = TestCards.wall;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var beforeHp = g.PlayersById[player1Id].CurrentHp;
                await testGameMaster.AttackToPlayer(pId, goblinCard.Id, player1Id);

                // 1ダメージしか受けない
                Assert.Equal(beforeHp - 1, g.PlayersById[player1Id].CurrentHp);
            });
        }

        [Fact]
        public async Task カードを1枚引く()
        {
            var goblin = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = TestCards.hikari;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
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
        public async Task カードをすべて捨てて同じ枚数ドローする()
        {
            var testCardDef = TestCards.unmei;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

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
        public async Task このカードが手札から捨てられたら1枚引く()
        {
            var testCardDef = TestCards.hikari;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

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
        public async Task 召喚時に自分プレイヤーを2回復()
        {
            var testCardDef = TestCards.healingAngel;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // 事前にダメージ
                g.PlayersById[pId].Damage(5);

                var beforeHp = g.PlayersById[pId].CurrentHp;
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 2回復している
                Assert.Equal(beforeHp + 2, g.PlayersById[pId].CurrentHp);
            });
        }

        [Fact]
        public async Task 召喚時に自軍クリーチャーに効果を付与する()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2);

            var testCardDef = TestCards.atena;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);

            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            var (goblin1, goblin2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin1 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var goblin2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);

                return (goblin1, goblin2);
            });

            // 後攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var goblin3 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ゴブリンで敵を攻撃
                // 攻撃した方はダメージを受けない
                await g.AttackToCreature(pId, goblin3.Id, goblin1.Id);
                Assert.Equal(0, goblin1.Toughness);
                Assert.Equal(goblinDef.Toughness, goblin3.Toughness);

                // テストカードで敵を攻撃
                // 自分には効果が付与されないので、ダメージを受ける
                await g.AttackToCreature(pId, testCard.Id, goblin2.Id);
                Assert.Equal(0, goblin2.Toughness);
                Assert.Equal(testCardDef.Toughness - goblin2.Power, testCard.Toughness);
            });
        }

        [Fact]
        public async Task 手札をすべて捨てその枚数だけ自分を強化()
        {
            var testCardDef = TestCards.tenyoku;
            testCardDef.Cost = 0;

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ↓で1枚増えるから
                var numHands = g.PlayersById[pId].Hands.AllCards.Count;
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(testCard.BasePower + numHands, testCard.Power);
                Assert.Equal(testCard.BaseToughness + numHands, testCard.Toughness);
            });
        }

        [Fact]
        public async Task 無作為に手札を１枚捨ててそのカードのコスト分ライフを回復する()
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

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforenumHands = g.PlayersById[pId].Hands.AllCards.Count;
                var beforeHp = g.PlayersById[pId].CurrentHp;
                // ↓で1枚増えるから
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforenumHands - 1, g.PlayersById[pId].Hands.AllCards.Count);
                Assert.Equal(beforeHp + testCardDef.Cost, g.PlayersById[pId].CurrentHp);
            });
        }

        [Fact]
        public async Task クリーチャーを一体破壊して同じ場にコピーを出す()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "ゴブリン", "テストクリーチャー", 2, 2);

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

            // 先攻
            var goblin = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // 後攻
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
        //public void 自分のクリーチャーの攻撃ダメージを増加する()
        //{
        //    var goblin = MessageObjectExtensions.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 3);
        //    goblin.TurnCountToCanAttack = 0;

        //    var testCardDef = TestCards.holyKnight;
        //    testCardDef.BaseCost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { goblin, testCardDef });

        //    var testGameMaster = new GameMaster(TestUtil.TestRuleBook, testCardFactory, new TestLogger(), null, (_,_) => {});

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

        //    testGameMaster.Start(player1Id);

        //    // 先攻
        //    var goblinCard = TestUtil.Turn(testGameMaster, (g, pId) =>
        //    {
        //        return TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
        //    });

        //    // 後攻
        //    TestUtil.Turn(testGameMaster, (g, pId) =>
        //    {
        //        var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
        //        TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

        //        // クリーチャーへ攻撃
        //        g.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);
        //        Assert.Equal(goblinCard.BaseToughness, goblinCard.Toughness);

        //        // プレイヤーへ攻撃
        //        g.AttackToPlayer(pId, goblinCard2.Id, player1Id);
        //        Assert.Equal(g.PlayersById[player1Id].MaxHp, g.PlayersById[player1Id].Hp);
        //    });
        //}
    }
}
