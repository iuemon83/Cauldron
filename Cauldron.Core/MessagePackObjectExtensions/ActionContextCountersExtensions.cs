using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextCountersExtensions
    {
        public static int GetCards(this ActionContextCounters _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (_this?.OfModifyCounter != null)
            {
                return _this.OfModifyCounter.GetRsult(effectOwnerCard, eventArgs);
            }
            else
            {
                return 0;
            }
        }
    }
}
