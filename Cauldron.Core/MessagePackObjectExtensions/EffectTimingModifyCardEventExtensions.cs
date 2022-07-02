using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingModifyCardEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingModifyCardEvent _this,
            Card effectOwnerCard, EffectEventArgs effectArgs)
        {
            if (effectArgs.SourceCard == null || effectArgs.ModifyCardContext == null)
            {
                return false;
            }

            var modifyCardContext = effectArgs.ModifyCardContext;

            foreach (var cond in _this.OrCardConditions)
            {
                var matched = await cond.IsMatch(effectOwnerCard, effectArgs, effectArgs.SourceCard)
                    && (await (_this.ModifyPowerCondition?.IsMatch(modifyCardContext.DiffPower, effectOwnerCard, effectArgs)
                        ?? ValueTask.FromResult(true)))
                    && (await (_this.ModifyToughnessCondition?.IsMatch(modifyCardContext.DiffToughness, effectOwnerCard, effectArgs)
                        ?? ValueTask.FromResult(true)))
                        ;

                if (matched)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
