using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectIfExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectIf effectIf, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var value = await effectIf.NumValue.Calculate(effectOwnerCard, eventArgs);
            return effectIf.NumCondition.IsMatch(value);
        }
    }
}
