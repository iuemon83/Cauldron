using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextPlayersExtensions
    {
        public static IEnumerable<Player> GetPlayers(this ActionContextPlayers _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (_this?.Choice != null)
            {
                return _this.Choice.GetRsult(effectOwnerCard, eventArgs);
            }
            else
            {
                return Array.Empty<Player>();
            }
        }
    }
}
