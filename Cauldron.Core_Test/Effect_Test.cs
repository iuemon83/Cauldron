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
        public async Task �������ɃN���[�`���[��2�̏o���\��()
        {
            var testCard = SampleCards.Creature(0, "�X���C��", "�e�X�g�N���[�`���[", 1, 1, 1,
                effects: new[]
                {
                    // �������A�X���C����2�̏���
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
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCard }) });

            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCard.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCard.Id, 40));

            await testGameMaster.Start(player1Id);
            await TestUtil.AssertGameAction(() => testGameMaster.PlayFromHand(player1Id, testGameMaster.ActivePlayer.Hands.AllCards[0].Id));

            // ��ɂ�3�̏o�Ă��āA����ԃX���C��
            Assert.Equal(3, testGameMaster.ActivePlayer.Field.AllCards.Count);
            foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
            {
                Assert.Equal(testCard.Id, card.CardDefId);
            }
        }

        [Fact]
        public async Task ����ׂɎ�D���P���̂ĂĂ��̃J�[�h�̃R�X�g�����C�t���񕜂���()
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

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var beforenumHands = player1.Hands.AllCards.Count;
                var beforeHp = player1.CurrentHp;
                // ����1�������邩��
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforenumHands - 1, player1.Hands.AllCards.Count);
                Assert.Equal(beforeHp + testCardDef.Cost, player1.CurrentHp);
            });
        }

        //[Fact]
        //public async Task ���S���ɑ���v���C���[��1�_���[�W()
        //{
        //    var goblinDef = SampleCards.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
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

        //        // �U�����̓S�u��������̂���
        //        Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);
        //        Assert.Equal(goblin.Id, g.ActivePlayer.Field.AllCards[0].Id);

        //        // �h�䑤�̓t�B�[���h����
        //        Assert.Equal(0, g.PlayersById[player1Id].Field.AllCards.Count);

        //        // �U���v���C���[�Ɉ�_�_���[�W
        //        Assert.Equal(beforeHp - 1, g.ActivePlayer.CurrentHp);
        //    });
        //}

        //[Fact]
        //public async Task �������Ɏ����̃N���[�`���[�������_���ň�̂��C��()
        //{
        //    var goblinDef = SampleCards.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);
        //    var testCreatureDef = TestCards.;
        //    testCreatureDef.Cost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCreatureDef }) });

        //    var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCreatureDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCreatureDef.Id, 40));

        //    // �S�u�����o���Ă�����ʃN���[�`���[���o��
        //    await testGameMaster.Start(player1Id);

        //    await TestUtil.Turn(testGameMaster, async (g, pid) =>
        //    {
        //        var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pid, goblinDef.Id);
        //        var testCreatureCard = await TestUtil.NewCardAndPlayFromHand(g, pid, testCreatureDef.Id);

        //        // �U������2��
        //        Assert.Equal(2, testGameMaster.ActivePlayer.Field.AllCards.Count);
        //        foreach (var card in testGameMaster.ActivePlayer.Field.AllCards)
        //        {
        //            Assert.Contains(card.CardDefId, new[] { goblinDef.Id, testCreatureDef.Id });
        //        }

        //        // power+2 �̃o�t����Ă�
        //        Assert.Equal(2, goblinCard.PowerBuff);
        //        Assert.Equal(0, goblinCard.ToughnessBuff);

        //        // �����������̓o�t����Ȃ�
        //        Assert.Equal(0, testCreatureCard.PowerBuff);
        //        Assert.Equal(0, testCreatureCard.ToughnessBuff);
        //    });
        //}

        //[Fact]
        //public async Task �^�[���I�����Ƀ����_���ȑ���N���[�`���[1�̂�1�_���[�W_���̌ケ�̃J�[�h��j��()
        //{
        //    var goblin = SampleCards.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

        //    var testCardDef = TestCards.devil;
        //    testCardDef.Cost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

        //    var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

        //    await testGameMaster.Start(player1Id);

        //    // ��U
        //    // �S�u�����Q�̏o��
        //    var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);
        //        var goblinCard2 = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

        //        return new { goblinCard, goblinCard2 };
        //    });

        //    // ��U
        //    // �e�X�g�J�[�h���o��
        //    await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

        //        // �t�B�[���h�ɂ�1��
        //        Assert.Equal(1, g.ActivePlayer.Field.AllCards.Count);

        //        // �܂��S�u�����̓m�[�_��
        //        Assert.Equal(2, cards.goblinCard.Toughness);
        //        Assert.Equal(2, cards.goblinCard2.Toughness);
        //    });

        //    // �j�󂳂��̂Ńt�B�[���h�ɂ�0��
        //    Assert.Equal(0, testGameMaster.PlayersById[player2Id].Field.AllCards.Count);

        //    // �ǂ��炩�̃S�u������1�_���[�W
        //    Assert.True(
        //        cards.goblinCard.Toughness == goblin.Toughness - 1
        //        || cards.goblinCard2.Toughness == goblin.Toughness - 1);
        //}

        //[Fact]
        //public async Task ���肩�����_���ȑ���N���[�`���[��̂�2�_���[�W()
        //{
        //    var goblin = SampleCards.Creature(0, "�S�u����", "�e�X�g�N���[�`���[", 1, 2, 0);

        //    var testCardDef = TestCards.shock;
        //    testCardDef.Cost = 0;

        //    var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
        //    testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblin, testCardDef }) });

        //    var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

        //    var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
        //    var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

        //    await testGameMaster.Start(player1Id);

        //    // ��U
        //    // �S�u�����Q�̏o��
        //    var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var goblinCard = await TestUtil.NewCardAndPlayFromHand(g, pId, goblin.Id);

        //        return new { goblinCard };
        //    });

        //    // ��U
        //    // �e�X�g�J�[�h���o��
        //    await TestUtil.Turn(testGameMaster, async (g, pId) =>
        //    {
        //        var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

        //        // �t�B�[���h�ɂ�0��
        //        Assert.Equal(0, g.ActivePlayer.Field.AllCards.Count);

        //        // �S�u����������v���C���[�Ƀ_���[�W
        //        Assert.True(
        //            cards.goblinCard.Toughness == goblin.Toughness - 2
        //            || testGameMaster.PlayersById[player1Id].CurrentHp == testGameMaster.RuleBook.MaxPlayerHp - 2);
        //    });
        //}

        //[Fact]
        //public void �����̃N���[�`���[�̍U���_���[�W�𑝉�����()
        //{
        //    var goblin = SampleCards.CreatureCard(0, $"test.�S�u����", "�S�u����", "�e�X�g�N���[�`���[", 1, 3);
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
