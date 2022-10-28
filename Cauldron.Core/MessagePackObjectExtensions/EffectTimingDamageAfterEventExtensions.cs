using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingDamageAfterEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingDamageAfterEvent _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (eventArgs.DamageContext == null)
            {
                return false;
            }

            if (_this.Type == EffectTimingDamageAfterEvent.TypeValue.Battle
                && eventArgs.DamageContext.Reason != DamageNotifyMessage.ReasonValue.Attack)
            {
                return false;
            }

            if (_this.Type == EffectTimingDamageAfterEvent.TypeValue.NonBattle
                && eventArgs.DamageContext.Reason == DamageNotifyMessage.ReasonValue.Attack)
            {
                return false;
            }

            if (eventArgs.DamageContext.Value <= 0)
            {
                return false;
            }

            return await EffectTimingDamageBeforeEventExtensions.SourceCardIsMatch(_this, effectOwnerCard, eventArgs)
                && await EffectTimingDamageBeforeEventExtensions.TakeIsMatch(_this, effectOwnerCard, eventArgs);
        }
    }
}
