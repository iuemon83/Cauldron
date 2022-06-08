using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingModifyPlayerEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingModifyPlayerEvent _this,
            Card effectOwnerCard, EffectEventArgs effectArgs)
        {
            if (effectArgs.SourcePlayer == null || effectArgs.ModifyPlayerContext == null)
            {
                return false;
            }

            var modPlayerContext = effectArgs.ModifyPlayerContext;

            foreach (var cond in _this.OrPlayerConditions)
            {
                var matched = cond.IsMatch(effectOwnerCard, effectArgs, effectArgs.SourcePlayer)
                    && (await (_this.ModifyMaxHpCondition?.IsMatch(modPlayerContext.DiffMaxHp, effectOwnerCard, effectArgs)
                        ?? ValueTask.FromResult(true)))
                    && (await (_this.ModifyCurrentHpCondition?.IsMatch(modPlayerContext.DiffCurrentHp, effectOwnerCard, effectArgs)
                        ?? ValueTask.FromResult(true)))
                    && (await (_this.ModifyMaxMpCondition?.IsMatch(modPlayerContext.DiffMaxMp, effectOwnerCard, effectArgs)
                        ?? ValueTask.FromResult(true)))
                    && (await (_this.ModifyCurrentMpCondition?.IsMatch(modPlayerContext.DiffCurrentMp, effectOwnerCard, effectArgs)
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
