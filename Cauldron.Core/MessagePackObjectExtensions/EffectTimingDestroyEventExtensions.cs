using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingDestroyEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingDestroyEvent _this,
            Card ownerCard, EffectEventArgs args)
        {
            if (args.SourceCard == null)
            {
                return false;
            }

            foreach (var cc in _this.OrCardCondition)
            {
                var matched = await cc.IsMatch(ownerCard, args, args.SourceCard);
                if (matched)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
