using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ActionContextPlayersOfChoiceExtensions
    {
        public static IEnumerable<Player> GetRsult(this ActionContextPlayersOfChoice _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            if (!args.GameMaster.TryGetActionContext(effectOwnerCard.Id, _this.ActionName, out var value))
            {
                return Array.Empty<Player>();
            }

            var playerIdList = value?.Choice?.GetPlayers(_this.Type) ?? Array.Empty<PlayerId>();

            return playerIdList
                .Select(cid => args.GameMaster.TryGet(cid))
                .Where(x => x.Exists)
                .Select(x => x.Player);
        }
    }
}
