using Cauldron.Server.Models.Effect;
using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public record CardEffect(EffectTiming Timing, IReadOnlyList<EffectAction> Actions)
    {
        public (bool, EffectEventArgs) DoIfMatched(Card effectOwnerCard, EffectEventArgs args)
        {
            if (!this.Timing.IsMatch(effectOwnerCard, args)) return (false, args);

            var done = false;
            var newArgs = args;
            foreach (var action in this.Actions)
            {
                var (done2, newArgs2) = action.Execute(effectOwnerCard, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }
    }
}
