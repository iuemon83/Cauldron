using Cauldron.Core.Entities.Effect;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueCalculatorForCounterExtensions
    {
        public static async ValueTask<int> Calculate(this NumValueCalculatorForCounter _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var countFromChoice = await CalculateCount(_this, effectOwnerCard, effectEventArgs);

            return countFromChoice + CalculateFromActionContext(_this, effectOwnerCard, effectEventArgs);

            static async ValueTask<int> CalculateCount(NumValueCalculatorForCounter _this,
                Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                if (_this.TargetChoice == default
                    || _this.CounterName == default)
                {
                    return 0;
                }

                var picked = await effectEventArgs.GameMaster
                    .Choice(effectOwnerCard, _this.TargetChoice, effectEventArgs);

                var numPlayerCounters = picked.PlayerIdList
                    .Sum(pid => effectEventArgs.GameMaster.Get(pid)?.GetCounter(_this.CounterName) ?? 0);

                var numCardCounters = picked.CardList
                    .Sum(c => c.GetCounter(_this.CounterName));

                return numPlayerCounters + numCardCounters;
            }

            static int CalculateFromActionContext(NumValueCalculatorForCounter _this,
                Card effectOwnerCard, EffectEventArgs effectEventArgs)
            {
                if (_this.ActionContextCounters is not { } actionContext)
                {
                    return 0;
                }

                if (actionContext.OfModifyCounter is { } c)
                {
                    return c.GetRsult(effectOwnerCard, effectEventArgs);
                }

                return 0;
            }
        }
    }
}
