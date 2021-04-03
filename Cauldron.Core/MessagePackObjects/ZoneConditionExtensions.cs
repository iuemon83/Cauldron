using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ZoneConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this ZoneCondition zoneCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, Zone checkValue)
        {
            var zones = await zoneCondition.Value.Calculate(effectOwnerCard, effectEventArgs);
            var opponentId = effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id;
            var isMatch = zones.Select(z => z.FromPrettyName(effectOwnerCard.OwnerId, opponentId))
                .Any(z => z == checkValue);

            return isMatch ^ zoneCondition.Not;
        }
    }
}
