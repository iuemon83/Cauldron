using Cauldron.Core.Entities.Effect;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingPlayEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingPlayEvent _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (args.SourceCard == null)
            {
                return false;
            }

            foreach (var cond in _this.OrCardConditions.Where(c => c != null))
            {
                var isMatched = await cond.IsMatch(effectOwnerCard, args, args.SourceCard);
                if (isMatched)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
