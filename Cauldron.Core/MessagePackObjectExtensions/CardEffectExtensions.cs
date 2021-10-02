using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardEffectExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> DoIfMatchedAnyZone(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (!await _this.Condition.IsMatchAnyZone(effectOwnerCard, args)) return (false, args);

            var done = false;
            var newArgs = args;
            foreach (var action in _this.Actions)
            {
                var (done2, newArgs2) = await action.Execute(effectOwnerCard, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }

        public static async ValueTask<(bool, EffectEventArgs)> DoIfMatchedOnPlay(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (!await _this.Condition.IsMatchOnPlay(effectOwnerCard, args)) return (false, args);

            var done = false;
            var newArgs = args;
            foreach (var action in _this.Actions)
            {
                var (done2, newArgs2) = await action.Execute(effectOwnerCard, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }

        public static async ValueTask<(bool, EffectEventArgs)> DoIfMatched(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (!await _this.Condition.IsMatch(effectOwnerCard, args)) return (false, args);

            var done = false;
            var newArgs = args;
            foreach (var action in _this.Actions)
            {
                var (done2, newArgs2) = await action.Execute(effectOwnerCard, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }

        public static bool ShouldRegisterEffect(this CardEffect _this)
        {
            return _this.Condition.Zone == ZonePrettyName.None
                && (_this.Condition.When != default
                    || _this.Condition.While != default);
        }
    }
}
