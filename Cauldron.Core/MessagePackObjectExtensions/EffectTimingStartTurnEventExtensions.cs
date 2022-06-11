using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingStartTurnEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingStartTurnEvent _this,
            Card ownerCard, EffectEventArgs args)
        {
            if (args.SourcePlayer == null)
            {
                return false;
            }

            if (_this.OrPlayerConditions.Length == 0)
            {
                return true;
            }

            foreach (var cond in _this.OrPlayerConditions)
            {
                var matched = await cond.IsMatch(ownerCard, args, args.SourcePlayer);
                if (matched)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
