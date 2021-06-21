using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
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
            var goblinDef = SampleCards.Creature(0, "�S�u����", "", 2, 2);
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
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
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var c = await TestUtil.InitTest(new[] { goblinDef, testCardDef });

            // ��U
            var goblin = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                return await TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            var beforeHp = c.Player2.CurrentHp;

            // ��U
            var testCard = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���ʂ���������
                Assert.Equal(beforeHp, c.Player2.CurrentHp);
                var status = await g.AttackToCreature(pId, testCard.Id, goblin.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);

                return testCard;
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                // �^�[�����܂������̂Ō��ʂ��������Ȃ�
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);
                var status = await g.AttackToCreature(pId, goblin.Id, testCard.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task ���̎����^�[���J�n��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
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

            var c = await TestUtil.InitTest(new[] { testCardDef });

            var beforeHp = c.Player1.CurrentHp;

            // 1�^�[����
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ���ʂ��������Ȃ�
            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // 2�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ���ʂ���������
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // 2�x�ڂ͌��ʂ��������Ȃ�
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task ���̎����^�[���I����()
        {
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
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

            var c = await TestUtil.InitTest(new[] { testCardDef });

            var beforeHp = c.Player1.CurrentHp;

            // 1�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 2�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ���ʂ��������Ȃ�
                Assert.Equal(beforeHp, c.Player1.CurrentHp);
            });

            // ���ʂ���������
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 2�x�ڂ͌��ʂ��������Ȃ�
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task �O�^�[����̊J�n����()
        {
            var testCardDef = SampleCards.Creature(0, "test", "", 1, 5,
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

            var c = await TestUtil.InitTest(new[] { testCardDef });

            var beforeHp = c.Player1.CurrentHp;

            // 1�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 2�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // ���ʂ��������Ȃ�
            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // 4�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // ���ʂ���������
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                // 2�x�ڂ͌��ʂ��������Ȃ�
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }
    }
}
