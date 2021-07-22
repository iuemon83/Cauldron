using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionModifyPlayerExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionModifyPlayer effectActionModifyPlayer, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, effectActionModifyPlayer.Choice, args);
            var targets = choiceResult.PlayerIdList;

            var done = false;
            foreach (var playerId in targets)
            {
                await args.GameMaster.ModifyPlayer(new(playerId, effectActionModifyPlayer.PlayerModifier), effectOwnerCard, args);

                done = true;
            }

            return (done, args);
        }
    }
}
