using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingDamageBeforeEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingDamageBeforeEvent _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (eventArgs.DamageContext == null)
            {
                return false;
            }

            if (_this.Type == EffectTimingDamageBeforeEvent.TypeValue.Battle
                && !eventArgs.DamageContext.IsBattle)
            {
                return false;
            }

            if (eventArgs.DamageContext.Value <= 0)
            {
                return false;
            }

            return TakePlayerIsMatch(_this, effectOwnerCard, eventArgs)
                && await SourceCardIsMatch(_this, effectOwnerCard, eventArgs)
                && await TakeCardIsMatch(_this, effectOwnerCard, eventArgs);

            static bool TakePlayerIsMatch(EffectTimingDamageBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.TakePlayerCondition == null)
                {
                    return true;
                }

                if (eventArgs.DamageContext?.GuardPlayer == null)
                {
                    return false;
                }

                return _this.TakePlayerCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardPlayer);
            }

            static async ValueTask<bool> SourceCardIsMatch(EffectTimingDamageBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.SourceCardCondition == null)
                {
                    return true;
                }

                if (eventArgs.DamageContext?.DamageSourceCard == null)
                {
                    return false;
                }

                return await _this.SourceCardCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.DamageSourceCard);
            }

            static async ValueTask<bool> TakeCardIsMatch(EffectTimingDamageBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.TakeCardCondition == null)
                {
                    return true;
                }

                if (eventArgs.DamageContext?.GuardCard == null)
                {
                    return false;
                }

                return await _this.TakeCardCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardCard);
            }
        }
    }
}
