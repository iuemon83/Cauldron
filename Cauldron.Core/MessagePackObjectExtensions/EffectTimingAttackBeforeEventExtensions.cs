#nullable enable

using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingAttackBeforeEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingAttackBeforeEvent _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (eventArgs.BattleContext == null)
            {
                return false;
            }

            return await IsMatchedAttackCardCondition(_this.AttackCardCondition, effectOwnerCard, eventArgs)
                && await IsMatchedGuardCardCondition(_this.GuardCardCondition, effectOwnerCard, eventArgs);

            static async ValueTask<bool> IsMatchedAttackCardCondition(CardCondition? attackCardCondition,
                Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (attackCardCondition == null)
                {
                    return true;
                }

                return eventArgs.BattleContext?.AttackCard != null
                    && await attackCardCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.BattleContext.AttackCard);
            }

            static async ValueTask<bool> IsMatchedGuardCardCondition(CardCondition? guardCardCondition,
                Card effectOwnerCard, EffectEventArgs eventArgs)
            {
                if (guardCardCondition == null)
                {
                    return true;
                }

                return eventArgs.BattleContext?.GuardCard != null
                    && await guardCardCondition.IsMatch(effectOwnerCard, eventArgs, eventArgs.BattleContext.GuardCard);
            }
        }
    }
}
