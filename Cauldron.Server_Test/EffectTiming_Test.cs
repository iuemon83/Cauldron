using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using System;
using System.Linq;
using Xunit;

namespace Cauldron.Server_Test
{
    /// <summary>
    /// カード効果発動タイミングのテスト
    /// </summary>
    public class EffectTiming_Test
    {
        private class TestEffectAction : IEffectAction
        {
            public int CallCount = 0;

            public Action<Card, EffectEventArgs> Action { get; set; }

            public (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs effectEventArgs)
            {
                this.CallCount++;

                this.Action?.Invoke(ownerCard, effectEventArgs);

                return (true, effectEventArgs);
            }
        }

        [Fact]
        public void すべてのターン開始時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField,
                            StartTurn: new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Both)
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
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
        public void 自分のターン開始時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Owner)),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void 相手のターン開始時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, StartTurn : new EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource.Other)),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void すべてのターン終了時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField,
                            EndTurn: new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Both)
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
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
        public void 自分のターン終了時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.You)),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(2, testAction.CallCount);
        }

        [Fact]
        public void 相手のターン終了時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, EndTurn : new EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource.Opponent)),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(0, testAction.CallCount);

            // 後攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);

            // 先行
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
            });
            Assert.Equal(1, testAction.CallCount);
        }

        [Fact]
        public void カードのプレイ時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, Play : new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.This)),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動する
                Assert.Equal(1, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動しない
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void 他のカードのプレイ時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 1, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, Play : new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.Other)),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 1, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先行
            // 効果カード出す
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);
                // プレイ時に効果が発動しない
                Assert.Equal(0, testAction.CallCount);

                TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // 他のカードのプレイ時には発動する
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void 戦闘開始前時_すべてのクリーチャー()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, BattleBefore : new EffectTimingBattleBeforeEvent(EffectTimingBattleBeforeEvent.EventSource.All, CardCondition : new CardCondition())),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                // プレイ時に効果が発動しない
                Assert.Equal(0, testAction.CallCount);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(2, testAction.CallCount);

                return (testCard, normal2);
            });

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(2, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, normal2.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(3, testAction.CallCount);
            });
        }

        [Fact]
        public void 戦闘開始前時_自分が攻撃()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, BattleBefore : new EffectTimingBattleBeforeEvent(EffectTimingBattleBeforeEvent.EventSource.Attack, CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 自分の攻撃時に発動する
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの攻撃時に発動しない
                Assert.Equal(1, testAction.CallCount);

                return testCard;
            });

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 自分の防御時に発動しない
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void 戦闘開始前時_自分が防御()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, BattleBefore : new EffectTimingBattleBeforeEvent(EffectTimingBattleBeforeEvent.EventSource.Guard, CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 自分の攻撃時に発動しない
                Assert.Equal(0, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの攻撃時に発動しない
                Assert.Equal(0, testAction.CallCount);

                return testCard;
            });

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 自分の防御時に発動する
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void 戦闘開始前時_他カードが攻撃()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, BattleBefore : new EffectTimingBattleBeforeEvent(EffectTimingBattleBeforeEvent.EventSource.Attack, CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 自分の攻撃時に発動しない
                Assert.Equal(0, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの攻撃時に発動する
                Assert.Equal(1, testAction.CallCount);

                return testCard;
            });
        }

        [Fact]
        public void 戦闘開始前時_他カードが防御()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, BattleBefore : new EffectTimingBattleBeforeEvent(EffectTimingBattleBeforeEvent.EventSource.Guard, CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 1);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var testCard = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // ほかカードの防御時に発動する
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                g.AttackToCreature(pId, normal2.Id, testCard.Id);
                // 自分の防御時に発動しない
                Assert.Equal(1, testAction.CallCount);

                return testCard;
            });
        }

        [Fact]
        public void 戦闘ダメージ前時_すべてのクリーチャー()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, DamageBefore : new EffectTimingDamageBeforeEvent(EffectTimingDamageBeforeEvent.EventSource.All, CardCondition : new CardCondition())),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 0);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(2, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(2, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(4, testAction.CallCount);

                return (testCard, normal2);
            });

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(4, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, normal2.Id);
                // 戦闘時に効果が発動する
                // 攻撃と防御で2回発動する
                Assert.Equal(6, testAction.CallCount);
            });
        }

        [Fact]
        public void 戦闘ダメージ前時_自分が攻撃()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField,
                            DamageBefore : new (EffectTimingDamageBeforeEvent.EventSource.DamageSource,
                                CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.This })),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 0);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの戦闘時に発動しない
                Assert.Equal(1, testAction.CallCount);

                return (testCard, normal2);
            });

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 攻撃されたときも発動する
                Assert.Equal(2, testAction.CallCount);
            });
        }

        [Fact]
        public void 戦闘ダメージ前時_ほかカードが防御()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 0,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(ZoneType.YouField, DamageBefore : new EffectTimingDamageBeforeEvent(EffectTimingDamageBeforeEvent.EventSource.Guard, CardCondition : new CardCondition() { Context = CardCondition.CardConditionContext.Others })),
                        new[]{ testAction }
                    )
                });

            var testNormalCardDef = CardDef.Creature(0, $"test.test2", "test2", "test2", 1, 5, 0);

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testNormalCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            var normal = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var normal = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);

                return normal;
            });

            // 後攻
            // 効果カード出す
            var (testCard, normal2) = TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                var testCard = TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                g.AttackToCreature(pId, testCard.Id, normal.Id);
                // 戦闘時に効果が発動する
                Assert.Equal(1, testAction.CallCount);

                var normal2 = TestUtil.NewCardAndPlayFromHand(g, pId, testNormalCardDef.Id);
                Assert.Equal(1, testAction.CallCount);
                g.AttackToCreature(pId, normal2.Id, normal.Id);
                // ほかカードの戦闘時に発動する
                // 攻撃と防御の2度発動する
                Assert.Equal(3, testAction.CallCount);

                return (testCard, normal2);
            });

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                Assert.Equal(3, testAction.CallCount);
                g.AttackToCreature(pId, normal.Id, testCard.Id);
                // 攻撃されたときも発動する
                Assert.Equal(4, testAction.CallCount);
            });
        }

        [Fact]
        public void 戦闘以外のダメージ前時_自分が防御()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            DamageBefore: new EffectTimingDamageBeforeEvent(
                                EffectTimingDamageBeforeEvent.EventSource.Guard,
                                CardCondition: new CardCondition()
                                {
                                    Context = CardCondition.CardConditionContext.This
                                }
                            )
                        ),
                        new[]{ testAction }
                    )
                });

            // クリーチャーに1ダメージの魔法
            var testSorceryDef = CardDef.Sorcery(0, $"test.test2", "test2", "test2",
                effects: new[]
                {
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouField,
                            Play: new EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource.This)
                        ),
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
                                            ZoneCondition = ZoneType.Field,
                                            TypeCondition = new CardTypeCondition(new[]{ CardType.Creature })
                                        },
                                    },
                                    Value = 1
                                }
                            }
                        }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef, testSorceryDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
            TestUtil.Turn(testGameMaster, (g, pId) =>
            {
                TestUtil.NewCardAndPlayFromHand(g, pId, testCardDef.Id);

                Assert.Equal(0, testAction.CallCount);
                TestUtil.NewCardAndPlayFromHand(g, pId, testSorceryDef.Id);
                Assert.Equal(1, testAction.CallCount);
            });
        }

        [Fact]
        public void 自分の破壊時()
        {
            var testAction = new TestEffectAction();
            var testCardDef = CardDef.Creature(0, $"test.test", "test", "test", 1, 5, 1,
                effects: new[]{
                    new CardEffect(
                        new EffectTiming(
                            ZoneType.YouCemetery,
                            Destroy: new (EffectTimingDestroyEvent.EventSource.This)
                        ),
                        new[]{ testAction }
                    )
                });

            var testCardFactory = new CardFactory();
            testCardFactory.SetCardPool(new[] { testCardDef });

            // 以下テスト
            var testGameMaster = new GameMaster(new RuleBook(), testCardFactory, new TestLogger(), null);

            var (_, player1Id) = testGameMaster.CreateNewPlayer("player1", Enumerable.Repeat(testCardDef.Id, 40));
            var (_, player2Id) = testGameMaster.CreateNewPlayer("player2", Enumerable.Repeat(testCardDef.Id, 40));

            testGameMaster.Start(player1Id);

            // 先攻
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
