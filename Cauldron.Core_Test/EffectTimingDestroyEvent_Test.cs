using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// �_���[�W�O�C�x���g�̃e�X�g
    /// </summary>
    public class EffectTimingDestroyEvent_Test
    {
        [Fact]
        public async Task IsMatch_�����Ȃ�()
        {
            var test = new EffectTimingDestroyEvent(Array.Empty<CardCondition>());

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDestroy,
                    default,
                    SourceCard: new Card(TestUtil.CardDef(""))
                ));

            Assert.False(actual);
        }

        [Fact]
        public async Task IsMatch_�C�ӂ̃J�[�h���j��()
        {
            var test = new EffectTimingPlayEvent(new[]
            {
                new CardCondition(CardCondition.ContextConditionValue.Any)
            });

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDestroy,
                    default,
                    SourceCard: new Card(TestUtil.CardDef(""))
                ));

            Assert.True(actual);
        }

        [Fact]
        public async Task IsMatch_�������j��()
        {
            var test = new EffectTimingPlayEvent(new[]
            {
                new CardCondition(CardCondition.ContextConditionValue.This)
            });

            var testCard = new Card(TestUtil.CardDef(""));

            var actual = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDestroy,
                    default,
                    SourceCard: testCard
                ));

            Assert.True(actual);
        }

        [Fact]
        public async Task IsMatch_���̃J�[�h���j��()
        {
            var test = new EffectTimingPlayEvent(new[]
            {
                new CardCondition(CardCondition.ContextConditionValue.Others)
            });

            var actual = await test.IsMatch(
                new Card(TestUtil.CardDef("")),
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDestroy,
                    default,
                    SourceCard: new Card(TestUtil.CardDef(""))
                ));

            Assert.True(actual);
        }
    }
}
