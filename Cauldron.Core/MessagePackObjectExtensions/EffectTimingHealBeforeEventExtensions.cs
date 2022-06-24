using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingHealBeforeEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingHealBeforeEvent _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (eventArgs.HealContext == null)
            {
                return false;
            }

            if (eventArgs.HealContext.Value <= 0)
            {
                return false;
            }

            return await SourceCardIsMatch(_this, effectOwnerCard, eventArgs)
                && await TakeIsMatch(_this, effectOwnerCard, eventArgs);

            static async ValueTask<bool> SourceCardIsMatch(EffectTimingHealBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.SourceCardCondition == null)
                {
                    return true;
                }

                if (eventArgs.HealContext?.HealSourceCard == null)
                {
                    return false;
                }

                return await _this.SourceCardCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.HealContext.HealSourceCard);
            }

            static async ValueTask<bool> TakeIsMatch(EffectTimingHealBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
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

            static async ValueTask<bool> TakePlayerIsMatch(EffectTimingHealBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.TakePlayerCondition == null)
                {
                    return false;
                }

                if (eventArgs.HealContext?.TakePlayer == null)
                {
                    return false;
                }

                return await _this.TakePlayerCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.HealContext.TakePlayer);
            }

            static async ValueTask<bool> TakeCardIsMatch(EffectTimingHealBeforeEvent _this, Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (_this.TakeCardCondition == null)
                {
                    return false;
                }

                // プレイヤーかカードのどっちかの条件で合致させたいから
                if (eventArgs.HealContext?.TakeCard == null)
                {
                    return false;
                }

                return await _this.TakeCardCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.HealContext.TakeCard);
            }
        }
    }
}
