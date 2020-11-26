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
            var slime = CardDef.CreatureCard(0, $"test.スライム", "スライム", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    // 召喚時、スライムを一体召喚
                    new CardEffect2()
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.Card },
                                        CardCondition = new CardCondition()
                                        {
                                            NameCondition = new TextCondition()
                                            {
                                                Value = $"test.スライム",
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(slime.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(slime.Id, 40));

            testGameMaster.Start(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.CurrentPlayer.Hands.AllCards[0].Id));

            // 場には2体出ていて、ぜんぶスライム
            Assert.Equal(2, testGameMaster.CurrentPlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.CurrentPlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public void 召喚時にクリーチャーを2体出す能力()
        {
            var slime = CardDef.CreatureCard(0, $"test.スライム", "スライム", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    // 召喚時、スライムを2体召喚
                    new CardEffect2()
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.Card },
                                        CardCondition = new CardCondition()
                                        {
                                            NameCondition = new TextCondition()
                                            {
                                                Value = $"test.スライム",
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(slime.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(slime.Id, 40));

            testGameMaster.Start(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.CurrentPlayer.Hands.AllCards[0].Id));

            // 場には3体出ていて、ぜんぶスライム
            Assert.Equal(3, testGameMaster.CurrentPlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.CurrentPlayer.Field.AllCards)
            {
                Assert.Equal(slime.Id, card.CardDefId);
            }
        }

        [Fact]
        public void 死亡時に相手プレイヤーに1ダメージ()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var mouse = CardDef.CreatureCard(0, $"test.ネズミ", "ネズミ", "テストクリーチャー", 1, 1,
                effects: new[]
                {
                    // 死亡時、相手プレイヤーに1ダメージ
                    new CardEffect2(){
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent(){
                                Owner = EffectTimingDestroyEvent.EventOwner.This
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.OtherOwnerPlayer },
                                        NumPicks=1
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(mouse.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(mouse.Id, 40));

            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.CurrentPlayer.Hands.AllCards[0].Id));
            TestUtil.AssertPhase(() => testGameMaster.EndTurn(player1Id));

            testGameMaster.StartTurn(player2Id);
            var newHandCard = testGameMaster.GenerateNewCard(goblin.Id, testGameMaster.CurrentPlayer.Id);
            testGameMaster.AddHand(testGameMaster.CurrentPlayer, newHandCard);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player2Id, newHandCard.Id));
            TestUtil.AssertPhase(() => testGameMaster.AttackToCreature(player2Id,
                newHandCard.Id,
                testGameMaster.PlayersById[player1Id].Field.AllCards[0].Id
                ));

            // 攻撃側はゴブリンが一体だけ
            Assert.Equal(1, testGameMaster.CurrentPlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.CurrentPlayer.Field.AllCards)
            {
                Assert.Equal(goblin.Id, card.CardDefId);
            }

            // 防御側はフィールドが空
            Assert.Equal(0, testGameMaster.PlayersById[player1Id].Field.AllCards.Count);

            // 攻撃プレイヤーに一点ダメージ
            Assert.Equal(testGameMaster.RuleBook.MaxPlayerHp - 1, testGameMaster.CurrentPlayer.Hp);
        }

        [Fact]
        public void 破壊時にフェアリー１枚を手札に加える()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var fairy = CardDef.TokenCard(0, $"test.フェアリー", "フェアリー", "テストクリーチャー", 1, 1);
            var waterFairy = CardDef.CreatureCard(0, $"test.ウォーターフェアリー", "ウォーターフェアリー", "テストクリーチャー", 1, 1,

                effects: new[]
                {
                    // 破壊時、フェアリー１枚を手札に加える
                    new CardEffect2()
                    {
                        Timing = new EffectTiming()
                        {
                            Destroy = new EffectTimingDestroyEvent()
                            {
                                Owner = EffectTimingDestroyEvent.EventOwner.This
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.Card },
                                        CardCondition = new CardCondition()
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(waterFairy.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(waterFairy.Id, 40));

            // ウォーターフェアリーを出す
            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.CurrentPlayer.Hands.AllCards[0].Id));
            TestUtil.AssertPhase(() => testGameMaster.EndTurn(player1Id));

            var beforeHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Select(c => c.Id).ToArray();

            // ゴブリン出してウォーターフェアリーに攻撃して破壊する
            testGameMaster.StartTurn(player2Id);
            var newHandCard = testGameMaster.GenerateNewCard(goblin.Id, testGameMaster.CurrentPlayer.Id);
            testGameMaster.AddHand(testGameMaster.CurrentPlayer, newHandCard);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player2Id, newHandCard.Id));
            TestUtil.AssertPhase(() => testGameMaster.AttackToCreature(player2Id,
                newHandCard.Id,
                testGameMaster.PlayersById[player1Id].Field.AllCards[0].Id
                ));

            // 攻撃側はゴブリンが一体だけ
            Assert.Equal(1, testGameMaster.CurrentPlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.CurrentPlayer.Field.AllCards)
            {
                Assert.Equal(goblin.Id, card.CardDefId);
            }

            // 防御側はフィールドが空
            Assert.Equal(0, testGameMaster.PlayersById[player1Id].Field.AllCards.Count);

            // 防御側の手札にフェアリーが一枚増える
            var addedHands = testGameMaster.PlayersById[player1Id].Hands.AllCards.Where(c => !beforeHands.Contains(c.Id)).ToArray();
            Assert.Single(addedHands);
            Assert.Equal(fairy.Id, addedHands[0].CardDefId);
        }

        [Fact]
        public void 召喚時に自分のクリーチャーをランダムで一体を修正()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var testCreature = CardDef.CreatureCard(0, $"test.テストクリーチャー", "テストクリーチャー", "テストクリーチャー", 2, 2,
                effects: new[]
                {
                    // 召喚時、自分のクリーチャーをランダムで一体を+2/+1
                    new CardEffect2()
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
                                        Candidates=new[]{ Choice.ChoiceCandidateType.Card },
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreature.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreature.Id, 40));

            // ゴブリン出してから効果クリーチャーを出す
            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            var goblinCard = testGameMaster.GenerateNewCard(goblin.Id, player1Id);
            testGameMaster.AddHand(testGameMaster.CurrentPlayer, goblinCard);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, goblinCard.Id));

            var testCreatureCard = testGameMaster.CurrentPlayer.Hands.AllCards[0];
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testCreatureCard.Id));

            // 攻撃側は2体
            Assert.Equal(2, testGameMaster.CurrentPlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.CurrentPlayer.Field.AllCards)
            {
                Assert.Contains(card.CardDefId, new[] { goblin.Id, testCreature.Id });
            }

            // power+2 のバフされてる
            Assert.Equal(2, goblinCard.PowerBuff);
            Assert.Equal(1, goblinCard.ToughnessBuff);

            // ただし自分はバフされない
            Assert.Equal(0, testCreatureCard.PowerBuff);
            Assert.Equal(0, testCreatureCard.ToughnessBuff);
        }

        [Fact]
        public void 召喚時に自分のクリーチャーすべてを修正()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;
            var testCreature = CardDef.CreatureCard(0, $"test.セージコマンダー", "セージコマンダー", "テストクリーチャー", 3, 3,
                effects: new[]
                {
                    // 召喚時、自分のクリーチャーすべてを+1/+2
                    new CardEffect2()
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
                                        Candidates =new[]{ Choice.ChoiceCandidateType.Card },
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCreature.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCreature.Id, 40));

            // ゴブリン２体出してから効果クリーチャーを出す
            testGameMaster.Start(player1Id);
            testGameMaster.StartTurn(player1Id);
            var goblinCard = testGameMaster.GenerateNewCard(goblin.Id, player1Id);
            var goblinCard2 = testGameMaster.GenerateNewCard(goblin.Id, player1Id);
            testGameMaster.AddHand(testGameMaster.CurrentPlayer, goblinCard);
            testGameMaster.AddHand(testGameMaster.CurrentPlayer, goblinCard2);
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, goblinCard.Id));
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, goblinCard2.Id));

            var testCreatureCard = testGameMaster.CurrentPlayer.Hands.AllCards[0];
            TestUtil.AssertPhase(() => testGameMaster.PlayFromHand(player1Id, testCreatureCard.Id));

            // 攻撃側は3体
            Assert.Equal(3, testGameMaster.CurrentPlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.CurrentPlayer.Field.AllCards)
            {
                Assert.Contains(card.CardDefId, new[] { goblin.Id, testCreature.Id });
            }

            // ゴブリンが2体とも+1/+2 されている
            Assert.Equal(1, goblinCard.PowerBuff);
            Assert.Equal(2, goblinCard.ToughnessBuff);
            Assert.Equal(1, goblinCard2.PowerBuff);
            Assert.Equal(2, goblinCard2.ToughnessBuff);

            // ただし自分はバフされない
            Assert.Equal(0, testCreatureCard.PowerBuff);
            Assert.Equal(0, testCreatureCard.ToughnessBuff);
        }

        [Fact]
        public void ターン終了時にランダムな相手クリーチャー1体に1ダメージ_その後このカードを破壊()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                new[]
                {
                    // ターン終了時、ランダムな相手クリーチャー一体に1ダメージ。その後このカードを破壊
                    new CardEffect2()
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
                                        Candidates = new[]{Choice.ChoiceCandidateType.Card},
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
                                        Candidates = new[]{ Choice.ChoiceCandidateType.Card },
                                        How = Choice.ChoiceHow.All,
                                        CardCondition = new CardCondition()
                                        {
                                            Context = CardCondition.CardConditionContext.Me,
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

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
                Assert.Equal(1, g.CurrentPlayer.Field.AllCards.Count);

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
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                effects: new[]
                {
                    // 使用時、相手かランダムな相手クリーチャー一体に2ダメージ
                    new CardEffect2()
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
                                        Candidates = new []{Choice.ChoiceCandidateType.Card, Choice.ChoiceCandidateType.OtherOwnerPlayer},
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

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

                // フィールドには1枚
                Assert.Equal(1, g.CurrentPlayer.Field.AllCards.Count);

                // ゴブリンか相手プレイヤーにダメージ
                Assert.True(
                    cards.goblinCard.Toughness == goblin.BaseToughness - 2
                    || testGameMaster.PlayersById[player1Id].Hp == testGameMaster.RuleBook.MaxPlayerHp - 2);
            });
        }

        [Fact]
        public void 対象の自分クリーチャーを修正()
        {
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                effects: new[]
                {
                    // 使用時、対象の自分クリーチャーを+2/+2
                    new CardEffect2()
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
                                        Candidates =new []{Choice.ChoiceCandidateType.Card },
                                        CardCondition=new CardCondition()
                                        {
                                            ZoneCondition= ZoneType.YouField,
                                            TypeCondition =new CardTypeCondition()
                                            {
                                                Value= new[]{CardType.Creature, CardType.Token }
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

            // カードの選択処理のテスト
            static ChoiceResult testAskCardAction(Guid _, ChoiceResult c, int i)
            {
                return new ChoiceResult()
                {
                    CardList = c.CardList.Take(1).ToArray()
                };
            }

            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), testAskCardAction);

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

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
            var goblin = CardDef.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 2);
            goblin.TurnCountToCanAttack = 0;

            var testCardDef = CardDef.ArtifactCard(0, $"test.test", "test", "test",
                effects: new[]
                {
                    // 使用時、すべての自分クリーチャーを+1/+0
                    new CardEffect2()
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
                                        Candidates = new[]{ Choice.ChoiceCandidateType.Card },
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
                    // 自分クリーチャーのプレイ時+1/+0
                    new CardEffect2()
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
                                        Candidates = new[]{ Choice.ChoiceCandidateType.Card },
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

            var player1Id = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var player2Id = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

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
    }
}
