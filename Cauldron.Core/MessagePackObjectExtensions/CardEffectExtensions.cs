using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardEffectExtensions
    {
        public static async ValueTask<bool> IsMatchedByPlaying(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (_this.Condition?.ByPlay == default)
            {
                return false;
            }

            return await _this.Condition.ByPlay.IsMatch(effectOwnerCard, args);
        }

        public static async ValueTask<bool> IsMatched(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var condition = _this.Condition.ByNotPlay
                ?? (EffectCondition?)_this.Condition.Reserve;

            if (condition == null)
            {
                return false;
            }

            return await condition.IsMatch(effectOwnerCard, args);
        }

        public static async ValueTask<(bool, EffectEventArgs)> DoAction(this CardEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var done = false;
            var newArgs = args;
            foreach (var action in _this.Actions)
            {
                var (done2, newArgs2) = await action.Execute(effectOwnerCard, _this.Id, newArgs);

                done = done || done2;
                newArgs = newArgs2;
            }

            return (done, newArgs);
        }
    }
}
