using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectWhileExtensions
    {
        private static readonly Dictionary<EffectWhile, int> effectWhileCounter = new();

        public static async ValueTask<bool> IsMatch(this EffectWhile effectWhile, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (!effectWhileCounter.ContainsKey(effectWhile))
            {
                effectWhileCounter.Add(effectWhile, 0);
            }

            if (await effectWhile.Timing.IsMatch(effectOwnerCard, eventArgs))
            {
                effectWhileCounter[effectWhile]++;
            }

            return (effectWhile.Skip == 0 || effectWhileCounter[effectWhile] > effectWhile.Skip)
                && effectWhileCounter[effectWhile] <= effectWhile.Take + effectWhile.Skip;
        }
    }
}
