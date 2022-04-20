using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingModifyPlayerEventExtensions
    {
        public static bool IsMatch(this EffectTimingModifyPlayerEvent _this,
            Card ownerCard, EffectEventArgs args)
        {
            if (args.SourcePlayer == null)
            {
                return false;
            }

            foreach (var cond in _this.OrPlayerConditions)
            {
                var matched = cond.IsMatch(ownerCard, args, args.SourcePlayer);
                if (matched)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
