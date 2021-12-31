using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardEffectExtensions
    {
        public static bool IsByNotPlay(this CardEffect _this)
        {
            var condition = _this.Condition?.ByNotPlay;
            if (condition == default)
            {
                return false;
            }

            return condition.Zone != ZonePrettyName.None
                && condition.When != default;
        }

        public static async ValueTask<(bool, EffectEventArgs)> DoReservedEffectIfMatched(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var condition = _this.Condition?.ByNotPlay
                ?? (EffectCondition)_this.Condition?.Reserve;

            if (condition == null)
            {
                return (false, args);
            }

            if (!await condition.IsMatch(effectOwnerCard, args)) return (false, args);

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

        public static async ValueTask<bool> IsMatchedByPlaying(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (_this.Condition?.ByPlay == default)
            {
                return false;
            }

            return await _this.Condition.ByPlay.IsMatch(effectOwnerCard, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> DoActionByPlaying(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
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
            if (_this.Condition?.ByNotPlay == default)
            {
                return (false, args);
            }

            if (!await _this.Condition.ByNotPlay.IsMatch(effectOwnerCard, args)) return (false, args);

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
    }
}
