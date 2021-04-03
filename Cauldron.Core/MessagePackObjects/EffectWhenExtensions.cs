using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectWhenExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectWhen effectWhen, Card effectOwnerCard, EffectEventArgs eventArgs)
            => await effectWhen.Timing.IsMatch(effectOwnerCard, eventArgs);
    }
}
