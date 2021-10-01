using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingStartTurnEventExtensions
    {
        public static bool IsMatch(this EffectTimingStartTurnEvent _this,
            Card ownerCard, EffectEventArgs args)
        {
            if (args.SourcePlayer == null)
            {
                return false;
            }

            if (_this.OrPlayerCondition.Length == 0)
            {
                return true;
            }

            foreach (var cond in _this.OrPlayerCondition)
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
