using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    /// <summary>
    /// �J�[�h���ʔ������Ԃ̃e�X�g
    /// </summary>
    public class EffectWhile_Test
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
        public void �^�[���I�����܂�()
        {
            var goblinDef = CardDef.Creature(0, "�S�u����", "", 2, 2);
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(
                                DamageBefore: new(EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                CardCondition: new()
                                {
                                    Context = CardCondition.CardConditionContext.This,
                                    ZoneCondition = new(new(new[]{ ZonePrettyName.YouField })),
                                }
                                ))),
                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 0, 0)
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook(
                DefaultNumTurnsToCanAttack: 0));
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { goblinDef, testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // ��U
            var goblin = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                return TestUtil.NewCardAndPlayFromHand(g, pId, goblinDef.Id);
            });

            // ��U
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                // ���ʂ���������
                Assert.Equal(0, testAction.CallCount);
                var status = g.AttackToCreature(pId, testCard.Id, goblin.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(1, testAction.CallCount);

                return testCard;
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // �^�[�����܂������̂Ō��ʂ��������Ȃ�
                Assert.Equal(1, testAction.CallCount);
                var status = g.AttackToCreature(pId, goblin.Id, testCard.Id);
                Assert.Equal(GameMasterStatusCode.OK, status);
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void ���̎����^�[���J�n��()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You))),
                            While: new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)), 0, 1)
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook(
                DefaultNumTurnsToCanAttack: 0));
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 1�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // ���ʂ��������Ȃ�
            Assert.Equal(0, testAction.CallCount);

            // 2�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // ���ʂ���������
                Assert.Equal(1, testAction.CallCount);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 2�x�ڂ͌��ʂ��������Ȃ�
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void ���̎����^�[���I����()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You))),
                            While: new(new(EndTurn: new(EffectTimingEndTurnEvent.EventSource.You)), 1, 1)
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook(
                DefaultNumTurnsToCanAttack: 0));
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 1�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 2�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // ���ʂ��������Ȃ�
                Assert.Equal(0, testAction.CallCount);
            });

            // ���ʂ���������
            Assert.Equal(1, testAction.CallCount);

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 2�x�ڂ͌��ʂ��������Ȃ�
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void �O�^�[����̊J�n����()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, "test", "", 1, 5,
                effects: new[]{
                    new CardEffect(
                        new(
                            ZonePrettyName.YouField,
                            new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You))),
                            While: new(new(StartTurn: new(EffectTimingStartTurnEvent.EventSource.You)), 2, 1)
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory(new RuleBook(
                DefaultNumTurnsToCanAttack: 0));
            testCardFactory.SetCardPool(new[] { new CardSet("Test", new[] { testCardDef }) });

            // �ȉ��e�X�g
            var testGameMaster = new GameMaster(new GameMasterOptions(new RuleBook(), testCardFactory, new TestLogger(), null, (_, _) => { }));

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 1�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 2�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // ���ʂ��������Ȃ�
            Assert.Equal(0, testAction.CallCount);

            // 4�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // ���ʂ���������
                Assert.Equal(1, testAction.CallCount);
            });

            // ��U
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
            });

            // 3�^�[����
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                // 2�x�ڂ͌��ʂ��������Ȃ�
                Assert.Equal(1, testAction.CallCount);
            });
        }
    }
}
