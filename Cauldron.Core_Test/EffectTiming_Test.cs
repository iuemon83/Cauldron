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
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new()))
                        )),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            var cards = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                return new { testCard };
            });

            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task �����̃^�[���J�n��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });

            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // ��s
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task ����̃^�[���J�n��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(StartTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // ��s
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task ���ׂẴ^�[���I����()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(EndTurn: new()))
                        )),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            var cards = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);

                return new { testCard };
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);

            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 3, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task �����̃^�[���I����()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(EndTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.You)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // ��s
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 2, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task ����̃^�[���I����()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(EndTurn : new(
                                OrPlayerCondition: new[]
                                {
                                    new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                                }))))),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });
            Assert.Equal(beforeHp, c.Player1.CurrentHp);

            // ��U
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                Assert.Equal(beforeHp, c.Player1.CurrentHp);
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

            // ��s
            await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
            });
            Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
        }

        [Fact]
        public async Task �J�[�h�̃v���C��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]{ TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 1);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp, c.Player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ���������
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔������Ȃ�
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task ���̃J�[�h�̃v���C��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(Play : new(
                                OrCardConditions: new[]
                                {
                                    new CardCondition(CardCondition.ContextConditionValue.Others)
                                }))))),
                        new[]{  TestUtil.TestEffectAction }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 1);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��s
            // ���ʃJ�[�h�o��
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ��������Ȃ�
                Assert.Equal(beforeHp, c.Player1.CurrentHp);

                await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔�������
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�_���[�W�O��_���ׂẴN���[�`���[()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5, numTurnsToCanAttack: 0,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.SourceValue.Any,
                                CardCondition : new CardCondition()))))),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]{
                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 5, numTurnsToCanAttack: 0);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            // ��U
            var normal = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = c.Player2.CurrentHp;

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(beforeHp + 2, c.Player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 2, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(beforeHp + 4, c.Player2.CurrentHp);

                return (testCard, normal2);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 4, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, normal2.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(beforeHp + 6, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�_���[�W�O��_�������U��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5, numTurnsToCanAttack: 0,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore : new (
                                Source: EffectTimingDamageBeforeEvent.SourceValue.DamageSource,
                                CardCondition : new CardCondition(
                                    ContextCondition: CardCondition.ContextConditionValue.This
                                    )))))),
                        new[]{
                            new EffectAction(
                                ModifyPlayer: new(
                                    new Choice(
                                        new ChoiceSource(
                                            orPlayerConditions: new[]
                                            {
                                                new PlayerCondition(PlayerCondition.ContextValue.You)
                                            })),
                                    new PlayerModifier(Hp: new NumValueModifier(
                                        NumValueModifier.OperatorValue.Add,
                                        new NumValue(1)
                                        ))))
                        }
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 5, numTurnsToCanAttack: 0);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            // ��U
            var normal = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = c.Player2.CurrentHp;

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̐퓬���ɔ������Ȃ�
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);

                return (testCard, normal2);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp + 1, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �U�����ꂽ�Ƃ�����������
                Assert.Equal(beforeHp + 2, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�_���[�W�O��_�ق��J�[�h���h��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5, numTurnsToCanAttack: 0,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                CardCondition : new CardCondition(
                                    ContextCondition: CardCondition.ContextConditionValue.Others
                                    )))))),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var testNormalCardDef = SampleCards.Creature(0, "test2", 1, 5, numTurnsToCanAttack: 0);

            var c = await TestUtil.InitTest(new[] { testCardDef, testNormalCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            // ��U
            var normal = await TestUtil.Turn(c.GameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
            });

            var beforeHp = c.Player2.CurrentHp;

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(beforeHp - 1, c.Player2.CurrentHp);

                var normal2 = await TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(beforeHp - 1, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̐퓬���ɔ�������
                // �U���Ɩh���2�x��������
                Assert.Equal(beforeHp - 3, c.Player2.CurrentHp);

                return (testCard, normal2);
            });

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                Assert.Equal(beforeHp - 3, c.Player2.CurrentHp);
                await g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �U�����ꂽ�Ƃ�����������
                Assert.Equal(beforeHp - 4, c.Player2.CurrentHp);
            });
        }

        [Fact]
        public async Task �퓬�ȊO�̃_���[�W�O��_�������h��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(ByNotPlay: new (
                            ZonePrettyName.YouField,
                            new(new(DamageBefore: new(
                                Source: EffectTimingDamageBeforeEvent.SourceValue.Take,
                                CardCondition: new CardCondition(
                                    ContextCondition: CardCondition.ContextConditionValue.This
                                )
                            )))
                        )),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            // �N���[�`���[��1�_���[�W�̖��@
            var testSorceryDef = SampleCards.Sorcery(0, "test2", "test2",
                effects: new[]
                {
                    new CardEffect(
                        new EffectConditionWrap(
                            ByPlay: new EffectConditionByPlaying()),
                        new[]
                        {
                            new EffectAction(
                                Damage: new EffectActionDamage(
                                    new NumValue(1),
                                    new Choice(
                                        new ChoiceSource(
                                            orCardConditions: new[]
                                            {
                                                new CardCondition(
                                                    ZoneCondition: new(new(new[]{ ZonePrettyName.YouField })),
                                                    TypeCondition: new CardTypeCondition(new[]{ CardType.Creature })
                                                )
                                            }))
                                ))
                        }
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef, testSorceryDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);
                await TestUtil.NewCardAndPlayFromHand(g, pId, testSorceryDef.Id);
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }

        [Fact]
        public async Task �����̔j��()
        {
            var testCardDef = SampleCards.Creature(0, "test", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new EffectConditionWrap(
                            ByNotPlay: new(
                                ZonePrettyName.YouCemetery,
                                new(new(Destroy: new(
                                    OrCardCondition: new[]
                                    {
                                        new CardCondition(CardCondition.ContextConditionValue.This)
                                    })))
                            )),
                        new[]{ TestUtil.TestEffectAction}
                    )
                });

            var c = await TestUtil.InitTest(new[] { testCardDef });
            await c.GameMaster.StartGame(c.Player1.Id);

            var beforeHp = c.Player1.CurrentHp;

            // ��U
            await TestUtil.Turn(c.GameMaster, async (g, pId) =>
            {
                var testCard = await TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(beforeHp, c.Player1.CurrentHp);
                await g.DestroyCard(testCard);
                Assert.Equal(beforeHp - 1, c.Player1.CurrentHp);
            });
        }
    }
}
