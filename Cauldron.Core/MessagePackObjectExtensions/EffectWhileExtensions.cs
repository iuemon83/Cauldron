using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectWhileExtensions
    {
        private static readonly Dictionary<(CardId, EffectWhile), int> effectWhileCounter = new();

        public static async ValueTask<bool> IsMatch(this EffectWhile effectWhile, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            Console.WriteLine($"{effectOwnerCard.Id}.{effectOwnerCard.Name}");

            var key = (effectOwnerCard.Id, effectWhile);

            if (!effectWhileCounter.ContainsKey(key))
            {
                effectWhileCounter.Add(key, 0);
            }

            if (await effectWhile.Timing.IsMatch(effectOwnerCard, eventArgs))
            {
                effectWhileCounter[key]++;
            }

            return (effectWhile.Skip == 0 || effectWhileCounter[key] > effectWhile.Skip)
                && effectWhileCounter[key] <= effectWhile.Take + effectWhile.Skip;
        }
    }
}
