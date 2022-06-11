using Cauldron.Core.Entities.Effect;

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
                && eventArgs.DamageContext.Reason != DamageNotifyMessage.ReasonValue.Attack)
            {
                return false;
            }

            if (_this.Type == EffectTimingDamageBeforeEvent.TypeValue.NonBattle
                && eventArgs.DamageContext.Reason == DamageNotifyMessage.ReasonValue.Attack)
            {
                return false;
            }

            if (eventArgs.DamageContext.Value <= 0)
            {
                return false;
            }

            return await SourceCardIsMatch(_this, effectOwnerCard, eventArgs)
                && await TakeIsMatch(_this, effectOwnerCard, eventArgs);

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

            static async ValueTask<bool> TakeIsMatch(EffectTimingDamageBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                // プレイヤーの条件とカードの条件が両方とも未指定なら素通し
                if (_this.TakePlayerCondition == null
                    && _this.TakeCardCondition == null)
                {
                    return true;
                }

                // どちらかでも指定されているなら、指定されているほうの条件には合致しないとダメ
                return await TakePlayerIsMatch(_this, effectOwnerCard, eventArgs)
                    || await TakeCardIsMatch(_this, effectOwnerCard, eventArgs);
            }

            static async ValueTask<bool> TakePlayerIsMatch(EffectTimingDamageBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.TakePlayerCondition == null)
                {
                    return false;
                }

                if (eventArgs.DamageContext?.GuardPlayer == null)
                {
                    return false;
                }

                return await _this.TakePlayerCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardPlayer);
            }

            static async ValueTask<bool> TakeCardIsMatch(EffectTimingDamageBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.TakeCardCondition == null)
                {
                    return false;
                }

                // プレイヤーかカードのどっちかの条件で合致させたいから
                if (eventArgs.DamageContext?.GuardCard == null)
                {
                    return false;
                }

                return await _this.TakeCardCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.DamageContext.GuardCard);
            }
        }
    }
}
