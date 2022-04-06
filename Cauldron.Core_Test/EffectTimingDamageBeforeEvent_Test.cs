using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// ダメージ前イベントのテスト
    /// </summary>
    public class EffectTimingDamageBeforeEvent_Test
    {
        [Fact]
        public async Task IsMatch_1以上のダメージ()
        {
            var test = new EffectTimingDamageBeforeEvent();

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1
                    )
                ));

            Assert.True(actual);
        }

        [Fact]
        public async Task IsMatch_0のダメージ()
        {
            var test = new EffectTimingDamageBeforeEvent();

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        0
                    )
                ));

            Assert.False(actual);
        }

        [Fact]
        public async Task IsMatch_戦闘ダメージ()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.Battle);

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        IsBattle: false
                    )
                ));

            Assert.False(actual);

            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        IsBattle: true
                    )
                ));

            Assert.True(actual2);
        }

        [Fact]
        public async Task IsMatch_戦闘ダメージ以外()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.NonBattle);

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        IsBattle: false
                    )
                ));

            Assert.True(actual);

            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        IsBattle: true
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_プレイヤーにダメージ()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.Any,
                TakePlayerCondition: new(
                    PlayerCondition.ContextValue.Any)
                );

            // プレイヤーがダメージを受けたのでtrue
            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        GuardPlayer: new(
                            PlayerId.NewId(),
                            "",
                            TestUtil.TestRuleBook,
                            Array.Empty<Card>(),
                            default
                            )
                    )
                ));

            Assert.True(actual);

            // クリーチャーがダメージを受けたのでfalse
            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_クリーチャーにダメージ()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.Any,
                TakeCardCondition: new(
                    CardCondition.ContextConditionValue.Any
                    )
                );

            // プレイヤーがダメージを受けたのでfalse
            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        GuardPlayer: new(
                            PlayerId.NewId(),
                            "",
                            TestUtil.TestRuleBook,
                            Array.Empty<Card>(),
                            default
                            )
                    )
                ));

            Assert.False(actual);

            // クリーチャーがダメージを受けたのでtrue
            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual2);
        }

        [Fact]
        public async Task IsMatch_プレイヤーかクリーチャーにダメージ()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.Any,
                TakePlayerCondition: new(
                    PlayerCondition.ContextValue.Any
                    ),
                TakeCardCondition: new(
                    CardCondition.ContextConditionValue.Any
                    )
                );

            // プレイヤーがダメージを受けたのでtrue
            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        GuardPlayer: new(
                            PlayerId.NewId(),
                            "",
                            TestUtil.TestRuleBook,
                            Array.Empty<Card>(),
                            default
                            )
                    )
                ));

            Assert.True(actual);

            // クリーチャーがダメージを受けたのでtrue
            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        default,
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual2);
        }

        [Fact]
        public async Task IsMatch_自分が攻撃()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.Any,
                    SourceCardCondition: new CardCondition(
                        ContextCondition: CardCondition.ContextConditionValue.This
                        )
                    );

            var testCard = new Card(TestUtil.CardDef(""));

            // 自分が攻撃したのでtrue
            var actual = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        testCard,
                        1,
                        GuardCard: new Card(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual);

            // 自分以外が攻撃したのでfalse
            var actual2 = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        new(TestUtil.CardDef("")),
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_自分が防御()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.Any,
                TakeCardCondition: new CardCondition(
                    ContextCondition: CardCondition.ContextConditionValue.This
                )
                );

            var testCard = new Card(TestUtil.CardDef(""));

            // 自分が防御したのでtrue
            var actual = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        new Card(TestUtil.CardDef("")),
                        1,
                        GuardCard: testCard
                    )
                ));

            Assert.True(actual);

            // 自分以外が防御したのでfalse
            var actual2 = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        new(TestUtil.CardDef("")),
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_他が防御()
        {
            var test = new EffectTimingDamageBeforeEvent(
                EffectTimingDamageBeforeEvent.TypeValue.Any,
                TakeCardCondition: new CardCondition(
                    ContextCondition: CardCondition.ContextConditionValue.Others
                    )
                );

            var testCard = new Card(TestUtil.CardDef(""));

            // 自分が防御したのでfalse
            var actual = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        new Card(TestUtil.CardDef("")),
                        1,
                        GuardCard: testCard
                    )
                ));

            Assert.False(actual);

            // 自分以外が防御したのでtrue
            var actual2 = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamageBefore,
                    default,
                    DamageContext: new(
                        new(TestUtil.CardDef("")),
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual2);
        }
    }
}
