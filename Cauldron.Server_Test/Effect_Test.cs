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
        public void 召喚時にクリーチャーを一体出す能力()
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

            // 場には2体出ていて、ぜんぶスライム
            Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public void 召喚時にクリーチャーを2体出す能力()
        {
            var slime = CardDef.Creature(0, $"test.スライム", "スライム", "テストクリーチャー", 1, 1, 1,
                effects: new[]
                {
                    // 召喚時、スライムを2体召喚
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
                                                $"test.スライム",
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

            // 場には3体出ていて、ぜんぶスライム
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public void 死亡時に相手プレイヤーに1ダメージ()
        {
            var goblinDef = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);
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

                // 攻撃側はゴブリンが一体だけ
                Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
                Assert.Equal(goblin.Id, g.ActivePlayer.Field.AllCards[0].Id);

                // 防御側はフィールドが空
                Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

                // 攻撃プレイヤーに一点ダメージ
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

            //// 攻撃側はゴブリンが一体だけ
            //Assert.Equal(1, testGameMaster.ActivePlayer.Field.AllCards.Count);
            //foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            //{
            //    Assert.Equal(goblinDef.Id, card.CardDefId);
            //}

            //// 防御側はフィールドが空
            //Assert.Equal(0, testGameMaster.PlayersById[player1Id].Field.AllCards.Count);

            //// 攻撃プレイヤーに一点ダメージ
            //Assert.Equal(testGameMaster.RuleBook.MaxPlayerHp - 1, testGameMaster.ActivePlayer.Hp);
        }

        [Fact]
        public void 破壊時にフェアリー１枚を手札に加える()
        {
            var goblinDef = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var fairyDef = TestCards.fairy;
            var waterFairyDef = TestCards.waterFairy;
            waterFairyDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { fairyDef, waterFairyDef, goblinDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(waterFairyDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(waterFairyDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            // ウォーターフェアリーを出す
            var waterFairy = TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pid, waterFairyDef.Id);
            });

            var beforeHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Select(c => c.Id).ToArray();

            // 後攻
            // ゴブリン出してウォーターフェアリーに攻撃して破壊する
            TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                var goblin = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                TestUtil.AssertGameAction(() => testGameMaster.AttackToCreature(player2Id,
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
        public void 召喚時に自分のクリーチャーをランダムで一体を修正()
        {
            var goblinDef = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var testCreatureDef = TestCards.whiteGeneral;
            testCreatureDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblinDef, testCreatureDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // ゴブリン出してから効果クリーチャーを出す
            testGameMaster.Start(player1Id);

            TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

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
        public void 召喚時に自分のクリーチャーすべてを修正()
        {
            var goblinDef = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);
            var testCreatureDef = TestCards.commander;
            testCreatureDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblinDef, testCreatureDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreatureDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreatureDef.Id, 40));

            // ゴブリン２体出してから効果クリーチャーを出す
            testGameMaster.Start(player1Id);
            TestUtil.Turn(testGameMaster, (g, pid) =>
            {
                //testGameMaster.StartTurn();
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
                var testCreatureCard = TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

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
        public void ターン終了時にランダムな相手クリーチャー1体に1ダメージ_その後このカードを破壊()
        {
            var goblin = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.devil;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            // ゴブリン２体出す
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard, goblinCard2 };
            });

            // 後攻
            // テストカードを出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

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
                cards.goblinCard.Toughness == goblin.BaseToughness - 1
                || cards.goblinCard2.Toughness == goblin.BaseToughness - 1);
        }

        [Fact]
        public void 相手かランダムな相手クリーチャー一体に2ダメージ()
        {
            var goblin = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.shock;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            // ゴブリン２体出す
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                return new { goblinCard };
            });

            // 後攻
            // テストカードを出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // フィールドには0枚
                Assert.Equal(0, g.ActivePlayer.Field.AllCards.Count);

                // ゴブリンか相手プレイヤーにダメージ
                Assert.True(
                    cards.goblinCard.Toughness == goblin.BaseToughness - 2
                    || testGameMaster.PlayersById[player1Id].CurrentHp == testGameMaster.RuleBook.MaxPlayerHp - 2);
            });
        }

        [Fact]
        public void 対象の自分クリーチャーを修正()
        {
            var goblin = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.buf;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            // カードの選択処理のテスト
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

            // 先攻
            // ゴブリン２体出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.True(goblinCard.PowerBuff == 2 && goblinCard.ToughnessBuff == 2);
            });
        }

        [Fact]
        public void 使用時にすべての自分クリーチャーを修正かつ自分クリーチャーのプレイ時に修正()
        {
            var goblin = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2, 0);

            var testCardDef = TestCards.flag;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 先に場にいたゴブリン2体が修正される
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);

                var goblinCard3 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                // あとに場に出たゴブリンだけが修正される
                Assert.True(goblinCard.PowerBuff == 1 && goblinCard.ToughnessBuff == 0);
                Assert.True(goblinCard2.PowerBuff == 1 && goblinCard2.ToughnessBuff == 0);
                Assert.True(goblinCard3.PowerBuff == 1 && goblinCard3.ToughnessBuff == 0);
            });
        }

        [Fact]
        public void 自分クリーチャーの被ダメージを軽減する()
        {
            var goblin = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = TestCards.shield;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var goblinCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return goblinCard;
            });

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard2 = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                testGameMaster.AttackToCreature(pId, goblinCard2.Id, goblinCard.Id);

                // 1ダメージしか受けない
                Assert.Equal(goblinCard.BaseToughness - 1, goblinCard.Toughness);
            });
        }

        [Fact]
        public void 自分プレイヤーの被ダメージを軽減する()
        {
            var goblin = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = TestCards.wall;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var goblinCard = TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
                testGameMaster.AttackToPlayer(pId, goblinCard.Id, player1Id);

                // 1ダメージしか受けない
                Assert.Equal(g.PlayersById[player1Id].MaxHp - 1, g.PlayersById[player1Id].CurrentHp);
            });
        }

        [Fact]
        public void カードを1枚引く()
        {
            var goblin = CardDef.Creature(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 2, 2, 0);

            var testCardDef = TestCards.hikari;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { goblin, testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
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
        public void このカードが手札から捨てられたら1枚引く()
        {
            var testCardDef = TestCards.hikari;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var beforeNumOfDecks = g.ActivePlayer.Deck.Count;
                var beforeNumOfHands = g.ActivePlayer.Hands.AllCards.Count;

                TestUtil.AssertGameAction(() => g.Discard(pId, new[] { g.ActivePlayer.Hands.AllCards[0].Id }));

                var afterNumOfDecks = g.ActivePlayer.Deck.Count;
                Assert.Equal(beforeNumOfDecks - 1, afterNumOfDecks);

                var afterNumOfHands = g.ActivePlayer.Hands.AllCards.Count;
                // 1枚減って1枚増える
                Assert.Equal(beforeNumOfHands, afterNumOfHands);
            });
        }

        [Fact]
        public void 召喚時に自分プレイヤーを2回復()
        {
            var testCardDef = TestCards.healingAngel;
            testCardDef.BaseCost = 0;

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), (_, c, _) => c, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 事前にダメージ
                g.PlayersById[pId].Damage(5);

                var beforeHp = g.PlayersById[pId].CurrentHp;
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // 2回復している
                Assert.Equal(beforeHp + 2, g.PlayersById[pId].CurrentHp);
            });
        }

        //[Fact]
        //public void 自分のクリーチャーの攻撃ダメージを増加する()
        //{
        //    var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 3);
        //    goblin.TurnCountToCanAttack = 0;

        //    var testCardDef = TestCards.holyKnight;
        //    testCardDef.BaseCost = 0;

        //    var testCardFactory = new CardFactory(new RuleBook());
        //    testCardFactory.SetCardPool(new[] { goblin, testCardDef });

        //    var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null, (_,_) => {});

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

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
