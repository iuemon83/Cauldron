using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// ダメージ前イベントのテスト
    /// </summary>
    public class EffectTimingPlayEvent_Test
    {
        [Fact]
        public async Task IsMatch_条件なし()
        {
            var test = new EffectTimingPlayEvent(Array.Empty<CardCondition>());

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnPlay,
                    default,
                    SourceCard: new Card(TestUtil.CardDef(""))
                ));

            Assert.False(actual);
        }

        [Fact]
        public async Task IsMatch_任意のカードをプレイ()
        {
            var test = new EffectTimingPlayEvent(new[]
            {
                new CardCondition(CardCondition.ContextConditionValue.Any)
            });

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnPlay,
                    default,
                    SourceCard: new Card(TestUtil.CardDef(""))
                ));

            Assert.True(actual);
        }

        [Fact]
        public async Task IsMatch_他のカードのプレイ時()
        {
            var test = new EffectTimingPlayEvent(new[]
            {
                new CardCondition(CardCondition.ContextConditionValue.Others)
            });

            var actual = await test.IsMatch(
                new Card(TestUtil.CardDef("")),
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnPlay,
                    default,
                    SourceCard: new Card(TestUtil.CardDef(""))
                ));

            Assert.True(actual);
        }
    }
}
