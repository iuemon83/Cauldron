using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionWinExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionWin _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.ChoicePlayers, args);
            var targets = choiceResult.PlayerIdList;

            var done = false;
            foreach (var playerId in targets)
            {
                args.GameMaster.Win(playerId);

                done = true;
            }

            return (done, args);
        }
    }
}
