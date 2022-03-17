using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;

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
            var targets = args.GameMaster.playerRepository.TryList(choiceResult.PlayerIdList).ToArray();

            var done = false;
            foreach (var player in targets)
            {
                var newArgs = args with
                {
                    ActionTargetPlayers = targets,
                    ActionTargetPlayer = player
                };

                await args.GameMaster.ModifyPlayer(new(player.Id, _this.PlayerModifier), effectOwnerCard, args);

                done = true;
            }

            return (done, args);
        }
    }
}
