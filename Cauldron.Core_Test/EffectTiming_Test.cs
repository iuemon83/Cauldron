using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// �J�[�h���ʔ����^�C�~���O�̃e�X�g
    /// </summary>
    public class EffectTiming_Test
    {
        [Fact]
        public async Task ���ׂẴ^�[���J�n��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)))
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return new { testCard };
            });

            Assert.Equal(beforeHp, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, player1.CurrentHp);
        }

        [Fact]
        public async Task �����̃^�[���J�n��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.You)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });

            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp, player1.CurrentHp);

            // ��s
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);
        }

        [Fact]
        public async Task ����̃^�[���J�n��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Opponent)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            Assert.Equal(beforeHp, player1.CurrentHp);

            // ��U
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // ��s
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);
        }

        [Fact]
        public async Task ���ׂẴ^�[���I����()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Both)))
                        ),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            var cards = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);

                return new { testCard };
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, player1.CurrentHp);

            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 3, player1.CurrentHp);
        }

        [Fact]
        public async Task �����̃^�[���I����()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // ��U
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // ��s
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, player1.CurrentHp);
        }

        [Fact]
        public async Task ����̃^�[���I����()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Opponent)))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });
            Assert.Equal(beforeHp, player1.CurrentHp);

            // ��U
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp, player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);

            // ��s
            await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp - 1, player1.CurrentHp);
        }

        [Fact]
        public async Task �J�[�h�̃v���C��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        SampleCards.Spell,
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 1, 1);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp, player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ���������
                Assert.Equal(beforeHp - 1, player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔������Ȃ�
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task ���̃J�[�h�̃v���C��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(Play : new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.Other)))),
                        new[]{  TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 1, 1);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ��������Ȃ�
                Assert.Equal(beforeHp, player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔�������
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�_���[�W�O��_���ׂẴN���[�`���[()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.EventSource.Any,
                                CardCondition : new CardCondition())))),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]{
                                                new PlayerCondition(Type: PlayerCondition.PlayerConditionType.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.ValueModifierOperator.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 5, 0);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            // ��U
            var normal = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = player2.CurrentHp;

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(beforeHp + 2, player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 2, player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(beforeHp + 4, player2.CurrentHp);

                return (testCard, normal2);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 4, player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, normal2.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(beforeHp + 6, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�_���[�W�O��_�������U��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new (
                                Source: EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })))),
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

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 5, 0);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            // ��U
            var normal = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = player2.CurrentHp;

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(beforeHp + 1, player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 1, player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̐퓬���ɔ������Ȃ�
                Assert.Equal(beforeHp + 1, player2.CurrentHp);

                return (testCard, normal2);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 1, player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �U�����ꂽ�Ƃ�����������
                Assert.Equal(beforeHp + 2, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�_���[�W�O��_�ق��J�[�h���h��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })))),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", "test2", 1, 5, 0);

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await testGameMaster.Start(player1.Id);

            // ��U
            var normal = await TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = player2.CurrentHp;

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(beforeHp - 1, player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp - 1, player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̐퓬���ɔ�������
                // �U���Ɩh���2�x��������
                Assert.Equal(beforeHp - 3, player2.CurrentHp);

                return (testCard, normal2);
            });

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp - 3, player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �U�����ꂽ�Ƃ�����������
                Assert.Equal(beforeHp - 4, player2.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�ȊO�̃_���[�W�O��_�������h��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(DamageBefore: new(
                                Source: EffectTimingDamageBeforeEvent.EventSource.Take,
                                CardCondition: new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }
                            )))
                        ),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            // �N���[�`���[��1�_���[�W�̖��@
            var testSorceryDef = SampleCards.Sorcery(0, "test2", "test2",
                effects: new[]
                {
                    new CardEffect(
                        SampleCards.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition()
                                                {
                                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                                },
                                            }))
                                )
                            }
                        }
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef, testSorceryDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testSorceryDef.Id);
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }

        [Fact]
        public async Task �����̔j��()
        {
            var testCardDef = SampleCards.Creature(0, "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouCemetery,
                            new(new(Destroy: new (EffectTimingDestroyEvent.EventSource.This)))
                        ),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var (testGameMaster, player1, player2) = await TestUtil.InitTest(new[] { testCardDef });
            await testGameMaster.Start(player1.Id);

            var beforeHp = player1.CurrentHp;

            // ��U
            await TestUtil.Turn(testGameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, player1.CurrentHp);
                await g.DestroyCard(testCard);
                Assert.Equal(beforeHp - 1, player1.CurrentHp);
            });
        }
    }
}
