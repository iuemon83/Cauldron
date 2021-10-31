using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectIfExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectIf _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return await _this.Condition.IsMatch(effectOwnerCard, eventArgs);
        }
    }
}
