using Cauldron.Shared.MessagePackObjects;
using System;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// ダメージ前イベントのテスト
    /// </summary>
    public class EffectTimingEndTurnEvent_Test
    {
        [Fact]
        public void IsMatch_すべてのターン開始時()
        {
            var test = new EffectTimingEndTurnEvent();

            var actual = test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: new Core.Entities.Player(
                        PlayerId.NewId(), "", TestUtil.TestRuleBook, Array.Empty<Card>(), default)
                ));

            Assert.True(actual);
        }

        [Fact]
        public void IsMatch_あなたのターン開始時()
        {
            var testPlayer = new Core.Entities.Player(
                PlayerId.NewId(), "", TestUtil.TestRuleBook, Array.Empty<Card>(), default
                );

            var youCard = new Card(TestUtil.CardDef(""))
            {
                OwnerId = testPlayer.Id
            };

            var noYouCard = new Card(TestUtil.CardDef(""));

            var test = new EffectTimingEndTurnEvent(
                OrPlayerConditions: new[]
                {
                    new PlayerCondition(PlayerCondition.ContextValue.You)
                });

            // あなたのターン開始
            var actual = test.IsMatch(
                youCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: testPlayer
                    ));

            Assert.True(actual);

            // あなた以外のターン開始
            actual = test.IsMatch(
                noYouCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: testPlayer
                    ));

            Assert.False(actual);
        }

        [Fact]
        public void IsMatch_相手のターン開始時()
        {
            var testPlayer = new Core.Entities.Player(
                PlayerId.NewId(), "", TestUtil.TestRuleBook, Array.Empty<Card>(), default
                );

            var youCard = new Card(TestUtil.CardDef(""))
            {
                OwnerId = testPlayer.Id
            };

            var noYouCard = new Card(TestUtil.CardDef(""));

            var test = new EffectTimingEndTurnEvent(
                OrPlayerConditions: new[]
                {
                    new PlayerCondition(PlayerCondition.ContextValue.Opponent)
                });

            // あなたのターン開始
            var actual = test.IsMatch(
                youCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: testPlayer
                    ));

            Assert.False(actual);

            // 相手のターン開始
            actual = test.IsMatch(
                noYouCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: testPlayer
                    ));

            Assert.True(actual);
        }
    }
}
