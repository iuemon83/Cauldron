using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingExcludeCardEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingExcludeCardEvent _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (args.SourceCard == null)
            {
                return false;
            }

            if (args.ExcludeCardContext == null)
            {
                return false;
            }

            // sourcecard のzone はすでにexcluded になっているので参照しない
            var isMatchedZone = await (_this.FromZoneCondition?.IsMatch(effectOwnerCard, args, args.ExcludeCardContext.From)
                ?? ValueTask.FromResult(true));

            if (!isMatchedZone)
            {
                return false;
            }

            foreach (var con in _this.OrCardConditions.Where(c => c != null))
            {
                var isMatched = await con.IsMatch(effectOwnerCard, args, args.SourceCard);
                if (isMatched)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
