using Cauldron.Core.Entities.Effect;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingModifyCounterOnCardEventExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectTimingModifyCounterOnCardEvent _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (args.SourceCard == null
                || args.ModifyCounterContext == null)
            {
                return false;
            }

            // カウンターの条件に合致するか
            var isCounterMatched = _this.Countername == args.ModifyCounterContext.CounterName
                && _this.Operator == args.ModifyCounterContext.Operator;

            if (!isCounterMatched)
            {
                return false;
            }

            // カードの条件に合致するか
            foreach (var con in _this.OrCardConditions.Where(c => c != null))
            {
                var isCardMatched = await con.IsMatch(args.SourceCard, effectOwnerCard, args);
                if (isCardMatched)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
