using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardEffectExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> DoIfMatchedAnyZone(this CardEffect cardEffect, Card effectOwnerCard, EffectEventArgs args)
        {
            if (!await cardEffect.Condition.IsMatchAnyZone(effectOwnerCard, args)) return (false, args);

            var done = false;
            var newArgs = args;
            foreach (var action in cardEffect.Actions)
            {
                var (done2, newArgs2) = await action.Execute(effectOwnerCard, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }

        public static async ValueTask<(bool, EffectEventArgs)> DoIfMatched(this CardEffect cardEffect,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (!await cardEffect.Condition.IsMatch(effectOwnerCard, args)) return (false, args);

            var done = false;
            var newArgs = args;
            foreach (var action in cardEffect.Actions)
            {
                var (done2, newArgs2) = await action.Execute(effectOwnerCard, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }

        public static bool IsAnyZoneEffect(this CardEffect cardEffect)
        {
            return cardEffect.Condition.ZonePrettyName == Shared.ZonePrettyName.None;
        }
    }
}
