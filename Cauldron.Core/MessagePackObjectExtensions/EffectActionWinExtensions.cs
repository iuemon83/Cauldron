using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionWinExecuter : IEffectActionExecuter
    {
        private readonly EffectActionWin _this;

        public EffectActionWinExecuter(EffectActionWin _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
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
