using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionReserveEffectExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionReserveEffect _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            args.GameMaster.ReserveEffect(effectOwnerCard, _this.EffectsToReserve);

            return (true, args);
        }
    }
}
