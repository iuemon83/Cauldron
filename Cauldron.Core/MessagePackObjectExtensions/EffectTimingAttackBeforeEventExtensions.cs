using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

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

            if (_this.AttackCardCondition != null)
            {
                return eventArgs.BattleContext.AttackCard != null
                    && await _this.AttackCardCondition.IsMatch(eventArgs.BattleContext.AttackCard, effectOwnerCard, eventArgs);
            }

            if (_this.GuardCardCondition != null)
            {
                return eventArgs.BattleContext.GuardCard != null
                    && await _this.GuardCardCondition.IsMatch(eventArgs.BattleContext.GuardCard, effectOwnerCard, eventArgs);
            }

            return true;
        }
    }
}
