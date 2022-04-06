using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// ダメージ前イベントのテスト
    /// </summary>
    public class EffectTimingDestroyEvent_Test
    {
        [Fact]
        public async Task IsMatch_条件なし()
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
        public async Task IsMatch_任意のカードが破壊()
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
        public async Task IsMatch_自分が破壊()
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
        public async Task IsMatch_他のカードが破壊()
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
