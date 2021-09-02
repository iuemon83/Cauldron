using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCountersOfModifyCounterExtensions
    {
        public static int GetRsult(this ActionContextCountersOfModifyCounter _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            return args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value)
                ? value?.ActionModifyCounterContext?.GetCounters(_this.Type) ?? 0
                : 0;
        }
    }
}
