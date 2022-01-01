using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionModifyPlayerExecuter : IEffectActionExecuter
    {
        private readonly EffectActionModifyPlayer _this;

        public EffectActionModifyPlayerExecuter(EffectActionModifyPlayer _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.Choice, args);
            var targets = choiceResult.PlayerIdList;

            var done = false;
            foreach (var playerId in targets)
            {
                await args.GameMaster.ModifyPlayer(new(playerId, _this.PlayerModifier), effectOwnerCard, args);

                done = true;
            }

            return (done, args);
        }
    }
}
