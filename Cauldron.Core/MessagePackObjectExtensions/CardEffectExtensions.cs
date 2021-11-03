using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardEffectExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> DoIfMatchedAnyZone(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (_this.Condition?.ByNotPlay == default)
            {
                return (false, args);
            }

            if (!await _this.Condition.ByNotPlay.IsMatchAnyZone(effectOwnerCard, args)) return (false, args);

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

            return await _this.Condition.ByPlay.IsMatchByPlaying(effectOwnerCard, args);
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

        public static bool ShouldRegisterEffect(this CardEffect _this)
        {
            return
                _this.Condition?.ByNotPlay != default
                && _this.Condition?.ByNotPlay.Zone == ZonePrettyName.None
                && (_this.Condition?.ByNotPlay.When != default
                    || _this.Condition?.ByNotPlay.While != default);
        }
    }
}
