using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// �J�[�h���ʔ������Ԃ̃e�X�g
    /// </summary>
    public class EffectWhile_Test
    {
        [Fact]
        public async Task �^�[���I�����܂�()
        {
            var goblinDef = MessageObjectExtensions.Creature(0, "�S�u����", "", 2, 2);
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(
                                    Source: EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                    CardCondition: new()
                                    {
                                        Context = CardCondition.CardConditionContext.This,
                                        ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                    }
                                    ))),
                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 0, 0)
                        ),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice()
                                    {
                                        How = Choice.ChoiceHow.All,
                                        PlayerCondition = new(Type: PlayerCondition.PlayerConditionType.You),
                                    },
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            // ��U
            var goblin = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            var beforeHp = testGameMaster.PlayersById[player2Id].CurrentHp;

            // ��U
            var testCard = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���ʂ���������
                Assert.Equal(beforeHp, testGameMaster.PlayersById[player2Id].CurrentHp);
                var status = await g.AttackToCreature(pId, testCard.Id, goblin.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(beforeHp + 1, testGameMaster.PlayersById[player2Id].CurrentHp);

                return testCard;
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // �^�[�����܂������̂Ō��ʂ��������Ȃ�
                Assert.Equal(beforeHp + 1, testGameMaster.PlayersById[player2Id].CurrentHp);
                var status = await g.AttackToCreature(pId, goblin.Id, testCard.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(beforeHp + 1, testGameMaster.PlayersById[player2Id].CurrentHp);
            });
        }

        [Fact]
        public async Task ���̎����^�[���J�n��()
        {
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You))),
                            While: new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)), 0, 1)
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            var beforeHp = testGameMaster.PlayersById[player1Id].CurrentHp;

            // 1�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // ���ʂ��������Ȃ�
            Assert.Equal(beforeHp, testGameMaster.PlayersById[player1Id].CurrentHp);

            // 2�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ���ʂ���������
                Assert.Equal(beforeHp - 1, testGameMaster.PlayersById[player1Id].CurrentHp);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // 2�x�ڂ͌��ʂ��������Ȃ�
                Assert.Equal(beforeHp - 1, testGameMaster.PlayersById[player1Id].CurrentHp);
            });
        }

        [Fact]
        public async Task ���̎����^�[���I����()
        {
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You))),
                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 1, 1)
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            var beforeHp = testGameMaster.PlayersById[player1Id].CurrentHp;

            // 1�^�[����
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // 2�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ���ʂ��������Ȃ�
                Assert.Equal(beforeHp, testGameMaster.PlayersById[player1Id].CurrentHp);
            });

            // ���ʂ���������
            Assert.Equal(beforeHp - 1, testGameMaster.PlayersById[player1Id].CurrentHp);

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // 2�x�ڂ͌��ʂ��������Ȃ�
            Assert.Equal(beforeHp - 1, testGameMaster.PlayersById[player1Id].CurrentHp);
        }

        [Fact]
        public async Task �O�^�[����̊J�n����()
        {
            var testCardDef = MessageObjectExtensions.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You))),
                            While: new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)), 2, 1)
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testCardFactory = new CardRepository(TestUtil.TestRuleBook);
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(TestUtil.GameMasterOptions(cardRepository: testCardFactory));

            var (_, player1Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer(PlayerId.NewId(), "player2", Enumerable.Repeat(testCardDef.Id, 40));

            await testGameMaster.Start(player1Id);

            var beforeHp = testGameMaster.PlayersById[player1Id].CurrentHp;

            // 1�^�[����
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // 2�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // ���ʂ��������Ȃ�
            Assert.Equal(beforeHp, testGameMaster.PlayersById[player1Id].CurrentHp);

            // 4�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // ���ʂ���������
                Assert.Equal(beforeHp - 1, testGameMaster.PlayersById[player1Id].CurrentHp);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                // 2�x�ڂ͌��ʂ��������Ȃ�
                Assert.Equal(beforeHp - 1, testGameMaster.PlayersById[player1Id].CurrentHp);
            });
        }
    }
}
