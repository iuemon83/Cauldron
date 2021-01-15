using Cauldron.Server.Models.Effect.Value;
using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public record ZoneCondition(ZoneValue Value, bool Not = false)
    {
        public bool IsMatch(Card effectOwnerCard, EffectEventArgs effectEventArgs, Zone checkValue)
        {
            var zones = this.Value.Calculate(effectOwnerCard, effectEventArgs);
            var opponentId = effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id;
            var isMatch = zones.Select(z => Zone.FromPrettyName(effectOwnerCard.OwnerId, opponentId, z))
                .Any(z => z == checkValue);

            return isMatch ^ this.Not;
        }
    }
}
