using Cauldron.Shared.MessagePackObjects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{
    /// <summary>
    /// �_���[�W�O�C�x���g�̃e�X�g
    /// </summary>
    public class EffectTimingDamageAfterEvent_Test
    {
        [Fact]
        public async Task IsMatch_1�ȏ�̃_���[�W()
        {
            var test = new EffectTimingDamageAfterEvent();

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1
                    )
                ));

            Assert.True(actual);
        }

        [Fact]
        public async Task IsMatch_0�̃_���[�W()
        {
            var test = new EffectTimingDamageAfterEvent();

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        0
                    )
                ));

            Assert.False(actual);
        }

        [Fact]
        public async Task IsMatch_�퓬�_���[�W()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.Battle);

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1
                    )
                ));

            Assert.False(actual);

            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Attack,
                        default,
                        1
                    )
                ));

            Assert.True(actual2);
        }

        [Fact]
        public async Task IsMatch_�퓬�_���[�W�ȊO()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.NonBattle);

            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1
                    )
                ));

            Assert.True(actual);

            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Attack,
                        default,
                        1
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_�v���C���[�Ƀ_���[�W()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.Any,
                TakePlayerCondition: new(
                    PlayerCondition.ContextValue.Any)
                );

            // �v���C���[���_���[�W���󂯂��̂�true
            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1,
                        GuardPlayer: new(
                            PlayerId.NewId(),
                            "",
                            TestUtil.TestRuleBook(),
                            Array.Empty<Card>(),
                            default
                            )
                    )
                ));

            Assert.True(actual);

            // �N���[�`���[���_���[�W���󂯂��̂�false
            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_�N���[�`���[�Ƀ_���[�W()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.Any,
                TakeCardCondition: new(
                    CardCondition.ContextConditionValue.Any
                    )
                );

            // �v���C���[���_���[�W���󂯂��̂�false
            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1,
                        GuardPlayer: new(
                            PlayerId.NewId(),
                            "",
                            TestUtil.TestRuleBook(),
                            Array.Empty<Card>(),
                            default
                            )
                    )
                ));

            Assert.False(actual);

            // �N���[�`���[���_���[�W���󂯂��̂�true
            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual2);
        }

        [Fact]
        public async Task IsMatch_�v���C���[���N���[�`���[�Ƀ_���[�W()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.Any,
                TakePlayerCondition: new(
                    PlayerCondition.ContextValue.Any
                    ),
                TakeCardCondition: new(
                    CardCondition.ContextConditionValue.Any
                    )
                );

            // �v���C���[���_���[�W���󂯂��̂�true
            var actual = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1,
                        GuardPlayer: new(
                            PlayerId.NewId(),
                            "",
                            TestUtil.TestRuleBook(),
                            Array.Empty<Card>(),
                            default
                            )
                    )
                ));

            Assert.True(actual);

            // �N���[�`���[���_���[�W���󂯂��̂�true
            var actual2 = await test.IsMatch(
                default,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        default,
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual2);
        }

        [Fact]
        public async Task IsMatch_�������U��()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.Any,
                    SourceCardCondition: new CardCondition(
                        ContextCondition: CardCondition.ContextConditionValue.This
                        )
                    );

            var testCard = new Card(TestUtil.CardDef(""));

            // �������U�������̂�true
            var actual = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        testCard,
                        1,
                        GuardCard: new Card(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual);

            // �����ȊO���U�������̂�false
            var actual2 = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        new(TestUtil.CardDef("")),
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_�������h��()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.Any,
                TakeCardCondition: new CardCondition(
                    ContextCondition: CardCondition.ContextConditionValue.This
                )
                );

            var testCard = new Card(TestUtil.CardDef(""));

            // �������h�䂵���̂�true
            var actual = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        new Card(TestUtil.CardDef("")),
                        1,
                        GuardCard: testCard
                    )
                ));

            Assert.True(actual);

            // �����ȊO���h�䂵���̂�false
            var actual2 = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        new(TestUtil.CardDef("")),
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.False(actual2);
        }

        [Fact]
        public async Task IsMatch_�����h��()
        {
            var test = new EffectTimingDamageAfterEvent(
                EffectTimingDamageAfterEvent.TypeValue.Any,
                TakeCardCondition: new CardCondition(
                    ContextCondition: CardCondition.ContextConditionValue.Others
                    )
                );

            var testCard = new Card(TestUtil.CardDef(""));

            // �������h�䂵���̂�false
            var actual = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        new Card(TestUtil.CardDef("")),
                        1,
                        GuardCard: testCard
                    )
                ));

            Assert.False(actual);

            // �����ȊO���h�䂵���̂�true
            var actual2 = await test.IsMatch(
                testCard,
                new Core.Entities.Effect.EffectEventArgs(
                    Core.Entities.GameEvent.OnDamage,
                    default,
                    DamageContext: new(
                        DamageNotifyMessage.ReasonValue.Effect,
                        new(TestUtil.CardDef("")),
                        1,
                        GuardCard: new(TestUtil.CardDef(""))
                    )
                ));

            Assert.True(actual2);
        }
    }
}
