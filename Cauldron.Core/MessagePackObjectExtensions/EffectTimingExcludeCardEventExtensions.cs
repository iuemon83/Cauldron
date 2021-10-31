using Cauldron.Core.Entities.Effect;
using System.Linq;
using System.Threading.Tasks;

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
