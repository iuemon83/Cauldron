using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// �_���[�W�O�C�x���g�̃e�X�g
    /// </summary>
    public class EffectTimingEndTurnEvent_Test
    {
        [Fact]
        public async Task IsMatch_���ׂẴ^�[���J�n��()
        {
            var test = new EffectTimingEndTurnEvent();

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: new Core.Entities.Player(
                        PlayerId.NewId(), "", TestUtil.TestRuleBook(), Array.Empty<Card>(), default)
                ));

            Assert.True(actual);
        }

        [Fact]
        public async Task IsMatch_���Ȃ��̃^�[���J�n��()
        {
            var testPlayer = new Core.Entities.Player(
                PlayerId.NewId(), "", TestUtil.TestRuleBook(), Array.Empty<Card>(), default
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

            // ���Ȃ��̃^�[���J�n
            var actual = await test.IsMatch(
                youCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: testPlayer
                    ));

            Assert.True(actual);

            // ���Ȃ��ȊO�̃^�[���J�n
            actual = await test.IsMatch(
                noYouCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: testPlayer
                    ));

            Assert.False(actual);
        }

        [Fact]
        public async Task IsMatch_����̃^�[���J�n��()
        {
            var testPlayer = new Core.Entities.Player(
                PlayerId.NewId(), "", TestUtil.TestRuleBook(), Array.Empty<Card>(), default
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

            // ���Ȃ��̃^�[���J�n
            var actual = await test.IsMatch(
                youCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnEndTurn,
                    default,
                    SourcePlayer: testPlayer
                    ));

            Assert.False(actual);

            // ����̃^�[���J�n
            actual = await test.IsMatch(
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
