using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    /// <summary>
    /// �J�[�h���ʔ����^�C�~���O�̃e�X�g
    /// </summary>
    public class EffectTiming_Test
    {
        private record TestEffectAction : EffectAction
        {
            public int CallCount = 0;

            public Action<Card, EffectEventArgs> Action { get; set; }

            public override (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs effectEventArgs)
            {
                this.CallCount++;

                this.Action?.Invoke(ownerCard, effectEventArgs);

                return (true, effectEventArgs);
            }
        }

        [Fact]
        public void ���ׂẴ^�[���J�n��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)))
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);

                return new { testCard };
            });
            Assert.Equal(0, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(2, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);
        }

        [Fact]
        public void �����̃^�[���J�n��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.You)))),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void ����̃^�[���J�n��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Opponent)))),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void ���ׂẴ^�[���I����()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Both)))
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            var cards = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);

                return new { testCard };
            });
            Assert.Equal(1, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);

            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(2, testAction.CallCount);
            });
            Assert.Equal(3, testAction.CallCount);
        }

        [Fact]
        public void �����̃^�[���I����()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You)))),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);
        }

        [Fact]
        public void ����̃^�[���I����()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Opponent)))),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // ��s
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void �J�[�h�̃v���C��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        EffectCondition.Spell,
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 1, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ���������
                Assert.Equal(1, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔������Ȃ�
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void ���̃J�[�h�̃v���C��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(Play : new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.Other)))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 1, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��s
            // ���ʃJ�[�h�o��
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // �v���C���Ɍ��ʂ��������Ȃ�
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // ���̃J�[�h�̃v���C���ɂ͔�������
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void �퓬�J�n�O��_���ׂẴN���[�`���[()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(BattleBefore : new(EffectTimingBattleBeforeEvent.EventSource.All, CardCondition : new CardCondition())))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // �v���C���Ɍ��ʂ��������Ȃ�
                Assert.Equal(0, testAction.CallCount);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(2, testAction.CallCount);

                return (testCard, normal2);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(2, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, normal2.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(3, testAction.CallCount);
            });
        }

        [Fact]
        public void �퓬�J�n�O��_�������U��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(
                                BattleBefore : new(
                                    EffectTimingBattleBeforeEvent.EventSource.Attack,
                                    CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �����̍U�����ɔ�������
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̍U�����ɔ������Ȃ�
                Assert.Equal(1, testAction.CallCount);

                return testCard;
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �����̖h�䎞�ɔ������Ȃ�
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void �퓬�J�n�O��_�������h��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(BattleBefore : new(
                                EffectTimingBattleBeforeEvent.EventSource.Guard,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �����̍U�����ɔ������Ȃ�
                Assert.Equal(0, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̍U�����ɔ������Ȃ�
                Assert.Equal(0, testAction.CallCount);

                return testCard;
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �����̖h�䎞�ɔ�������
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void �퓬�J�n�O��_���J�[�h���U��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(BattleBefore : new(
                                EffectTimingBattleBeforeEvent.EventSource.Attack,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �����̍U�����ɔ������Ȃ�
                Assert.Equal(0, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̍U�����ɔ�������
                Assert.Equal(1, testAction.CallCount);

                return testCard;
            });
        }

        [Fact]
        public void �퓬�J�n�O��_���J�[�h���h��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(BattleBefore : new(
                                EffectTimingBattleBeforeEvent.EventSource.Guard,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �ق��J�[�h�̖h�䎞�ɔ�������
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                g.AttackToCreature(pId, normal2.Id, testCard.Id);
                // �����̖h�䎞�ɔ������Ȃ�
                Assert.Equal(1, testAction.CallCount);

                return testCard;
            });
        }

        [Fact]
        public void �퓬�_���[�W�O��_���ׂẴN���[�`���[()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                EffectTimingDamageBeforeEvent.EventSource.All,
                                CardCondition : new CardCondition())))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 0);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(2, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(2, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(4, testAction.CallCount);

                return (testCard, normal2);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(4, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, normal2.Id);
                // �퓬���Ɍ��ʂ���������
                // �U���Ɩh���2�񔭓�����
                Assert.Equal(6, testAction.CallCount);
            });
        }

        [Fact]
        public void �퓬�_���[�W�O��_�������U��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new (EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 0);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̐퓬���ɔ������Ȃ�
                Assert.Equal(1, testAction.CallCount);

                return (testCard, normal2);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �U�����ꂽ�Ƃ�����������
                Assert.Equal(2, testAction.CallCount);
            });
        }

        [Fact]
        public void �퓬�_���[�W�O��_�ق��J�[�h���h��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new(ZonePrettyName.YouField,
                            new(new(DamageBefore : new(
                                EffectTimingDamageBeforeEvent.EventSource.Guard,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })))),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 0);

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // ��U
            // ���ʃJ�[�h�o��
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // �퓬���Ɍ��ʂ���������
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // �ق��J�[�h�̐퓬���ɔ�������
                // �U���Ɩh���2�x��������
                Assert.Equal(3, testAction.CallCount);

                return (testCard, normal2);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(3, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // �U�����ꂽ�Ƃ�����������
                Assert.Equal(4, testAction.CallCount);
            });
        }

        [Fact]
        public void �퓬�ȊO�̃_���[�W�O��_�������h��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(DamageBefore: new(
                                EffectTimingDamageBeforeEvent.EventSource.Guard,
                                CardCondition: new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }
                            )))
                        ),
                        new[]{ testAction }
                    )
                });

            // �N���[�`���[��1�_���[�W�̖��@
            var testSorceryDef = CardDef.Sorcery(0, $"test.test2", "test2", "test2",
                effects: new[]
                {
                    new CardEffect(
                        EffectCondition.Spell,
                        new[]
                        {
                            new EffectAction()
                            {
                                Damage = new EffectActionDamage()
                                {
                                    Choice = new Choice()
                                    {
                                        CardCondition = new CardCondition()
                                        {
                                            ZoneCondition = new(new[]{ ZonePrettyName.YouField }),
                                            TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                        },
                                    },
                                    Value = 1
                                }
                            }
                        }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef, testSorceryDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                TestUtil.NewCardAndPlayFromHand(g, pId, testSorceryDef.Id);
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void �����̔j��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouCemetery,
                            new(new(Destroy: new (EffectTimingDestroyEvent.EventSource.This)))
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook());
            testCardFactory.SetCardPool(new[] { testCardDef });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                TestUtil.AssertGameAction(() =>
                {
                    g.DestroyCard(testCard);
                    return (true, "");
                });
                Assert.Equal(1, testAction.CallCount);
            });
        }
    }
}
