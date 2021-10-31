using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ConditionWrapExtensions
    {
        public static async ValueTask<bool> IsMatch(this ConditionWrap _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (_this.ConditionAnd != default)
            {
                return await _this.ConditionAnd.IsMatch(effectOwnerCard, eventArgs);
            }

            if (_this.ConditionOr != default)
            {
                return await _this.ConditionOr.IsMatch(effectOwnerCard, eventArgs);
            }

            if (_this.ConditionNot != default)
            {
                return await _this.ConditionNot.IsMatch(effectOwnerCard, eventArgs);
            }

            if (_this.PlayerExistsCondition != default)
            {
                return eventArgs.GameMaster.Exists(_this.PlayerExistsCondition, effectOwnerCard, eventArgs);
            }

            if (_this.NumCondition != default)
            {
                return await _this.NumCondition.IsMatch(effectOwnerCard, eventArgs);
            }

            if (_this.TextCondition != default)
            {
                return await _this.TextCondition.IsMatch(effectOwnerCard, eventArgs);
            }

            return true;
        }
    }
}
