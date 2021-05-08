using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    public class Effect_Test
    {
        [Fact]
        public async Task 召喚時にクリーチャーを2体出す能力()
        {
            var testCard = SampleCards.Creature(0, "スライム", "テストクリーチャー", 1, 1, 1,
                effects: new[]
                {
                    // 召喚時、スライムを2体召喚
                    new CardEffect(
                        SampleCards.Spell,
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
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCard }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCard.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCard.Id, 40));

            await testGameMaster.Start(player1Id);
            await TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // 場には3体出ていて、ぜんぶスライム
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(testCard.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task 無作為に手札を１枚捨ててそのカードのコスト分ライフを回復する()
        {
            var testCardDef = SampleCards.Sorcery(0, "test", "", effects: new[] {
                new CardEffect(
                    SampleCards.Spell,
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
                MaxNumHands: 10, InitialMp: 1, MaxLimitMp: 10, MinMp: 1, LimitMpToIncrease: 1, MaxNumFieldCards: 5, DefaultNumTurnsToCanAttack: 0,
                DefaultNumAttacksLimitInTurn: 1);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef },
                TestUtil.GameMasterOptions(ruleBook: testRulebook));

            // 先攻
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforenumHands = player1.Hands.AllCards.Count;
                var beforeHp = player1.CurrentHp;
                // ↓で1枚増えるから
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforenumHands - 1, player1.Hands.AllCards.Count);
                Assert.Equal(beforeHp + testCardDef.Cost, player1.CurrentHp);
            });
        }

        //[Fact]
        //public async Task 死亡時に相手プレイヤーに1ダメージ()
        //{
        //    var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
        //    var mouseDef = TestCards.mouse;
        //    mouseDef.Cost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { mouseDef, goblinDef }) });

        //    var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(mouseDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(mouseDef.Id, 40));

        //    await testGameMaster.Start(player1Id);

        //    var testCard = await TestUtil.Turn(testGameMaster, (g, pid) =>
        //    {
        //        return TestUtil.NewCardAndPlayFromHand(g, pid, mouseDef.Id);
        //    });

        //    await TestUtil.Turn(testGameMaster, async (g, pid) =>
        //    {
        //        var goblin = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
        //        var beforeHp = g.ActivePlayer.CurrentHp;

        //        await TestUtil.AssertGameAction(() => g.AttackToCreature(pid,
        //            goblin.Id,
        //            testCard.Id
        //            ));

        //        // 攻撃側はゴブリンが一体だけ
        //        Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
        //        Assert.Equal(goblin.Id, g.ActivePlayer.Field.AllCards[0].Id);

        //        // 防御側はフィールドが空
        //        Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

        //        // 攻撃プレイヤーに一点ダメージ
        //        Assert.Equal(beforeHp - 1, g.ActivePlayer.CurrentHp);
        //    });
        //}

        //[Fact]
        //public async Task 召喚時に自分のクリーチャーをランダムで一体を修正()
        //{
        //    var goblinDef = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);
        //    var testCreatureDef = TestCards.;
        //    testCreatureDef.Cost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

        //    var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

        //    // ゴブリン出してから効果クリーチャーを出す
        //    await testGameMaster.Start(player1Id);

        //    await TestUtil.Turn(testGameMaster, async (g, pid) =>
        //    {
        //        var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
        //        var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

        //        // 攻撃側は2体
        //        Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
        //        foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
        //        {
        //            Assert.Contains(card.CardDefId, new[] { goblinDef.Id, testCreatureDef.Id });
        //        }

        //        // power+2 のバフされてる
        //        Assert.Equal(2, goblinCard.PowerBuff);
        //        Assert.Equal(0, goblinCard.ToughnessBuff);

        //        // ただし自分はバフされない
        //        Assert.Equal(0, testCreatureCard.PowerBuff);
        //        Assert.Equal(0, testCreatureCard.ToughnessBuff);
        //    });
        //}

        //[Fact]
        //public async Task ターン終了時にランダムな相手クリーチャー1体に1ダメージ_その後このカードを破壊()
        //{
        //    var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

        //    var testCardDef = TestCards.devil;
        //    testCardDef.Cost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

        //    var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

        //    await testGameMaster.Start(player1Id);

        //    // 先攻
        //    // ゴブリン２体出す
        //    var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
        //        var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

        //        return new { goblinCard, goblinCard2 };
        //    });

        //    // 後攻
        //    // テストカードを出す
        //    await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

        //        // フィールドには1枚
        //        Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);

        //        // まだゴブリンはノーダメ
        //        Assert.Equal(2, cards.goblinCard.Toughness);
        //        Assert.Equal(2, cards.goblinCard2.Toughness);
        //    });

        //    // 破壊されるのでフィールドには0体
        //    Assert.Equal(0, testGameMaster.PlayersById[player2Id].Field.AllCards.Count);

        //    // どちらかのゴブリンが1ダメージ
        //    Assert.True(
        //        cards.goblinCard.Toughness == goblin.Toughness - 1
        //        || cards.goblinCard2.Toughness == goblin.Toughness - 1);
        //}

        //[Fact]
        //public async Task 相手かランダムな相手クリーチャー一体に2ダメージ()
        //{
        //    var goblin = SampleCards.Creature(0, "ゴブリン", "テストクリーチャー", 1, 2, 0);

        //    var testCardDef = TestCards.shock;
        //    testCardDef.Cost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

        //    var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

        //    await testGameMaster.Start(player1Id);

        //    // 先攻
        //    // ゴブリン２体出す
        //    var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

        //        return new { goblinCard };
        //    });

        //    // 後攻
        //    // テストカードを出す
        //    await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

        //        // フィールドには0枚
        //        Assert.Equal(0, g.ActivePlayer.Field.AllCards.Count);

        //        // ゴブリンか相手プレイヤーにダメージ
        //        Assert.True(
        //            cards.goblinCard.Toughness == goblin.Toughness - 2
        //            || testGameMaster.PlayersById[player1Id].CurrentHp == testGameMaster.RuleBook.MaxPlayerHp - 2);
        //    });
        //}

        //[Fact]
        //public void 自分のクリーチャーの攻撃ダメージを増加する()
        //{
        //    var goblin = SampleCards.CreatureCard(0, $"test.ゴブリン", "ゴブリン", "テストクリーチャー", 1, 3);
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
